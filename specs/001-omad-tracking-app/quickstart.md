# PoOmad Quickstart Guide

**Last Updated**: 2025-11-22  
**Purpose**: Get PoOmad running locally with one-step F5 debug (Constitution requirement)

---

## Prerequisites

### Required Software
- [.NET 8 or .NET 9 SDK](https://dotnet.microsoft.com/download) (as specified in `global.json`)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) or [VS Code](https://code.visualstudio.com/) with C# extension
- [Node.js 20+](https://nodejs.org/) (for Playwright E2E tests)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (Azure Table Storage emulator)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerization)

### Optional but Recommended
- [Visual Studio Code REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) (to run `.http` files)
- [Azure Storage Explorer](https://azure.microsoft.com/features/storage-explorer/) (to inspect Azurite tables)

---

## Initial Setup

### 1. Clone Repository
```powershell
git clone https://github.com/your-org/PoOmad.git
cd PoOmad
git checkout 001-omad-tracking-app
```

### 2. Install .NET Workloads
```powershell
dotnet workload install aspire
```

### 3. Restore Dependencies
```powershell
dotnet restore
```

### 4. Set Up Google OAuth (Required for Authentication)

#### 4.1 Create Google OAuth App
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project: "PoOmad-Dev"
3. Enable Google+ API
4. Create OAuth 2.0 Client ID:
   - Application type: Web application
   - Authorized redirect URIs: `https://localhost:7001/api/auth/google/callback`
5. Copy `Client ID` and `Client Secret`

#### 4.2 Configure User Secrets (Local Development)
```powershell
cd src/PoOmad.Api
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET"
```

**Note**: Constitution requires appsettings.json for local config, but user secrets are used for OAuth credentials to prevent accidental git commits.

### 5. Start Azurite (Azure Table Storage Emulator)
```powershell
# Option 1: Start Azurite via npm (if installed globally)
azurite --silent --location c:\azurite --debug c:\azurite\debug.log

# Option 2: Start Azurite via Docker
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

# Option 3: Start Azurite via Visual Studio (included in Azure development workload)
# Tools → Options → Azure Service Authentication → Start Azurite
```

**Verify Azurite is running**:
- Table Storage endpoint: `http://127.0.0.1:10002/devstoreaccount1`

---

## One-Step F5 Debug (Constitution Requirement)

### Visual Studio 2022

1. Open `PoOmad.sln`
2. Set `PoOmad.AppHost` as startup project (right-click → Set as Startup Project)
3. Press **F5** or click **Debug → Start Debugging**

**What happens**:
- Azure Aspire orchestrator (`PoOmad.AppHost`) starts
- Aspire Dashboard opens in browser at `http://localhost:15888`
- `PoOmad.Api` (backend) starts at `https://localhost:7001`
- `PoOmad.Client` (Blazor WASM) starts at `https://localhost:5001`
- Aspire automatically configures service discovery between components
- Breakpoints in both API and Client code will work

### VS Code

1. Open folder in VS Code
2. Install recommended extensions (C# Dev Kit, Aspire)
3. Open Command Palette (`Ctrl+Shift+P`)
4. Run: `.NET: Generate Assets for Build and Debug`
5. Press **F5**

**If F5 doesn't work**, use this `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Aspire App",
      "type": "dotnet",
      "request": "launch",
      "projectPath": "${workspaceFolder}/src/PoOmad.AppHost/PoOmad.AppHost.csproj",
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "Now listening on: (https?://\\S+)",
        "uriFormat": "%s"
      }
    }
  ]
}
```

---

## Verify Everything Works

### 1. Aspire Dashboard
Open `http://localhost:15888` to see:
- **Resources**: PoOmad.Api, PoOmad.Client, Azure Table Storage
- **Traces**: Distributed tracing across services
- **Logs**: Real-time logs from all services
- **Metrics**: Telemetry dashboards

### 2. API Health Check
```powershell
curl https://localhost:7001/api/health
```
Expected response:
```json
{
  "status": "Healthy",
  "checks": {
    "tableStorage": "Healthy",
    "authentication": "Healthy"
  }
}
```

### 3. Blazor WASM App
1. Open `https://localhost:5001` in browser
2. Click "Sign in with Google"
3. Complete OAuth flow (redirects to Google, then back)
4. You should see the setup wizard (no profile exists yet)
5. Enter height and starting weight
6. Verify redirect to calendar dashboard

### 4. Manual API Testing (.http files)
1. Install VS Code REST Client extension
2. Open `specs/001-omad-tracking-app/contracts/profile.http`
3. Click "Send Request" on any endpoint
4. Verify responses match documented contracts

---

## Common Issues & Troubleshooting

