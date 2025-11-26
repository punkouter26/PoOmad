using Microsoft.AspNetCore.Authentication.Cookies;
using PoOmad.Api.Infrastructure.Health;
using PoOmad.Api.Infrastructure.Middleware;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Api.Features.Profile;
using PoOmad.Api.Features.Authentication;
using PoOmad.Api.Features.DailyLogs;
using PoOmad.Api.Features.Analytics;
using Serilog;
using FluentValidation;
using OpenTelemetry.Metrics;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting PoOmad API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add services to the container
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();

    // Application Insights (telemetry will be added after Aspire integration)
    builder.Services.AddApplicationInsightsTelemetry();

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddMeter("PoOmad.Api")
            .AddConsoleExporter());

    // MediatR for CQRS
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Azure Table Storage Client
    builder.Services.AddSingleton(sp =>
    {
        var connectionString = builder.Configuration.GetConnectionString("TableStorage") ?? "UseDevelopmentStorage=true";
        return new Azure.Data.Tables.TableServiceClient(connectionString);
    });

    builder.Services.AddSingleton<TableStorageClient>();

    // No CORS needed - Blazor WASM is hosted in-process

    // Authentication (HTTP-only cookies for BFF pattern)
    var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/api/auth/google";
        });

    // Only add Google auth if credentials are configured
    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
    {
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.CallbackPath = "/api/auth/google/callback";
        });
    }
    else
    {
        Log.Warning("Google OAuth not configured - authentication will be disabled");
    }

    builder.Services.AddAuthorization();

    // Rate limiting
    builder.Services.AddRateLimitingPolicies();

    // Response caching
    builder.Services.AddResponseCaching();
    builder.Services.AddOutputCache();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandling();
    app.UseSerilogRequestLogging();
    
    // Only use HTTPS redirection in production
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseRateLimitingPolicies();
    app.UseResponseCaching();
    app.UseOutputCache();
    
    // Serve Blazor WebAssembly files
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    
    app.UseAuthentication();
    app.UseAuthorization();

    // Map health check endpoint
    app.MapHealthChecks();

    // Map feature endpoints
    app.MapAuthEndpoints();
    app.MapProfileEndpoints();
    app.MapDailyLogsEndpoints();
    app.MapAnalyticsEndpoints();
    
    // Fallback to Blazor for client-side routing
    app.MapFallbackToFile("index.html");

    Log.Information("PoOmad API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    await Log.CloseAndFlushAsync();
}

