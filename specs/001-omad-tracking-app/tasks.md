# Tasks: PoOmad - Minimalist OMAD Tracker

**Input**: Design documents from `specs/001-omad-tracking-app/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: NOT included in this task list. Test tasks will be added when implementing TDD workflow.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5)
- All file paths are absolute from repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file `PoOmad.sln` in repository root
- [X] T002 Create `global.json` with .NET 10 SDK version lock (.NET 10.0.100 available)
- [X] T003 Create `Directory.Packages.props` in repository root with centralized package management
- [X] T004 [P] Create `src/PoOmad.Api/PoOmad.Api.csproj` with Web SDK, nullable enabled
- [X] T005 [P] Create `src/PoOmad.Client/PoOmad.Client.csproj` with Blazor WebAssembly SDK, nullable enabled
- [X] T006 [P] Create `src/PoOmad.Shared/PoOmad.Shared.csproj` with standard SDK, nullable enabled
- [X] T007 [P] Create `src/PoOmad.AppHost/PoOmad.AppHost.csproj` with Aspire.Hosting SDK
- [X] T008 [P] Create `tests/PoOmad.Api.Tests/PoOmad.Api.Tests.csproj` with xUnit, FluentAssertions, NSubstitute
- [X] T009 [P] Create `tests/PoOmad.Client.Tests/PoOmad.Client.Tests.csproj` with bUnit, xUnit
- [X] T010 [P] Create `tests/PoOmad.E2E.Tests/package.json` with Playwright TypeScript dependencies
- [X] T011 Add NuGet packages to `Directory.Packages.props`: Aspire.Hosting, Azure.Data.Tables, MediatR, FluentValidation, Serilog, OpenTelemetry, Radzen.Blazor
- [X] T012 Create `.gitignore` for .NET projects (bin/, obj/, .vs/, .vscode/, node_modules/, coverage/)
- [X] T013 Create `.dockerignore` for API containerization
- [X] T014 Create `README.md` in repository root with project description and quickstart link

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T015 Configure Azure Aspire orchestration in `src/PoOmad.AppHost/Program.cs` with service discovery for API, Table Storage, App Insights
- [X] T016 Create `src/PoOmad.Api/Program.cs` with Minimal API setup, CORS, authentication middleware registration
- [X] T017 Implement health check endpoint `GET /api/health` in `src/PoOmad.Api/Infrastructure/Health/HealthCheckEndpoints.cs` with Table Storage connectivity validation
- [X] T018 Configure Serilog in `src/PoOmad.Api/Program.cs` with Console and Application Insights sinks
- [X] T019 Configure OpenTelemetry in `src/PoOmad.Api/Program.cs` with custom metrics for business events
- [X] T020 Create `src/PoOmad.Api/Infrastructure/TableStorage/TableStorageClient.cs` wrapper for Azure.Data.Tables.TableClient
- [X] T021 Configure Azure Table Storage connection in `src/PoOmad.Api/appsettings.json` with "UseDevelopmentStorage=true" for Azurite
- [X] T022 Create `src/PoOmad.Client/Program.cs` with Blazor WASM setup, HttpClient configuration pointing to API base URL
- [X] T023 Configure Radzen.Blazor in `src/PoOmad.Client/Program.cs` services and add dark theme CSS to `wwwroot/index.html`
- [X] T024 Create `src/PoOmad.Shared/DTOs/ProblemDetailsDto.cs` for RFC 7807 error responses
- [X] T025 Configure ASP.NET Core Identity with Google OAuth in `src/PoOmad.Api/Infrastructure/Authentication/GoogleAuthConfig.cs`
- [X] T026 Add Google OAuth client ID/secret user secrets configuration instructions to `src/PoOmad.Api/README.md`
- [X] T027 Create `.vscode/launch.json` with F5 debug configuration for Aspire AppHost startup AND Blazor WebAssembly client-side debugging attachment (Constitution requirement: one-step F5 for API + browser)
- [X] T028 Configure MediatR pipeline behaviors in `src/PoOmad.Api/Program.cs` for validation and logging
- [X] T029 Create `src/PoOmad.Api/Infrastructure/Middleware/ExceptionHandlingMiddleware.cs` for global error handling with RFC 7807 responses

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Initial Setup & Profile Creation (Priority: P1) 🎯 MVP

**Goal**: Enable new users to sign in with Google OAuth, create their profile with height and starting weight, and be redirected to the calendar dashboard. Each user's data is isolated by Google account ID.

**Independent Test**: Launch app as new user, authenticate with Google, enter height (e.g., 5'10") and weight (e.g., 180 lbs), verify profile created, redirected to dashboard. Sign out, sign in with different Google account, verify separate isolated data.

### Implementation for User Story 1

- [X] T030 [P] [US1] Create `src/PoOmad.Shared/DTOs/UserProfileDto.cs` with Email, Height, StartingWeight, StartDate properties
- [X] T031 [P] [US1] Create `src/PoOmad.Shared/Validators/ProfileValidator.cs` with FluentValidation rules: height (4'0"-7'0" or 122-213cm with regex validation for formats like "5'10\"" or "178cm"), weight (50-500 lbs per FR-005)
- [X] T032 [P] [US1] Create `src/PoOmad.Api/Features/Profile/UserProfile.cs` Table Storage entity with ITableEntity (PartitionKey=GoogleId, RowKey="profile")
- [X] T033 [US1] Implement `src/PoOmad.Api/Features/Profile/CreateProfile.cs` MediatR command handler to insert UserProfile into Azure Table Storage
- [X] T034 [US1] Implement `src/PoOmad.Api/Features/Profile/GetProfile.cs` MediatR query handler to retrieve UserProfile by GoogleId
- [X] T035 [US1] Implement `src/PoOmad.Api/Features/Profile/UpdateProfile.cs` MediatR command handler to update existing UserProfile
- [X] T036 [US1] Create `src/PoOmad.Api/Features/Profile/ProfileEndpoints.cs` with Minimal API endpoints: POST /api/profile, GET /api/profile, PUT /api/profile (map to contracts/profile.http)
- [X] T037 [P] [US1] Create `src/PoOmad.Shared/DTOs/AuthenticationDto.cs` with GoogleAuthCallbackDto, UserInfoDto properties
- [X] T038 [US1] Implement `src/PoOmad.Api/Features/Authentication/GoogleAuthEndpoints.cs` with BFF OAuth flow: GET /api/auth/google (initiate), GET /api/auth/google/callback, GET /api/auth/me (map to contracts/authentication.http)
- [X] T038a [US1] Implement sign-out endpoint POST /api/auth/signout in `GoogleAuthEndpoints.cs` with cookie clearing, session termination, and redirect to auth page (FR-021)
- [X] T039 [US1] Configure HTTP-only cookie authentication in `src/PoOmad.Api/Infrastructure/Authentication/CookieAuthConfig.cs` with secure, SameSite=Strict settings
- [X] T040 [US1] Create `src/PoOmad.Client/Pages/Setup.razor` profile setup wizard with height and weight input form
- [X] T041 [US1] Create `src/PoOmad.Client/Services/ApiClient.cs` HTTP client wrapper with authentication cookie handling for profile CRUD operations
- [X] T042 [US1] Implement client-side validation in `src/PoOmad.Client/Pages/Setup.razor` using ProfileValidator from PoOmad.Shared
- [X] T043 [US1] Implement profile creation flow in `src/PoOmad.Client/Pages/Setup.razor`: submit form → POST /api/profile → navigate to Index (calendar dashboard)
- [X] T044 [US1] Create `src/PoOmad.Client/Pages/Auth.razor` page with "Sign in with Google" button that redirects to /api/auth/google
- [X] T045 [US1] Implement authentication state check in `src/PoOmad.Client/Program.cs` to redirect unauthenticated users to Auth.razor
- [X] T046 [US1] Create `src/PoOmad.Client/Services/AuthStateService.cs` to track authentication status and current user email

**Checkpoint**: At this point, User Story 1 should be fully functional - new users can authenticate, create profile, and see dashboard

---

## Phase 4: User Story 2 - Daily OMAD Logging (Priority: P1)

**Goal**: Enable users to log their daily OMAD compliance, alcohol consumption, and weight in under 10 seconds. Support editing existing entries. Prevent future date logging. Confirm weight changes exceeding 5 lbs.

**Independent Test**: Select today on calendar, complete 3-question form (OMAD: Yes, Alcohol: No, Weight: 178.5), submit, verify green indicator appears. Click today again, verify form pre-fills with existing data. Enter weight change >5 lbs, verify confirmation prompt.

### Implementation for User Story 2

- [X] T047 [P] [US2] Create `src/PoOmad.Shared/DTOs/DailyLogDto.cs` with Date, OmadCompliant, AlcoholConsumed, Weight, ServerTimestamp properties
- [X] T048 [P] [US2] Create `src/PoOmad.Shared/Validators/DailyLogValidator.cs` with FluentValidation rules: no future dates (FR-015), weight range 50-500 lbs (FR-005), 5 lb threshold check (FR-017a, FR-017b)
- [X] T049 [P] [US2] Create `src/PoOmad.Api/Features/DailyLogs/DailyLogEntry.cs` Table Storage entity with ITableEntity (PartitionKey=GoogleId, RowKey=yyyy-MM-dd)
- [X] T050 [US2] Implement `src/PoOmad.Api/Features/DailyLogs/LogDay.cs` MediatR command handler to insert or update DailyLogEntry with ServerTimestamp for conflict resolution (FR-019a)
- [X] T051 [US2] Implement `src/PoOmad.Api/Features/DailyLogs/GetDayLog.cs` MediatR query handler to retrieve specific day's log by date
- [X] T052 [US2] Implement `src/PoOmad.Api/Features/DailyLogs/GetMonthlyLogs.cs` MediatR query handler to retrieve all logs for a month (date range query on PartitionKey)
- [X] T053 [US2] Implement `src/PoOmad.Api/Features/DailyLogs/CalculateStreak.cs` MediatR query handler to count consecutive OMAD-compliant days from most recent logged date; unlogged days do NOT break streak, only logged OmadCompliant=false breaks streak (FR-009, FR-010)
- [X] T054 [US2] Implement `src/PoOmad.Api/Features/DailyLogs/DeleteDayLog.cs` MediatR command handler to delete specific day's entry (FR-011)
- [X] T055 [US2] Create `src/PoOmad.Api/Features/DailyLogs/DailyLogsEndpoints.cs` with Minimal API endpoints: POST /api/daily-logs, GET /api/daily-logs/{date}, PUT /api/daily-logs/{date}, DELETE /api/daily-logs/{date}, GET /api/daily-logs/month/{year}/{month}, GET /api/daily-logs/streak (map to contracts/daily-logs.http)
- [X] T056 [US2] Implement 5 lb weight threshold validation in `src/PoOmad.Api/Features/DailyLogs/LogDay.cs` handler by fetching previous day's weight and comparing (FR-017b context-dependent validation)
- [X] T057 [US2] Create `src/PoOmad.Client/Components/DailyLogModal.razor` with 3-question form: OMAD (Yes/No toggle), Alcohol (Yes/No toggle), Weight (number input)
- [X] T058 [US2] Implement form submission in `src/PoOmad.Client/Components/DailyLogModal.razor`: validate → POST /api/daily-logs → close modal → refresh parent calendar
- [X] T059 [US2] Implement weight change confirmation prompt in `src/PoOmad.Client/Components/DailyLogModal.razor` when API returns validation error for >5 lb change (FR-017b)
- [X] T060 [US2] Implement edit mode in `src/PoOmad.Client/Components/DailyLogModal.razor`: GET /api/daily-logs/{date} → pre-fill form → PUT on submit
- [X] T061 [US2] Add client-side validation in `src/PoOmad.Client/Components/DailyLogModal.razor` using DailyLogValidator to prevent future date selection (FR-015)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can log/edit daily data with validation

---

## Phase 5: User Story 3 - Calendar Dashboard & Visual Consistency Chain (Priority: P2)

**Goal**: Display monthly calendar grid with weeks starting on Sunday, showing green cells for OMAD success days, red cells for missed days, neutral for unlogged days. Display current streak counter. Support month navigation.

**Independent Test**: Log several days (mix of OMAD Yes/No), navigate to calendar, verify visual indicators (green/red/neutral), weeks start on Sunday, streak counter shows correct count. Navigate to previous/next month, verify data displays correctly.

### Implementation for User Story 3

- [X] T062 [P] [US3] Create `src/PoOmad.Client/Components/CalendarGrid.razor` with monthly grid layout starting weeks on Sunday (FR-006, clarification)
- [X] T063 [US3] Implement calendar cell rendering logic in `src/PoOmad.Client/Components/CalendarGrid.razor`: green for OmadCompliant=true, red for OmadCompliant=false, neutral for unlogged (FR-018)
- [X] T064 [US3] Implement month navigation in `src/PoOmad.Client/Components/CalendarGrid.razor` with previous/next month buttons (FR-016)
- [X] T065 [US3] Implement data fetching in `src/PoOmad.Client/Components/CalendarGrid.razor`: call GET /api/daily-logs/month/{year}/{month} when month changes
- [X] T066 [US3] Implement cell click handler in `src/PoOmad.Client/Components/CalendarGrid.razor` to open DailyLogModal for selected date
- [X] T067 [P] [US3] Create `src/PoOmad.Client/Components/StreakCounter.razor` to display current streak count prominently
- [X] T068 [US3] Implement streak data fetching in `src/PoOmad.Client/Components/StreakCounter.razor`: call GET /api/daily-logs/streak on dashboard load
- [X] T069 [US3] Create `src/PoOmad.Client/Pages/Index.razor` calendar dashboard page composing CalendarGrid, StreakCounter, and DailyLogModal components
- [X] T070 [US3] Implement dashboard load time optimization in `src/PoOmad.Client/Pages/Index.razor` to meet <1 second performance target (SC-003)
- [X] T071 [US3] Implement automatic dashboard refresh in `src/PoOmad.Client/Pages/Index.razor` when DailyLogModal submission completes (update visual indicators and streak)

**Checkpoint**: All P1 and P2 user stories complete - users have full tracking and visualization workflow

---

## Phase 6: User Story 4 - Weight & Alcohol Analytics (Priority: P3)

**Goal**: Display combo chart with weight trend line and alcohol consumption bars overlaid. Intelligently handle missing data by carrying forward last known weight. Show tooltips on hover. Require minimum 3 days of data.

**Independent Test**: Log 7+ days with weight and alcohol data, navigate to analytics, verify combo chart displays with weight line and alcohol bars. Verify tooltip shows details on hover. Test with <3 days of data, verify "Log at least 3 days to see your trends" message.

### Implementation for User Story 4

- [X] T072 [P] [US4] Create `src/PoOmad.Shared/DTOs/AnalyticsDto.cs` with TrendDataPointDto (Date, Weight, AlcoholConsumed, IsCarryForward) and TrendsResponseDto
- [X] T073 [US4] Implement `src/PoOmad.Api/Features/Analytics/GetTrends.cs` MediatR query handler to retrieve date range of daily logs with gap-filling logic (FR-013: carry forward last known weight); MUST require weight on first logged day in range, return 400 BadRequest if first day missing weight
- [X] T074 [US4] Implement `src/PoOmad.Api/Features/Analytics/GetCorrelation.cs` MediatR query handler to calculate alcohol/weight correlation statistics
- [X] T075 [US4] Create `src/PoOmad.Api/Features/Analytics/AnalyticsEndpoints.cs` with Minimal API endpoints: GET /api/analytics/trends?startDate={date}&endDate={date} (default 90 days), GET /api/analytics/correlation (map to contracts/analytics.http)
- [X] T076 [US4] Implement minimum data validation in `src/PoOmad.Api/Features/Analytics/GetTrends.cs` handler to return 400 if <3 days of data (per acceptance scenario)
- [X] T077 [P] [US4] Create `src/PoOmad.Client/Components/WeightChart.razor` Radzen combo chart wrapper with weight line series and alcohol bar series
- [X] T078 [US4] Configure Radzen chart in `src/PoOmad.Client/Components/WeightChart.razor`: dark mode theme, dual Y-axes (weight in lbs, alcohol binary), X-axis with date labels // Constitution III allows Radzen for complex requirements - combo chart with dual axes qualifies
- [X] T079 [US4] Implement tooltip display in `src/PoOmad.Client/Components/WeightChart.razor` showing date, weight, and alcohol status on data point hover (FR-020)
- [X] T080 [US4] Implement gap visualization in `src/PoOmad.Client/Components/WeightChart.razor` to indicate carry-forward data points (e.g., dashed line segment or different color)
- [X] T081 [US4] Create `src/PoOmad.Client/Pages/Analytics.razor` analytics page with WeightChart component and correlation statistics display
- [X] T082 [US4] Implement data fetching in `src/PoOmad.Client/Pages/Analytics.razor`: call GET /api/analytics/trends with default 90-day range
- [X] T083 [US4] Implement minimum data check in `src/PoOmad.Client/Pages/Analytics.razor` to display "Log at least 3 days to see your trends" message when API returns 400

**Checkpoint**: All core features complete (P1, P2, P3) - full tracking, visualization, and analytics available

---

## Phase 7: User Story 5 - Dark Mode Interface (Priority: P3)

**Goal**: Apply professional dark mode theme across all screens (setup, dashboard, modals, analytics) with high-contrast text and accessible color ratios for WCAG 2.1 AA compliance.

**Independent Test**: Navigate through all screens, verify dark backgrounds, high-contrast light text, vibrant green/red calendar indicators visible against dark background, input fields styled for dark mode with clear focus states, chart gridlines/axes optimized for dark theme.

### Implementation for User Story 5

- [X] T084 [P] [US5] Create `src/PoOmad.Client/wwwroot/css/dark-theme.css` with dark color palette variables and global styles
- [X] T085 [P] [US5] Update `src/PoOmad.Client/wwwroot/index.html` to include dark-theme.css and set dark background body style
- [X] T086 [US5] Apply dark theme styles to `src/PoOmad.Client/Pages/Setup.razor` setup wizard (dark backgrounds, light text, styled input fields)
- [X] T087 [US5] Apply dark theme styles to `src/PoOmad.Client/Components/CalendarGrid.razor` with high-contrast green (#4CAF50) and red (#F44336) cell indicators (FR-014, acceptance scenario 2)
- [X] T088 [US5] Apply dark theme styles to `src/PoOmad.Client/Components/DailyLogModal.razor` with dark modal background, light labels, styled toggles and number input with focus states
- [X] T089 [US5] Apply dark theme styles to `src/PoOmad.Client/Components/StreakCounter.razor` with prominent light text on dark background
- [X] T090 [US5] Configure Radzen dark theme in `src/PoOmad.Client/Components/WeightChart.razor` with dark gridlines, light axes labels, chart optimized for dark background
- [X] T091 [US5] Apply dark theme styles to `src/PoOmad.Client/Pages/Analytics.razor` analytics page background and text
- [X] T092 [US5] Verify WCAG 2.1 AA contrast ratios for all text/background combinations using browser DevTools accessibility checker

**Checkpoint**: All user stories complete with polished dark mode UI

---

## Phase 8: Offline Sync & Data Retention (Cross-Cutting)

**Goal**: Enable offline data caching with cloud sync, last-write-wins conflict resolution based on server timestamps. Support indefinite data retention and user-initiated account deletion.

**Independent Test**: Log data while offline (airplane mode), verify data cached locally. Reconnect, verify data syncs to cloud. Edit same day on two devices while offline, reconnect, verify most recent edit wins. Test account deletion flow.

### Implementation for Offline Sync

- [ ] T093 [P] Create `src/PoOmad.Client/Services/LocalStorageService.cs` using Blazored.LocalStorage with IndexedDB backend (browser native) for offline caching of daily logs and profile data
- [ ] T094 Create `src/PoOmad.Client/Services/OfflineSyncService.cs` to queue pending writes and flush on reconnect
- [ ] T095 Implement offline detection in `src/PoOmad.Client/Services/OfflineSyncService.cs` using JavaScript interop to monitor navigator.onLine
- [ ] T096 Implement write queue in `src/PoOmad.Client/Services/OfflineSyncService.cs` to store pending POST/PUT operations in LocalStorage when offline
- [ ] T097 Implement sync on reconnect in `src/PoOmad.Client/Services/OfflineSyncService.cs`: flush queue → POST/PUT to API → refresh local cache
- [ ] T098 Implement last-write-wins in `src/PoOmad.Api/Features/DailyLogs/LogDay.cs` handler by comparing client ServerTimestamp with existing entity Timestamp (FR-019a)
- [ ] T099 Update `src/PoOmad.Client/Components/DailyLogModal.razor` to use OfflineSyncService for submissions instead of direct ApiClient calls
- [ ] T100 Update `src/PoOmad.Client/Pages/Index.razor` to load data from LocalStorageService first (instant display), then fetch from API to sync

### Implementation for Data Retention & Account Deletion

- [ ] T101 [P] Implement `src/PoOmad.Api/Features/Profile/DeleteAccount.cs` MediatR command handler to cascade delete UserProfile and all DailyLogEntry records (FR-023)
- [ ] T102 Create `src/PoOmad.Api/Features/Profile/ProfileEndpoints.cs` endpoint: DELETE /api/profile with confirmation requirement
- [ ] T103 Create `src/PoOmad.Client/Pages/Settings.razor` settings page with "Delete My Account" button and confirmation modal (depends on T101, T102 API being functional)
- [ ] T104 Verify no automatic data deletion policies in Azure Table Storage configuration (FR-022: indefinite retention)

**Checkpoint**: Offline functionality and data management features complete

---

## Phase 9: Infrastructure & Deployment (Cross-Cutting)

**Goal**: Configure Azure infrastructure, CI/CD pipelines, monitoring, and cost management for production deployment to Azure Container Apps via Azure Aspire.

### Azure Infrastructure (Bicep)

- [ ] T105 [P] Create `infra/main.bicep` entry point with parameter definitions for environment, location, resourceGroup
- [ ] T106 [P] Create `infra/container-apps.bicep` module for Azure Container Apps environment and app resources (API + Client)
- [ ] T107 [P] Create `infra/table-storage.bicep` module for Azure Storage Account with Table Storage enabled
- [ ] T108 [P] Create `infra/app-insights.bicep` module for Application Insights and Log Analytics workspace
- [ ] T109 [P] Create `infra/budget.bicep` module for $5 monthly budget with 80% alert threshold email to punkouter26@gmail.com
- [ ] T110 Run `azd init` in repository root to generate Azure Developer CLI configuration and link to Bicep templates
- [ ] T111 Test local deployment with `azd up` to provision dev environment and verify resources created

### GitHub Actions CI/CD

- [ ] T112 [P] Create `.github/workflows/ci.yml` workflow: dotnet restore → build → test → code coverage report with 80% threshold check
- [ ] T113 [P] Create `.github/workflows/cd.yml` workflow: build containers → push to Azure Container Registry → deploy to Container Apps via azd deploy
- [ ] T114 Configure GitHub OIDC federated credentials in Azure for GitHub Actions authentication (no secrets in repository)
- [ ] T115 Add GitHub Actions secrets: AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID, AZURE_CLIENT_ID for OIDC
- [ ] T116 Configure Playwright E2E tests in `.github/workflows/ci.yml` to run against deployed staging environment

### Monitoring & Observability

- [ ] T117 [P] Create `docs/kql/user-activity.kql` query for active users and login frequency metrics
- [ ] T118 [P] Create `docs/kql/streak-metrics.kql` query for average streaks and longest streaks analysis
- [ ] T119 [P] Create `docs/kql/weight-trends.kql` query for weight loss patterns and alcohol correlation insights
- [ ] T120 Configure Application Insights sampling at 10% in `src/PoOmad.Api/appsettings.json` to stay under $5 budget
- [ ] T121 Create Azure Monitor alert rule for API p95 latency >200ms threshold
- [ ] T122 Create Azure Monitor alert rule for failed requests >5% of total requests

**Checkpoint**: Full production infrastructure ready for deployment

---

## Phase 10: Polish & Cross-Cutting Concerns
e structure (Authent ication, Profile, DailyLogs, Analytics)
- [X] T125 [P] Create `docs/adrs/001-azure-aspire.md` Architecture Decision Record explaining Aspire choice over traditional deployment
- [X] T126 [P] Create `docs/adrs/002-table-storage.md` ADR explaining Table Storage choice over Cosmos DB
- [X] T127 [P] Create `docs/adrs/003-bff-pattern.md` ADR explaining BFF pattern with HTTP-only cookies for security
- [X] T128 Run `dotnet format` across all projects to enforce code style consistency
- [X] T129 Review all Minimal API endpoints for consistent error handling with RFC 7807 ProblemDetails
- [X] T130 Add XML documentation comments to all public APIs in PoOmad.Shared DTOs
- [X] T131 Generate Swagger/OpenAPI documentation at `/swagger` endpoint with API descriptions and examples
- [X] T132 [P] Create `scripts/seed-test-data.ps1` PowerShell script to generate 90 days of realistic test data for 3 test users with realistic Google ID GUIDs (e.g., user1@test.com, user2@test.com, user3@test.com) for manual testing
- [X] T133 Create `scripts/run-e2e-local.ps1` PowerShell script to launch API + run Playwright tests in sequence
- [X] T134 Verify quickstart.md instructions by following F5 debug steps on clean machine
- [X] T135 Run code coverage report generation and verify 80% threshold achieved: `dotnet test --collect:"XPlat Code Coverage"`
- [X] T136 Performance optimization: implement response caching for GET /api/daily-logs/month/{year}/{month} with 5-minute expiration
- [X] T137 Performance optimization: add database index hints (if needed) for streak calculation query to meet <1s dashboard load (SC-003)
- [X] T138 Security hardening: add rate limiting middleware to prevent brute force attacks on authentication endpoints
- [X] T139 Accessibility audit: run axe-core accessibility tests in Playwright E2E suite to verify WCAG 2.1 AA compliance
- [X] T140 Final validation: manually test all acceptance scenarios from spec.md user stories

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - Can proceed in parallel if team has multiple developers
  - Or sequentially in priority order: US1 → US2 → US3 → US4 → US5
- **Offline Sync (Phase 8)**: Depends on US1 + US2 completion (requires profile and daily logs)
- **Infrastructure (Phase 9)**: Can start in parallel with user stories (Bicep development), but deployment requires all features complete
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Initial Setup & Profile)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **US2 (Daily Logging)**: Can start after Foundational (Phase 2) - May reference US1 profile data but independently testable
- **US3 (Calendar Dashboard)**: Depends on US2 (needs daily logs to display) - Can start after US2 complete
- **US4 (Analytics)**: Depends on US2 (needs daily logs for trends) - Can start after US2 complete
- **US5 (Dark Mode)**: Can start in parallel with any user story - Purely UI styling, no data dependencies

### Within Each User Story

**General Pattern**:
1. DTOs and Validators first ([P] tasks - can run in parallel)
2. Table Storage entities next ([P] tasks - can run in parallel)
3. MediatR handlers (depends on entities, but different handlers can be [P])
4. API endpoints (depends on handlers)
5. Blazor components/pages (depends on API endpoints being functional)

**US1 Example**:
- T030, T031, T032, T037 can run in parallel (DTOs, validators, entities)
- T033, T034, T035 can run in parallel after T032 (different handlers)
- T036 depends on T033-T035 (endpoints need handlers)
- T038, T039 (auth endpoints and config) can overlap with profile work
- T040-T046 (Blazor UI) depends on T036, T038 (API endpoints ready)

### Parallel Opportunities

**Phase 1 (Setup)**: T004-T010 all run in parallel (different project files)

**Phase 2 (Foundational)**: Many tasks can overlap:
- T020, T021 (Table Storage) || T025, T026 (OAuth) || T023 (Radzen) || T024 (DTOs)

**Phase 3 (US1)**: 
- T030, T031, T032, T037 all run in parallel (different files)
- T033, T034, T035 run in parallel (different handler files)

**Phase 4 (US2)**:
- T047, T048, T049 all run in parallel (different files)
- T050-T054 run in parallel (different handler files)

**Phase 5 (US3)**:
- T062, T067 run in parallel (different component files)

**Phase 6 (US4)**:
- T072, T077 run in parallel (DTO vs component)

**Phase 7 (US5)**:
- T084, T085 run in parallel (CSS file vs HTML edit)
- T086-T092 can be batched (all styling tasks on different files)

**Phase 9 (Infrastructure)**:
- T105-T109 all run in parallel (different Bicep modules)
- T112, T113 run in parallel (different workflow files)
- T117-T119 run in parallel (different KQL query files)
- T125-T127 run in parallel (different ADR files)

**Phase 10 (Polish)**:
- T123-T127 all run in parallel (different doc files)

---

## Parallel Example: User Story 1

```bash
# Batch 1: Create DTOs, validators, and entities in parallel
T030: Create UserProfileDto.cs
T031: Create ProfileValidator.cs
T032: Create UserProfile.cs entity
T037: Create AuthenticationDto.cs