### Issue: "Azurite is not running"
**Solution**:
```powershell
# Check if Azurite is running
netstat -ano | findstr :10002

# If not running, start it:
azurite --silent
```

### Issue: "Google OAuth redirect mismatch"
**Error**: `redirect_uri_mismatch`

**Solution**:
1. Verify redirect URI in Google Cloud Console matches exactly: `https://localhost:7001/api/auth/google/callback`
2. Ensure you're using `https`, not `http`
3. Check `appsettings.Development.json` has correct `CallbackPath`

### Issue: "Cannot connect to Table Storage"
**Solution**:
1. Verify Azurite is running on port 10002
2. Check `appsettings.Development.json` connection string:
   ```json
   {
     "ConnectionStrings": {
       "TableStorage": "UseDevelopmentStorage=true"
     }
   }
   ```

### Issue: "Blazor app shows blank page"
**Solution**:
1. Open browser DevTools (F12)
2. Check Console for errors
3. Common fix: Clear browser cache, hard refresh (`Ctrl+Shift+R`)

### Issue: "Aspire dashboard doesn't open"
**Solution**:
```powershell
# Check if port 15888 is in use
netstat -ano | findstr :15888

# If blocked, kill the process or change port in PoOmad.AppHost/Program.cs
```

---

## Development Workflow

### Daily Development
1. Start Azurite (one-time, runs in background)
2. Press F5 in Visual Studio
3. Make code changes
4. Hot reload applies changes automatically (no restart needed)
5. Set breakpoints in API or Client code as needed

### Running Tests

#### Unit + Integration Tests (xUnit)
```powershell
dotnet test tests/PoOmad.Api.Tests/PoOmad.Api.Tests.csproj
```

#### Component Tests (bUnit)
```powershell
dotnet test tests/PoOmad.Client.Tests/PoOmad.Client.Tests.csproj
```

#### E2E Tests (Playwright)
```powershell
# Install Playwright browsers (one-time)
cd tests/PoOmad.E2E.Tests
npm install
npx playwright install chromium

# Run E2E tests (requires API to be running)
npm test
```

#### Code Coverage Report
```powershell
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:docs/coverage -reporttypes:Html
# Open docs/coverage/index.html in browser
```

---

## Next Steps

### Implement Features (TDD Workflow)
1. Pick a task from `specs/001-omad-tracking-app/tasks.md` (generated via `/speckit.tasks`)
2. Write failing test first (Red)
3. Implement feature to make test pass (Green)
4. Refactor code while keeping tests passing (Refactor)
5. Commit with descriptive message

### Deploy to Azure
```powershell
# Initialize Azure deployment (one-time)
azd init

# Provision infrastructure + deploy
azd up

# After deployment, configure production secrets:
azd env set GOOGLE_CLIENT_ID "your_production_client_id"
azd env set GOOGLE_CLIENT_SECRET "your_production_client_secret"
```

### Monitor Production
- Application Insights: Azure Portal → PoOmad-rg → Application Insights
- KQL Queries: Use queries from `docs/kql/` folder
- Budget Alerts: Configured for 80% of $5/month threshold

---

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│           Azure Aspire Orchestrator             │
│            (PoOmad.AppHost)                     │
└────────────┬────────────────────────────────────┘
             │
      ┌──────┴───────┐
      │              │
      ▼              ▼
┌──────────┐   ┌──────────────┐
│ PoOmad   │   │ PoOmad.Api   │
│ .Client  │◄──┤ (Backend)    │
│ (Blazor  │   │ Minimal APIs │
│  WASM)   │   │ + MediatR    │
└──────────┘   └──────┬───────┘
                      │
                      ▼
               ┌─────────────────┐
               │ Azure Table     │
               │ Storage         │
               │ (Azurite local) │
               └─────────────────┘
```

---

## Constitution Compliance Checklist

- ✅ One-step F5 debug launch (this guide)
- ✅ Azurite for local storage (Azure Table Storage emulator)
- ✅ appsettings.json for local config (user secrets for OAuth)
- ✅ Health check at `/api/health` validates dependencies
- ✅ Swagger UI at `https://localhost:7001/swagger` for API docs
- ✅ `.http` files in `specs/001-omad-tracking-app/contracts/` for manual testing

---

## Further Reading

- [Azure Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor WebAssembly Guide](https://learn.microsoft.com/aspnet/core/blazor/)
- [Azure Table Storage Best Practices](https://learn.microsoft.com/azure/storage/tables/table-storage-design)
- [Google OAuth 2.0 for Web Apps](https://developers.google.com/identity/protocols/oauth2/web-server)
- [Playwright TypeScript Guide](https://playwright.dev/docs/intro)
