# Research: PoOmad - Technical Decisions & Best Practices

**Date**: 2025-11-22  
**Purpose**: Phase 0 research to resolve technical unknowns and document best practices for PoOmad implementation

---

## Decision 1: Azure Aspire Orchestration Pattern

**Context**: Constitution specifies traditional Azure App Service deployment with Bicep, but user architecture requires Azure Aspire for service discovery and Container Apps deployment.

**Decision**: Use Azure Aspire orchestration with Azure Container Apps deployment

**Rationale**:
- **Service Discovery**: Aspire provides automatic service discovery between API and dependencies (Table Storage, App Insights) without hard-coded connection strings
- **Local Development**: Aspire dashboard provides unified local testing experience with integrated observability
- **Cost Efficiency**: Container Apps support scale-to-zero, better aligned with $5/month budget constraint
- **Bicep Generation**: Aspire auto-generates Bicep templates, satisfying Constitution's IaC requirement
- **Azure Developer CLI**: Aspire integrates with `azd`, satisfying Constitution's deployment tooling requirement

**Alternatives Considered**:
- Traditional App Service: Rejected due to lack of container orchestration and higher baseline costs
- Kubernetes (AKS): Rejected due to operational complexity and cost overhead for single-developer project

**Implementation Approach**:
1. Add `PoOmad.AppHost` project for Aspire orchestration
2. Configure service references in `Program.cs` (Api, Table Storage, App Insights)
3. Use `azd init` to generate deployment infrastructure
4. Deploy to Azure Container Apps via `azd up`

---

## Decision 2: Azure Table Storage Schema Design

**Context**: NoSQL storage for user profiles and daily log entries with efficient querying by user ID

**Decision**: Partition by `UserId` (Google account ID), use composite row keys for daily logs

**Rationale**:
- **Efficient Queries**: All user data co-located in same partition for fast retrieval
- **Cost**: Table Storage is ~10x cheaper than Cosmos DB for this workload (<1000 users)
- **Query Patterns**: Primary access pattern is "get all logs for user X" – perfect for partition key queries
- **Scalability**: Single user's annual data (~365 entries) fits well within partition size limits

**Schema**:

### User Profile Table (`UserProfiles`)
| PartitionKey | RowKey | Height | StartingWeight | StartDate | Email |
|--------------|--------|--------|----------------|-----------|-------|
| `{GoogleId}` | `profile` | `5'10"` | `180.0` | `2025-11-22` | `user@gmail.com` |

### Daily Logs Table (`DailyLogs`)
| PartitionKey | RowKey | OmadCompliant | AlcoholConsumed | Weight | Timestamp |
|--------------|--------|---------------|-----------------|--------|-----------|
| `{GoogleId}` | `2025-11-22` | `true` | `false` | `178.5` | `2025-11-22T14:30:00Z` |
| `{GoogleId}` | `2025-11-23` | `true` | `true` | `179.0` | `2025-11-23T15:15:00Z` |

**Alternatives Considered**:
- Cosmos DB: Rejected due to cost (5-10x more expensive for this scale)
- SQL Database: Rejected due to cost and over-engineering for simple key-value access patterns
- Partition by date: Rejected because it scatters user data across partitions, requiring expensive cross-partition queries

---

## Decision 3: Google OAuth with BFF Pattern

**Context**: Multi-user authentication with secure token handling in Blazor WASM app

**Decision**: Backend-for-Frontend (BFF) pattern with HTTP-only cookies, no JWT in localStorage

**Rationale**:
- **Security**: HTTP-only cookies prevent XSS attacks from stealing tokens (OWASP best practice)
- **WASM Limitation**: Blazor WASM cannot securely store secrets in browser storage
- **BFF Pattern**: API acts as OAuth client, frontend only stores session cookie
- **ASP.NET Core Identity**: Built-in support for Google OAuth provider

**Authentication Flow**:
1. User clicks "Sign in with Google" → redirects to `/api/auth/google`
2. API handles OAuth flow, exchanges auth code for tokens
3. API creates ASP.NET Core Identity session, sets HTTP-only cookie
4. Frontend makes authenticated requests with cookie (automatic)
5. API validates cookie on each request, extracts user identity

**Alternatives Considered**:
- JWT in localStorage: Rejected due to XSS vulnerability
- Auth0/Azure AD B2C: Rejected due to added cost and complexity for Google-only auth

---

## Decision 4: Radzen Blazor Components for Charting

**Context**: Constitution prefers standard Blazor components; Radzen allowed for complex requirements

**Decision**: Use Radzen Charts for combo chart (weight line + alcohol bars)

**Rationale**:
- **Complexity**: Combo chart with dual axes (weight trend + alcohol bars) is beyond standard Blazor capabilities
- **Constitution Compliance**: "Radzen.Blazor MAY ONLY be used for complex requirements as needed" – charts qualify
- **Developer Experience**: Radzen provides production-ready dark mode themes matching spec requirements
- **Performance**: Optimized for rendering 90+ data points (3 months of daily logs)

**Usage Scope** (limited to complex UI):
- ✅ Weight/alcohol analytics combo chart (US4)
- ❌ Calendar grid (will use standard Blazor components)
- ❌ Daily log modal (will use standard Blazor forms)
- ❌ Profile setup wizard (will use standard Blazor forms)

---

## Decision 5: Offline Sync Strategy (Last-Write-Wins)

