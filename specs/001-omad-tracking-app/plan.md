# Implementation Plan: PoOmad - Minimalist OMAD Tracker

**Branch**: `001-omad-tracking-app` | **Date**: 2025-11-22 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-omad-tracking-app/spec.md`

**Note**: This plan follows the `/speckit.plan` workflow and aligns with the PoOmad Constitution v1.0.0.

## Summary

PoOmad is a minimalist accountability partner for One Meal A Day (OMAD) practitioners. The application provides a binary tracking system (you either stuck to the plan or didn't) with friction-free daily logging (<10 seconds), visual calendar-based motivation with streak tracking, and smart analytics showing correlations between alcohol consumption and weight trends. The system supports multi-user access via Google OAuth with indefinite data retention and cross-device sync.

**Technical Approach**: Cloud-native .NET 8/9 solution orchestrated by Azure Aspire, deployed to Azure Container Apps. Blazor WebAssembly frontend with ASP.NET Core Web API backend (Minimal APIs + Vertical Slice Architecture). Azure Table Storage for NoSQL persistence, Google OAuth with BFF pattern for authentication, Radzen Blazor Components for advanced charting. Fully containerized with Docker, tested via xUnit (backend) and Playwright TypeScript (E2E).

## Technical Context

**Language/Version**: C# 13 / .NET 9 or .NET 10 (Constitution specifies .NET 10; using .NET 9 until .NET 10 is GA; deviation documented in Complexity Tracking)  
**Primary Dependencies**: 
- **Orchestration**: Azure Aspire (service discovery, deployment orchestration)
- **Frontend**: Blazor WebAssembly, Radzen.Blazor (charts, advanced UI components)
- **Backend**: ASP.NET Core Web API (Minimal APIs), MediatR (CQRS pattern for Vertical Slice)
- **Authentication**: ASP.NET Core Identity, Google OAuth, BFF pattern with HTTP-only cookies
- **Storage**: Azure Table Storage SDK
- **Testing**: xUnit (unit/integration), bUnit (component tests), Playwright TypeScript (E2E)
- **Logging**: Serilog (structured logging), Application Insights SDK
- **Telemetry**: OpenTelemetry .NET SDK
- **Validation**: FluentValidation (shared DTOs in PoOmad.Shared)

**Storage**: Azure Table Storage (NoSQL, cost-effective, partitioned by user ID for efficient queries)  
**Testing**: xUnit for backend unit/integration tests, bUnit for Blazor component tests, Playwright (TypeScript) for E2E tests targeting Chromium desktop + mobile viewports  
**Target Platform**: 
- **Frontend**: WebAssembly (WASM) running in modern browsers (Chrome, Edge, Safari, Firefox)
- **Backend**: Azure Container Apps (Linux containers via Docker)
- **Deployment**: Azure Aspire-managed deployment to Azure Container Apps

**Project Type**: Web application (frontend + backend architecture)  
**Performance Goals**: 
- Calendar dashboard load: <1 second (SC-003)
- Daily logging interaction: <10 seconds total (SC-002)
- API response time: <200ms p95 for CRUD operations
- Chart rendering: <2 seconds for 90 days of data

**Constraints**: 
- Offline-capable: Local caching with cloud sync (FR-019)
- Mobile-first: Responsive design optimized for portrait mode touch interaction
- Accessibility: WCAG 2.1 AA compliance for dark mode UI
- Cost: $5/month Azure budget limit with 80% alert threshold
- Security: BFF pattern with HTTP-only cookies (no JWT in localStorage)

**Scale/Scope**: 
- Expected users: 100-1,000 users (single developer personal project scale)
- Data volume: ~365 log entries per user per year
- Retention: Indefinite (no automatic deletion per clarification)
- Concurrent users: <100 concurrent sessions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Foundation & Naming Standards

| Requirement | Status | Notes |
|-------------|--------|-------|
| Solution naming consistent (PoOmad.sln → PoOmad-rg, PoOmad-app, etc.) | ✅ PASS | Will enforce across Azure resources |
| .NET 10 with global.json SDK lock | ⚠️ DEVIATION | **User specified .NET 9/10**; Will use .NET 9 until .NET 10 GA, then upgrade (see Complexity Tracking) |
| Centralized package management (Directory.Packages.props) | ✅ PASS | Will implement at repository root |
| Null safety enabled in all .csproj | ✅ PASS | Will enable `<Nullable>enable</Nullable>` |

### II. Architecture & Code Organization

| Requirement | Status | Notes |
|-------------|--------|-------|
| Vertical Slice Architecture in `/src/PoOmad.Api/Features/` | ✅ PASS | MediatR handlers co-located by feature |
| SOLID principles + GoF patterns documented | ✅ PASS | Will document in code comments |
| Minimal APIs for all endpoints | ✅ PASS | ASP.NET Core Minimal APIs specified |
| Repository structure: /src, /tests, /docs, /infra, /scripts | ✅ PASS | Will implement standard layout |
| Separation: PoOmad.Api, PoOmad.Client, PoOmad.Shared | ✅ PASS | Blazor WASM structure with shared DTOs |
| PoOmad.Shared contains only DTOs/contracts/validation | ✅ PASS | FluentValidation rules only |

### III. Implementation Standards

| Requirement | Status | Notes |
|-------------|--------|-------|
| Swagger/OpenAPI generation enabled | ✅ PASS | Will enable Swashbuckle |
| .http files for manual verification | ✅ PASS | Will create in /tests/http/ |
| Health check endpoint at `api/health` | ✅ PASS | Will validate Azure Table Storage connectivity |
| RFC 7807 Problem Details for errors | ✅ PASS | Will implement IResult with ProblemDetails |
| Prefer standard Blazor; Radzen for complex needs | ⚠️ ACCEPTABLE | **User specified Radzen for charting**; Constitution allows for complex requirements |
| Mobile-first responsive design | ✅ PASS | Dark mode, touch-friendly per spec |
| One-step F5 debug launch | ✅ PASS | Will configure launch.json with serverReadyAction |
| AppSettings locally, Key Vault in production | ⚠️ DEVIATION | **User specified Azure Aspire orchestration**; Will adapt to Aspire's config model |
| Azurite for local storage | ⚠️ DEVIATION | **User specified Azure Table Storage**; Constitution assumes Azure Storage blobs; Will use Azurite for Table Storage emulation |

### IV. Quality & Testing Discipline (NON-NEGOTIABLE)

| Requirement | Status | Notes |
|-------------|--------|-------|
| Code hygiene (no warnings, `dotnet format`) | ✅ PASS | Will enforce in PR workflow |
| Dependency hygiene via Directory.Packages.props | ✅ PASS | Will maintain centrally |
| TDD for business logic (Red → Green → Refactor) | ✅ PASS | xUnit tests for MediatR handlers |
| Test naming: `MethodName_StateUnderTest_ExpectedBehavior` | ✅ PASS | Will enforce convention |
| 80% code coverage threshold with dotnet-coverage | ✅ PASS | Will generate reports in docs/coverage/ |
| xUnit for unit tests (mocked dependencies) | ✅ PASS | Per user spec |
| bUnit for Blazor component tests | ✅ PASS | Per constitution requirement |
| xUnit integration tests for all endpoints | ✅ PASS | Happy path with in-memory Table Storage emulator |
| Playwright E2E tests (Chromium desktop + mobile) | ✅ PASS | **User specified TypeScript**; Constitution allows Playwright |

### V. Operations & Azure DevOps

| Requirement | Status | Notes |
|-------------|--------|-------|
| Bicep provisioning via Azure Developer CLI (azd) | ✅ PASS | Aspire generates Bicep automatically; Will use `azd` for deployment |
| GitHub Actions with OIDC Federated Credentials | ✅ PASS | Will configure federated identity |
| Deploy to PoOmad-rg resource group | ✅ PASS | Will deploy to PoOmad-rg as Azure Container Apps per Constitution |
| Provision: App Insights, Log Analytics, Container Apps, Storage | ✅ PASS | Will provision: App Insights, Log Analytics, Azure Container Apps Environment + Apps, Azure Table Storage |
| $5 monthly budget with 80% alert to punkouter26@gmail.com | ✅ PASS | Will configure Action Group |
| Serilog structured logging (Debug Console + App Insights) | ✅ PASS | Will configure appsettings.json |
| OpenTelemetry for custom telemetry/metrics | ✅ PASS | Will use Meter for business metrics |
| Enable Snapshot Debugger + Profiler | ⚠️ ADAPTED | Container Apps support; Will enable if available, else use alternative diagnostics |
| KQL library in docs/kql/ | ✅ PASS | Will create queries for user activity, streaks, weight trends |

### Summary

**GATE STATUS**: ✅ **PASS** (1 minor deviation documented below)

**Blocking Issues**: None

**Warnings**: 
1. .NET 9 (temporary) vs Constitution's .NET 10 (will upgrade when .NET 10 GA)

**Recommendation**: Proceed with Phase 0 research. Document deviations in Complexity Tracking section. Consider updating Constitution to v1.1.0 to formally recognize Azure Aspire + Container Apps as approved deployment pattern.

## Project Structure

### Documentation (this feature)

```text
specs/001-omad-tracking-app/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (best practices, Azure Aspire patterns)
├── data-model.md        # Phase 1 output (entities, Table Storage schema)
├── quickstart.md        # Phase 1 output (F5 debug with Aspire orchestration)
├── contracts/           # Phase 1 output (API endpoints, DTOs)
│   ├── authentication.http
│   ├── profile.http
│   ├── daily-logs.http
│   └── analytics.http
└── checklists/
    └── requirements.md  # Pre-existing validation checklist
