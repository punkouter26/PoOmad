# PoOmad: The Minimalist Accountability Partner

**One Meal A Day (OMAD) Tracking Application**

## Overview

PoOmad is a minimalist web application designed to help users track their One Meal A Day (OMAD) adherence, alcohol consumption, and weight trends. Built with a focus on simplicity and speed, the app enables daily logging in under 10 seconds while providing powerful analytics and visual feedback to maintain accountability.

## Key Features

- **Google OAuth Authentication**: Secure sign-in with Google accounts
- **Profile Management**: Set your height and starting weight
- **Daily Logging**: Quick 3-question form (OMAD compliance, alcohol, weight)
- **Visual Streak Counter**: See your consecutive OMAD-compliant days
- **Calendar Dashboard**: Monthly view with color-coded success indicators
- **Analytics Dashboard**: Weight trends and alcohol correlation charts
- **Dark Mode Interface**: Professional dark theme with WCAG 2.1 AA compliance
- **Multi-User Support**: Each user's data is fully isolated by Google account

## Architecture

PoOmad is built with modern .NET technologies and cloud-native patterns:

- **Frontend**: Blazor WebAssembly with Radzen UI components
- **Backend**: ASP.NET Core Minimal APIs with Vertical Slice Architecture
- **Orchestration**: Azure Aspire for local development and cloud deployment
- **Storage**: Azure Table Storage (NoSQL)
- **Authentication**: BFF (Backend-for-Frontend) pattern with HTTP-only cookies
- **Observability**: Serilog + OpenTelemetry + Application Insights
- **Deployment**: Azure Container Apps

### Technology Stack

- .NET 10.0
- Azure Aspire 13.0.0
- Blazor WebAssembly
- ASP.NET Core Web API
- Azure Table Storage
- MediatR (CQRS)
- FluentValidation
- Radzen.Blazor 5.6.8
- xUnit + bUnit + Playwright

## Documentation

- **[Quickstart Guide](../specs/001-omad-tracking-app/quickstart.md)**: Get started with F5 debugging
- **[Feature Specification](../specs/001-omad-tracking-app/spec.md)**: Detailed user stories and acceptance criteria
- **[Architecture Overview](./architecture/vertical-slices.md)**: Feature slice structure
- **[Data Model](../specs/001-omad-tracking-app/data-model.md)**: Entity design and partitioning strategy

### Architecture Decision Records (ADRs)

- **[ADR-001: Azure Aspire](./adrs/001-azure-aspire.md)**: Why we chose Aspire for orchestration
- **[ADR-002: Table Storage](./adrs/002-table-storage.md)**: Why Azure Table Storage over Cosmos DB
- **[ADR-003: BFF Pattern](./adrs/003-bff-pattern.md)**: Security architecture for authentication

## Project Structure

```
PoOmad/
├── src/
│   ├── PoOmad.Api/           # ASP.NET Core Web API
│   ├── PoOmad.Client/        # Blazor WebAssembly app
│   ├── PoOmad.Shared/        # Shared DTOs and validators
│   └── PoOmad.AppHost/       # Azure Aspire orchestration
├── tests/
│   ├── PoOmad.Api.Tests/     # Backend unit/integration tests
│   ├── PoOmad.Client.Tests/  # Blazor component tests
│   └── PoOmad.E2E.Tests/     # Playwright end-to-end tests
├── specs/
│   └── 001-omad-tracking-app/  # Specification documents
├── docs/
│   ├── architecture/         # Architecture documentation
│   └── adrs/                 # Architecture Decision Records
└── scripts/                  # Utility scripts
```

## Getting Started

### Prerequisites

- .NET 10.0.100 SDK or later
- Azure Storage Emulator (Azurite) for local development
- Google OAuth credentials (Client ID and Secret)
- Node.js 20+ (for E2E tests)

### Quick Start

1. **Clone the repository**
   ```powershell
   git clone <repository-url>
   cd PoOmad
   ```

2. **Configure Google OAuth**
   ```powershell
   cd src/PoOmad.Api/PoOmad.Api
   dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
   ```

3. **Press F5 in Visual Studio Code**
   - Aspire will launch the API, Azurite, and open the Blazor client in your browser
   - See [quickstart.md](../specs/001-omad-tracking-app/quickstart.md) for detailed instructions

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run E2E tests
cd tests/PoOmad.E2E.Tests
npm install
npm test
```

## Success Criteria

- ✅ **Setup in <2 minutes**: New users can authenticate and create profile quickly
- ✅ **Logging in <10 seconds**: Daily entry form is fast and intuitive
- ✅ **Dashboard loads <1 second**: Calendar and streak display instantly
- ✅ **Visual adherence pattern**: Color-coded calendar provides instant feedback
- ✅ **Accurate streak calculation**: Consecutive OMAD days counted correctly
- ✅ **Analytics with gaps**: Weight trends intelligently handle missing data

## Contributing

This project follows Vertical Slice Architecture. Each feature is self-contained:

- DTOs and validators in `PoOmad.Shared`
- MediatR handlers and entities in `PoOmad.Api/Features/<FeatureName>/`
- Minimal API endpoints in `PoOmad.Api/Features/<FeatureName>/<FeatureName>Endpoints.cs`
- Blazor components in `PoOmad.Client/Components/` or `Pages/`

See the [Implementation Plan](../specs/001-omad-tracking-app/plan.md) for detailed architecture guidance.

## License

[Add license information]

## Contact

[Add contact information]