**Context**: Spec clarification requires offline capability with cloud sync; conflict resolution strategy defined as "last write wins"

**Decision**: Client-side caching with optimistic UI updates, server timestamp-based conflict resolution

**Rationale**:
- **Simplicity**: Avoids complex merge UI, aligns with "minimalist" philosophy
- **Rare Conflicts**: Single user unlikely to edit same day on multiple devices simultaneously
- **User Expectation**: Most recent action should be authoritative (intuitive behavior)

**Implementation**:
1. Daily log writes include `Timestamp` (server-generated)
2. Client caches writes locally (IndexedDB via Blazor.LocalStorage)
3. On reconnect, client pushes pending writes to server
4. Server compares timestamps: if client timestamp > server timestamp, accept write
5. Client refreshes data after sync to ensure consistency

**Alternatives Considered**:
- Manual conflict resolution UI: Rejected due to UX complexity contradicting 10-second logging goal
- Operational Transform: Rejected as over-engineering for simple key-value updates

---

## Decision 6: Vertical Slice Architecture with MediatR

**Context**: Constitution requires Vertical Slice Architecture; user architecture doesn't mention CQRS library

**Decision**: Use MediatR library to implement Vertical Slice with CQRS pattern

**Rationale**:
- **Feature Isolation**: Each feature (Profile, DailyLogs, Analytics) is self-contained folder with commands/queries/handlers
- **Testability**: Handlers are easily unit-tested with mocked dependencies
- **Constitution Alignment**: MediatR is de facto .NET standard for Vertical Slice implementations
- **Minimal APIs Integration**: MediatR handlers map cleanly to Minimal API endpoints

**Slice Example** (DailyLogs feature):
```
Features/DailyLogs/
├── LogDay.cs               # Command + Handler + Validator
├── GetDayLog.cs            # Query + Handler
├── CalculateStreak.cs      # Query + Handler
└── DailyLogsEndpoints.cs   # Minimal API endpoint registrations
```

**Alternatives Considered**:
- Manual slice organization: Rejected due to lack of consistent command/query separation
- Carter library: Rejected due to MediatR being more widely adopted in .NET community

---

## Decision 7: FluentValidation in Shared Project

**Context**: Constitution requires PoOmad.Shared to contain only DTOs and validation rules

**Decision**: Place FluentValidation rules in PoOmad.Shared, referenced by both API and Client

**Rationale**:
- **DRY Principle**: Validation rules defined once, used in both server-side (API) and client-side (Blazor) validation
- **Constitution Compliance**: FluentValidation qualifies as "shared validation logic"
- **Blazor Compatibility**: FluentValidation works in WASM with same rules as server

**Validation Examples**:
- Weight range: 50-500 lbs (FR-005)
- Height range: 4'-8' or 120-250cm (FR-004)
- 5 lb threshold trigger (FR-017b)
- No future dates (FR-015)

---

## Decision 8: Playwright TypeScript for E2E Tests

**Context**: Constitution requires Playwright; user specified TypeScript over C# bindings

**Decision**: Use Playwright TypeScript with official @playwright/test runner

**Rationale**:
- **Official Support**: TypeScript is Playwright's primary language (better documentation, faster updates)
- **Ecosystem**: Rich plugin ecosystem (accessibility testing, visual regression)
- **Developer Experience**: VSCode IntelliSense superior for Playwright TypeScript vs C# bindings

**Test Strategy**:
- Desktop viewport: 1920x1080 (Chromium)
- Mobile viewport: 375x667 (Chromium mobile)
- Tests: auth flow, setup wizard, daily logging, calendar navigation, analytics chart, accessibility (axe-core)

**Alternatives Considered**:
- Playwright .NET bindings: Rejected due to less mature ecosystem and slower updates
- Selenium: Rejected due to Playwright's superior developer experience and built-in waiting

---

## Best Practices Summary

### Development Workflow
1. **F5 Debug**: Launch Aspire dashboard → starts API + observability tools
2. **Local Testing**: Azurite for Table Storage emulation
3. **Code Formatting**: `dotnet format` before each commit
4. **Test-First**: Write xUnit test → verify it fails → implement → verify it passes

### Security Patterns
- HTTP-only cookies for session management
- No secrets in appsettings.json (use Aspire configuration)
- Production: Azure Key Vault for Google OAuth client secret
- HTTPS enforced in production (Container Apps default)

### Performance Targets
- Calendar load: <1s (SC-003)
- Daily logging: <10s total (SC-002)
- API p95: <200ms for CRUD operations
- Chart rendering: <2s for 90 days of data

### Cost Management
- Container Apps: Scale-to-zero when idle
- Table Storage: <$1/month for 1000 users
- App Insights: Sample at 10% to stay under $5 budget
- Budget alert: 80% threshold email to punkouter26@gmail.com

---

## Clarifications Resolved

All technical unknowns from spec have been addressed:
- ✅ Authentication model: Google OAuth with BFF pattern
- ✅ Data storage: Azure Table Storage partitioned by UserId
- ✅ Offline sync: Last-write-wins with server timestamps
- ✅ Charting library: Radzen for complex combo chart
- ✅ Architecture pattern: Vertical Slice with MediatR
- ✅ Deployment model: Azure Aspire to Container Apps

**Next Phase**: Proceed to data-model.md (Phase 1)