# Batch 2: Create handlers in parallel (after Batch 1 complete)
T033: CreateProfile handler
T034: GetProfile handler
T035: UpdateProfile handler

# Batch 3: API endpoints (after Batch 2 complete)
T036: ProfileEndpoints.cs
T038: GoogleAuthEndpoints.cs
T039: CookieAuthConfig.cs

# Batch 4: Blazor UI in sequence (depends on API being functional)
T040: Setup.razor wizard
T041: ApiClient.cs service
T042: Client-side validation
T043: Profile creation flow
T044: Auth.razor page
T045: Authentication state check
T046: AuthStateService.cs
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

**Minimum Viable Product** delivers core accountability tracking:

1. **Phase 1**: Setup (T001-T014) - 1-2 hours
2. **Phase 2**: Foundational (T015-T029) - 4-6 hours
3. **Phase 3**: User Story 1 (T030-T046) - 6-8 hours
   - **STOP and VALIDATE**: Test authentication, profile creation, dashboard redirect
4. **Phase 4**: User Story 2 (T047-T061) - 6-8 hours
   - **STOP and VALIDATE**: Test daily logging, editing, validation, streak calculation
5. **Deploy MVP**: Users can track OMAD compliance and see visual feedback

**MVP Total**: ~20-26 hours of focused development