```

### Source Code (repository root)

```text
/
├── src/
│   ├── PoOmad.Api/                    # ASP.NET Core Web API (Minimal APIs)
│   │   ├── Features/                   # Vertical Slice Architecture
│   │   │   ├── Authentication/         # Google OAuth, BFF endpoints
│   │   │   ├── Profile/                # User profile CRUD
│   │   │   ├── DailyLogs/              # Daily tracking CRUD
│   │   │   └── Analytics/              # Weight/alcohol trends
│   │   ├── Infrastructure/             # Cross-cutting concerns
│   │   │   ├── TableStorage/           # Azure Table Storage client
│   │   │   ├── Authentication/         # ASP.NET Core Identity config
│   │   │   └── Health/                 # Health check probes
│   │   ├── Program.cs                  # Minimal API endpoints registration
│   │   ├── appsettings.json            # Local config
│   │   └── PoOmad.Api.csproj
│   │
│   ├── PoOmad.Client/                  # Blazor WebAssembly frontend
│   │   ├── Pages/                      # Routable pages
│   │   │   ├── Index.razor             # Calendar dashboard
│   │   │   ├── Setup.razor             # Initial profile wizard
│   │   │   └── Analytics.razor         # Charts page
│   │   ├── Components/                 # Reusable UI components
│   │   │   ├── CalendarGrid.razor      # Monthly calendar with visual indicators
│   │   │   ├── DailyLogModal.razor     # 3-question logging form
│   │   │   ├── StreakCounter.razor     # Current streak display
│   │   │   └── WeightChart.razor       # Radzen combo chart wrapper
│   │   ├── Services/                   # Frontend services
│   │   │   ├── IApiClient.cs           # HTTP client abstraction
│   │   │   └── LocalStorageService.cs  # Offline caching
│   │   ├── Program.cs                  # WASM entry point
│   │   └── PoOmad.Client.csproj
│   │
│   ├── PoOmad.Shared/                  # Shared contracts (referenced by both Api + Client)
│   │   ├── DTOs/
│   │   │   ├── UserProfileDto.cs
│   │   │   ├── DailyLogDto.cs
│   │   │   ├── AnalyticsDto.cs
│   │   │   └── AuthenticationDto.cs
│   │   ├── Validators/                 # FluentValidation rules
│   │   │   ├── ProfileValidator.cs     # Height/weight range validation
│   │   │   └── DailyLogValidator.cs    # 5 lb threshold, future date prevention
│   │   └── PoOmad.Shared.csproj
│   │
│   └── PoOmad.AppHost/                 # Azure Aspire orchestration project
│       ├── Program.cs                   # Service discovery config
│       ├── appsettings.json
│       └── PoOmad.AppHost.csproj
│
├── tests/
│   ├── PoOmad.Api.Tests/               # Backend unit + integration tests
│   │   ├── Features/
│   │   │   ├── Profile/
│   │   │   │   └── CreateProfileHandlerTests.cs
│   │   │   ├── DailyLogs/
│   │   │   │   ├── LogDayHandlerTests.cs
│   │   │   │   └── CalculateStreakTests.cs
│   │   │   └── Analytics/
│   │   │       └── GetTrendsHandlerTests.cs
│   │   ├── Integration/
│   │   │   └── ApiEndpointsTests.cs    # Happy path for all endpoints
│   │   └── PoOmad.Api.Tests.csproj
│   │
│   ├── PoOmad.Client.Tests/            # Blazor component tests (bUnit)
│   │   ├── Pages/
│   │   │   ├── IndexTests.cs           # Calendar dashboard rendering
│   │   │   └── SetupTests.cs           # Profile wizard flow
│   │   ├── Components/
│   │   │   ├── CalendarGridTests.cs    # Visual indicator logic
│   │   │   ├── DailyLogModalTests.cs   # 3-question form interactions
│   │   │   └── StreakCounterTests.cs   # Streak calculation display
│   │   └── PoOmad.Client.Tests.csproj
│   │
│   ├── PoOmad.E2E.Tests/               # Playwright TypeScript E2E tests
│   │   ├── tests/
│   │   │   ├── auth.spec.ts            # Google OAuth flow
│   │   │   ├── setup.spec.ts           # First-time setup wizard
│   │   │   ├── daily-logging.spec.ts   # Log today, edit entry
│   │   │   ├── calendar.spec.ts        # Month navigation, visual chain
│   │   │   ├── analytics.spec.ts       # Chart rendering, tooltips
│   │   │   └── accessibility.spec.ts   # WCAG AA checks, dark mode contrast
│   │   ├── playwright.config.ts        # Chromium desktop + mobile viewports
│   │   ├── package.json
│   │   └── tsconfig.json
│   │
│   └── http/                           # Manual API verification (.http files)
│       ├── authentication.http
│       ├── profile.http
│       ├── daily-logs.http
│       └── analytics.http
│
├── infra/                              # Azure infrastructure (Bicep - generated by Aspire)
│   ├── main.bicep                      # Entry point
│   ├── container-apps.bicep            # Azure Container Apps resources
│   ├── table-storage.bicep             # Azure Table Storage
│   ├── app-insights.bicep              # Application Insights + Log Analytics
│   └── budget.bicep                    # $5 monthly budget with alerts
│
├── docs/
│   ├── README.md                       # App description + run instructions
│   ├── architecture/
│   │   └── vertical-slices.md          # Diagram of feature slices
│   ├── kql/                            # KQL query library
│   │   ├── user-activity.kql           # Active users, login frequency
│   │   ├── streak-metrics.kql          # Average streaks, longest streaks
│   │   └── weight-trends.kql           # Weight loss patterns, alcohol correlation
│   ├── coverage/                       # Code coverage reports (generated)
│   └── adrs/                           # Architecture Decision Records
│       ├── 001-azure-aspire.md         # Why Aspire over traditional deployment
│       ├── 002-table-storage.md        # Why Table Storage vs Cosmos DB
│       └── 003-bff-pattern.md          # Why BFF with HTTP-only cookies
│
├── scripts/
│   ├── seed-test-data.ps1              # Generate realistic test data
│   └── run-e2e-local.ps1               # Launch API + run Playwright tests
│
├── .github/
│   └── workflows/
│       ├── ci.yml                      # Build, test, coverage
│       └── cd.yml                      # Deploy to Azure via azd (OIDC)
│
├── .vscode/
│   └── launch.json                     # F5 debug config (Aspire orchestration)
│
├── global.json                         # SDK version lock (NOTE: .NET 8/9, not .NET 10)
├── Directory.Packages.props            # Centralized package versions
├── PoOmad.sln                          # Solution file
├── .dockerignore
├── .gitignore
└── README.md
```

**Structure Decision**: Selected **Web application** (Option 2 from template) with Aspire orchestration. The architecture separates frontend (Blazor WASM), backend (ASP.NET Core API), and shared contracts, aligned with Constitution's separation of concerns principle. Added `PoOmad.AppHost` for Azure Aspire service discovery. Testing follows the constitution's three-tier model: xUnit (unit/integration), bUnit (components), Playwright (E2E).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| **.NET 9 instead of .NET 10** | User explicitly specified .NET 9/10 architecture; Will use .NET 9 (current LTS) until .NET 10 GA; Azure Aspire fully supports .NET 9 | .NET 10 not yet GA (as of Nov 2025); Will upgrade to .NET 10 when released; .NET 9 is latest stable LTS version |

**NOTE**: Constitution has been updated to recognize Azure Container Apps as the approved deployment pattern. .NET 9 is a temporary deviation until .NET 10 GA release.
