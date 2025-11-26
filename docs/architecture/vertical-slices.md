# Vertical Slice Architecture

## Overview

PoOmad uses **Vertical Slice Architecture** to organize code by feature rather than technical layer. Each feature slice contains all the layers it needs (API, business logic, data access) in a single cohesive unit.

## Benefits

- **Feature Isolation**: Each slice can be developed, tested, and deployed independently
- **Reduced Coupling**: Features don't share business logic across slices
- **Easier Navigation**: All code for a feature lives in one place
- **Parallel Development**: Teams can work on different slices simultaneously
- **Clear Boundaries**: CQRS with MediatR enforces request/response contracts

## Feature Slices

```
src/PoOmad.Api/Features/
├── Authentication/
│   ├── GoogleAuthEndpoints.cs      # Minimal API routes
│   └── ... (OAuth flow handlers)
├── Profile/
│   ├── ProfileEndpoints.cs         # Minimal API routes
│   ├── UserProfile.cs              # Table Storage entity
│   ├── CreateProfile.cs            # MediatR command handler
│   ├── GetProfile.cs               # MediatR query handler
│   └── UpdateProfile.cs            # MediatR command handler
├── DailyLogs/
│   ├── DailyLogsEndpoints.cs       # Minimal API routes
│   ├── DailyLogEntry.cs            # Table Storage entity
│   ├── LogDay.cs                   # MediatR command handler
│   ├── GetDayLog.cs                # MediatR query handler
│   ├── GetMonthlyLogs.cs           # MediatR query handler
│   ├── CalculateStreak.cs          # MediatR query handler
│   └── DeleteDayLog.cs             # MediatR command handler
└── Analytics/
    ├── AnalyticsEndpoints.cs       # Minimal API routes
    ├── GetTrends.cs                # MediatR query handler
    └── GetCorrelation.cs           # MediatR query handler
```

## Anatomy of a Slice

Each feature slice follows this pattern:

### 1. DTOs (Shared Project)

```csharp
// src/PoOmad.Shared/DTOs/DailyLogDto.cs
public record DailyLogDto
{
    public DateOnly Date { get; init; }
    public bool OmadCompliant { get; init; }
    public bool AlcoholConsumed { get; init; }
    public decimal Weight { get; init; }
    public DateTimeOffset ServerTimestamp { get; init; }
}
```

### 2. Validators (Shared Project)

```csharp
// src/PoOmad.Shared/Validators/DailyLogValidator.cs
public class DailyLogValidator : AbstractValidator<DailyLogDto>
{
    public DailyLogValidator()
    {
        RuleFor(x => x.Date).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Weight).InclusiveBetween(50, 500);
    }
}
```

### 3. Table Storage Entity (API Project)

```csharp
// src/PoOmad.Api/Features/DailyLogs/DailyLogEntry.cs
public class DailyLogEntry : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // GoogleId
    public string RowKey { get; set; } = string.Empty;       // yyyy-MM-dd
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public bool OmadCompliant { get; set; }
    public bool AlcoholConsumed { get; set; }
    public decimal Weight { get; set; }
    public DateTimeOffset ServerTimestamp { get; set; }
}
```

### 4. MediatR Handlers (API Project)

```csharp
// src/PoOmad.Api/Features/DailyLogs/LogDay.cs
public record LogDayCommand(string GoogleId, DailyLogDto Log) : IRequest<Result<DailyLogDto>>;

public class LogDayHandler : IRequestHandler<LogDayCommand, Result<DailyLogDto>>
{
    private readonly TableClient _tableClient;
    
    public LogDayHandler(TableClient tableClient)
    {
        _tableClient = tableClient;
    }
    
    public async Task<Result<DailyLogDto>> Handle(LogDayCommand request, CancellationToken ct)
    {
        // Business logic here
        var entity = new DailyLogEntry
        {
            PartitionKey = request.GoogleId,
            RowKey = request.Log.Date.ToString("yyyy-MM-dd"),
            OmadCompliant = request.Log.OmadCompliant,
            AlcoholConsumed = request.Log.AlcoholConsumed,
            Weight = request.Log.Weight,
            ServerTimestamp = DateTimeOffset.UtcNow
        };
        
        await _tableClient.UpsertEntityAsync(entity, cancellationToken: ct);
        
        return Result.Ok(/* map entity to dto */);
    }
}
```

### 5. Minimal API Endpoints (API Project)