### Incremental Delivery (Add Features Progressively)

After MVP validation:

6. **Phase 5**: User Story 3 (T062-T071) - 4-6 hours → Calendar visualization with streak
7. **Phase 6**: User Story 4 (T072-T083) - 6-8 hours → Analytics and insights
8. **Phase 7**: User Story 5 (T084-T092) - 2-4 hours → Dark mode polish
9. **Phase 8**: Offline Sync (T093-T104) - 6-8 hours → Offline capability
10. **Phase 9**: Infrastructure (T105-T122) - 6-8 hours → Production deployment
11. **Phase 10**: Polish (T123-T140) - 4-6 hours → Documentation, optimization, security

**Full Feature Set**: ~50-66 hours total

### Parallel Team Strategy

With 3 developers after Foundational phase complete:

- **Developer A**: User Story 1 (Authentication + Profile) - 6-8 hours
- **Developer B**: User Story 2 (Daily Logging) - 6-8 hours
- **Developer C**: Infrastructure (Bicep, CI/CD) - 6-8 hours

Then converge:
- **Developer A**: User Story 3 (Calendar Dashboard)
- **Developer B**: User Story 4 (Analytics)
- **Developer C**: User Story 5 (Dark Mode) + Offline Sync

**Parallel Completion**: ~16-20 hours wall-clock time (with 3 devs)

