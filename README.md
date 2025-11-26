# PoOmad - The Minimalist Accountability Partner

**One Meal A Day (OMAD) tracking made simple.**

PoOmad is a minimalist web application designed to help OMAD practitioners maintain accountability through friction-free daily logging and visual streak tracking. Built with .NET 10, Azure Aspire, and Blazor WebAssembly.

## Features

- 🔐 **Secure Authentication** - Sign in with Google OAuth
- ⚡ **10-Second Logging** - Quick daily check-in (OMAD compliance, alcohol, weight)
- 📅 **Visual Calendar** - Monthly grid showing your consistency chain
- 🔥 **Streak Tracking** - Never break the chain motivation
- 📊 **Smart Analytics** - Weight trends and alcohol correlation insights
- 🌙 **Dark Mode** - Professional dark theme optimized for daily use
- 📱 **Mobile-First** - Responsive design for portrait mode
- ☁️ **Cloud Sync** - Access your data across devices with offline support

## Quick Start

See [quickstart.md](specs/001-omad-tracking-app/quickstart.md) for detailed setup instructions.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/) (for E2E tests)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)

### Run Locally

```powershell
# Start Azurite (Azure Storage emulator)
azurite --silent

# Press F5 in Visual Studio or VS Code
# - API launches at https://localhost:7001
# - Client launches at https://localhost:5001
# - Aspire Dashboard at http://localhost:15888
```

## Architecture

- **Frontend**: Blazor WebAssembly with Radzen components
- **Backend**: ASP.NET Core Minimal APIs with Vertical Slice Architecture
- **Database**: Azure Table Storage
- **Authentication**: Google OAuth with BFF pattern (HTTP-only cookies)
- **Orchestration**: Azure Aspire for service discovery and deployment
- **Deployment**: Azure Container Apps

## Project Structure

```
/
├── src/
│   ├── PoOmad.Api/          # ASP.NET Core Web API
│   ├── PoOmad.Client/       # Blazor WebAssembly
│   ├── PoOmad.Shared/       # Shared DTOs and validators
│   └── PoOmad.AppHost/      # Azure Aspire orchestration
├── tests/
│   ├── PoOmad.Api.Tests/    # xUnit backend tests
│   ├── PoOmad.Client.Tests/ # bUnit component tests
│   └── PoOmad.E2E.Tests/    # Playwright E2E tests
├── specs/                    # Feature specifications
└── docs/                     # Documentation and ADRs
```

## Development

This project follows [PoOmad Constitution](/.specify/memory/constitution.md) principles:
- ✅ Vertical Slice Architecture
- ✅ Test-Driven Development (TDD)
- ✅ 80% code coverage threshold
- ✅ Centralized package management
- ✅ One-step F5 debug launch

## Testing

```powershell
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run E2E tests (requires API running)
cd tests/PoOmad.E2E.Tests
npm test
```

## Deployment

```powershell
# Initialize Azure resources
azd init

# Provision infrastructure and deploy
azd up
```

## License

MIT

## Support

For issues or questions, please see the [specification](specs/001-omad-tracking-app/spec.md) or [implementation plan](specs/001-omad-tracking-app/plan.md).
