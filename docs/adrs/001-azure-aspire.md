# ADR-001: Azure Aspire for Orchestration

**Status**: Accepted  
**Date**: 2025-11-23  
**Context**: Phase 1 - Project Setup

## Context and Problem Statement

We need a local development orchestrator that:
- Simplifies multi-service development (API + Storage + Observability)
- Provides seamless cloud deployment to Azure Container Apps
- Handles service discovery and configuration management
- Supports F5 debug experience with one-step startup

Traditional approaches (Docker Compose, Kubernetes locally) require significant configuration overhead and don't align with Azure cloud-native deployment.

## Decision Drivers

- **Developer Experience**: One-step F5 debug launch (Constitution requirement)
- **Cloud Alignment**: Direct path from local dev to Azure Container Apps
- **Service Discovery**: Automatic configuration of connection strings and endpoints
- **Observability**: Built-in integration with Application Insights and OpenTelemetry
- **Simplicity**: Minimal configuration files, prefer code-based orchestration

## Considered Options

### Option 1: Azure Aspire (Chosen)

**Pros**:
- ✅ One-line service registration: `builder.AddProject<Projects.PoOmad_Api>("api")`
- ✅ Automatic service discovery via Aspire SDK
- ✅ Built-in Azurite (Azure Storage Emulator) integration
- ✅ Direct deployment to Azure Container Apps via `azd up`
- ✅ Dashboard UI for monitoring services locally
- ✅ OpenTelemetry configured automatically
- ✅ Connection strings injected via configuration

**Cons**:
- ⚠️ Requires .NET 10 (acceptable - we're using latest)
- ⚠️ Relatively new (v13.0.0) - smaller community
- ⚠️ Tied to Azure ecosystem (acceptable - we're deploying to Azure)

### Option 2: Docker Compose

**Pros**:
- ✅ Industry standard
- ✅ Large community and documentation
- ✅ Cloud-agnostic

**Cons**:
- ❌ Requires separate docker-compose.yml file
- ❌ No automatic service discovery
- ❌ Manual connection string configuration
- ❌ Extra step to deploy to Azure (build images, push to ACR, deploy)
- ❌ No built-in observability dashboard

### Option 3: Kubernetes (Minikube/Kind locally)

**Pros**:
- ✅ Production-like local environment
- ✅ Cloud-agnostic

**Cons**:
- ❌ Extremely complex for a simple 2-service app
- ❌ Requires YAML manifests for every resource
- ❌ Slow local startup times
- ❌ Overkill for single-user OMAD tracker

### Option 4: Manual Launch (no orchestrator)

**Pros**:
- ✅ No dependencies

**Cons**:
- ❌ Violates Constitution requirement: "F5 should launch both API and browser"
- ❌ Developer must manually start Azurite, API, and Blazor client
- ❌ No service discovery - hardcoded URLs
- ❌ No observability dashboard

## Decision Outcome

**Chosen**: Azure Aspire

### Implementation

```csharp
// src/PoOmad.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var tables = storage.AddTables("tables");

var api = builder.AddProject<Projects.PoOmad_Api>("api")
    .WithReference(tables);

builder.AddProject<Projects.PoOmad_Client>("client")
    .WithReference(api);

builder.Build().Run();
```

### Justification

1. **F5 Experience**: Single launch.json entry starts AppHost → all services launch
2. **Zero Configuration**: Connection strings auto-injected via `builder.Configuration`
3. **Cloud Path**: `azd up` deploys directly to Azure Container Apps
4. **Built-in Dashboard**: Navigate to http://localhost:15000 to see all services
5. **OpenTelemetry**: Traces/metrics flow to dashboard without extra config

### Trade-offs

- **Azure Lock-in**: Aspire is Azure-first. Migrating to AWS/GCP would require rework.
  - **Mitigation**: We're already using Azure Table Storage, so we're committed to Azure
- **Maturity**: Aspire is new (GA in 2024). May have fewer resources than Docker Compose.
  - **Mitigation**: Microsoft-backed with strong documentation and active development

## Consequences

### Positive

- Developers can F5 debug the entire stack instantly
- Service discovery eliminates hardcoded URLs
- Deployment to Azure is streamlined (no Dockerfile management)
- Built-in observability reduces troubleshooting time

### Negative

- Team must learn Aspire-specific APIs (small learning curve)
- Cloud provider migration would require orchestration rework
- Some IDE features (e.g., Docker Compose integration) not applicable

## Validation

**Success Criteria** (from quickstart.md):
- ✅ F5 launches Aspire AppHost
- ✅ AppHost starts Azurite, API, and Blazor client
- ✅ Browser opens automatically to Blazor app
- ✅ Developer can set breakpoints in API and client
- ✅ Dashboard shows all services healthy

## References

- [Azure Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire GitHub Repository](https://github.com/dotnet/aspire)
- [Azure Container Apps Deployment Guide](https://learn.microsoft.com/azure/container-apps/)
- [Constitution Requirement: One-step F5 debug](../../specs/001-omad-tracking-app/speckit.constitution.md)

## Related Decisions

- **ADR-002**: Table Storage choice (Aspire has built-in Azurite support)
- **Phase 9 Tasks**: Azure deployment uses `azd deploy` command