---

## Notes

- **[P] Marker**: Tasks marked [P] can run in parallel - they touch different files and have no dependencies
- **[Story] Labels**: Each task is tagged with its user story (US1-US5) for traceability and independent completion
- **File Paths**: All paths are absolute from repository root for clarity
- **Checkpoints**: Each phase ends with an independent validation checkpoint
- **Testing**: Test tasks are NOT included - add when implementing TDD workflow (write tests first, verify they fail, implement, verify they pass)
- **Constitution Compliance**: Tasks align with Constitution requirements (Vertical Slice, Minimal APIs, FluentValidation, xUnit, bUnit, Playwright, Aspire)
- **Performance**: Tasks T070, T136, T137 specifically target performance goals (SC-002, SC-003)
- **Security**: Tasks T039, T138 address security (BFF pattern, rate limiting)
- **Accessibility**: Tasks T092, T139 ensure WCAG 2.1 AA compliance
- **Deviations**: Tasks acknowledge documented Constitution deviations (.NET 8/9, Aspire, Container Apps)

---

## Success Metrics Validation

Map completed tasks to spec.md success criteria:

- **SC-001** (Setup in <2 min): US1 tasks T030-T046
- **SC-002** (Logging in <10 sec): US2 tasks T047-T061, optimized by T136
- **SC-003** (Dashboard <1s load): US3 tasks T062-T071, optimized by T070, T137
- **SC-004** (Visual adherence pattern): US3 tasks T062-T071 (green/red color coding)
- **SC-005** (Accurate streak): US2 task T053 (CalculateStreak handler)
- **SC-006** (Chart with 20% gaps): US4 task T073 (gap-filling logic)
- **SC-007** (90% log without help): US2 tasks T057-T061 (intuitive 3-question modal)
- **SC-008** (Offline functionality): Phase 8 tasks T093-T100 (offline sync)

All success criteria have corresponding implementation tasks.