```csharp
// src/PoOmad.Api/Features/DailyLogs/DailyLogsEndpoints.cs
public static class DailyLogsEndpoints
{
    public static IEndpointRouteBuilder MapDailyLogsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily-logs")
            .RequireAuthorization()
            .WithTags("Daily Logs");
        
        group.MapPost("/", async (DailyLogDto dto, IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();
            
            var command = new LogDayCommand(googleId, dto);
            var result = await mediator.Send(command);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.BadRequest(result.Error);
        })
        .WithName("LogDay");
        
        return app;
    }
}
```

### 6. Blazor Components (Client Project)

```csharp
// src/PoOmad.Client/Components/DailyLogModal.razor
@inject ApiClient Api

<RadzenDialog>
    <EditForm Model="@_log" OnValidSubmit="@HandleSubmit">
        <FluentValidationValidator ValidatorType="typeof(DailyLogValidator)" />
        
        <RadzenFormField Text="OMAD Compliant?">
            <RadzenSwitch @bind-Value="_log.OmadCompliant" />
        </RadzenFormField>
        
        <RadzenFormField Text="Alcohol Consumed?">
            <RadzenSwitch @bind-Value="_log.AlcoholConsumed" />
        </RadzenFormField>
        
        <RadzenFormField Text="Weight (lbs)">
            <RadzenNumeric @bind-Value="_log.Weight" />
        </RadzenFormField>
        
        <RadzenButton ButtonType="ButtonType.Submit" Text="Save" />
    </EditForm>
</RadzenDialog>

@code {
    private DailyLogDto _log = new();
    
    private async Task HandleSubmit()
    {
        await Api.PostAsync("/api/daily-logs", _log);
        // Close modal and refresh parent
    }
}
```

## Request Flow

1. **User Action**: Click calendar cell in Blazor UI
2. **Component Event**: `DailyLogModal.razor` calls `ApiClient.PostAsync()`
3. **HTTP Request**: POST to `/api/daily-logs`
4. **Minimal API Route**: `DailyLogsEndpoints.MapPost()` receives request
5. **Authentication Check**: Extract GoogleId from ClaimsPrincipal
6. **Command Creation**: Map DTO to `LogDayCommand`
7. **MediatR Pipeline**: Send command through validation behavior
8. **Handler Execution**: `LogDayHandler.Handle()` executes business logic
9. **Data Access**: Upsert entity to Azure Table Storage
10. **Response**: Return `Result<DailyLogDto>` to caller
11. **UI Update**: Component refreshes and closes modal

## CQRS Pattern

We use **MediatR** to implement CQRS (Command Query Responsibility Segregation):

- **Commands**: Mutate state (Create, Update, Delete)
  - `LogDayCommand`, `CreateProfileCommand`, `UpdateProfileCommand`
- **Queries**: Read state (Get, List)
  - `GetDayLogQuery`, `GetMonthlyLogsQuery`, `CalculateStreakQuery`

### Benefits

- **Clear Intent**: Command vs Query is explicit
- **Single Responsibility**: Each handler does one thing
- **Easy Testing**: Mock `IMediator.Send()` in component tests
- **Pipeline Behaviors**: Cross-cutting concerns (validation, logging) in one place

## Shared Infrastructure

Cross-cutting concerns live outside feature slices:

```
src/PoOmad.Api/Infrastructure/
├── Authentication/
│   ├── CookieAuthConfig.cs         # HTTP-only cookie configuration
│   └── GoogleAuthConfig.cs         # OAuth settings
├── Health/
│   └── HealthCheckEndpoints.cs     # /api/health endpoint
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Global error handler
└── TableStorage/
    └── TableStorageClient.cs       # Azure.Data.Tables wrapper
```

## Benefits of This Architecture

### 1. **Testability**

Each slice can be tested in isolation:

```csharp
// Test a handler directly
var handler = new LogDayHandler(mockTableClient);
var result = await handler.Handle(command, CancellationToken.None);
Assert.True(result.IsSuccess);
```

### 2. **Maintainability**

All related code is co-located. To change "daily logging", only touch `Features/DailyLogs/`.

### 3. **Scalability**

New features don't impact existing ones. Add a new slice without risk.

### 4. **Parallel Development**

Multiple developers can work on different slices without merge conflicts.

## Comparison to Layered Architecture

### Traditional Layers (Avoided)

```
❌ Controllers/
❌ Services/
❌ Repositories/
❌ Models/
```

**Problems**:
- Feature code scattered across 4+ folders
- Hard to find all code for a feature
- Shared services create coupling
- Difficult to delete features cleanly

### Vertical Slices (Used)

```
✅ Features/DailyLogs/      # All daily logging code
✅ Features/Profile/        # All profile code
✅ Features/Analytics/      # All analytics code
```

**Benefits**:
- Feature code in one place
- Easy to navigate
- Delete a feature = delete one folder
- No shared business logic

## References

- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
