# PoOmad Infrastructure

This folder contains Azure Bicep templates for deploying the PoOmad application infrastructure.

## Resources Deployed

| Resource | Purpose |
|----------|---------|
| **Log Analytics Workspace** | Centralized logging and monitoring |
| **Application Insights** | APM, telemetry, Snapshot Debugger & Profiler |
| **Storage Account** | Azure Table Storage for OMAD daily logs |
| **App Service Plan** | Linux hosting for .NET 10 |
| **App Service** | Web app hosting Blazor WASM + API |
| **Budget** | $5/month cost alert |
| **Action Group** | Email notifications for budget alerts |

## Prerequisites

1. Azure CLI installed
2. Bicep CLI installed (or use Azure CLI)
3. Azure subscription with Contributor access

## Deployment

### Using Azure CLI

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "Your-Subscription-Name"

# Create resource group
az group create --name rg-poomad-dev --location eastus

# Deploy infrastructure
az deployment group create \
  --resource-group rg-poomad-dev \
  --template-file main.bicep \
  --parameters main.bicepparam \
  --parameters budgetAlertEmail=your-email@example.com
```

### Using Azure Developer CLI (azd)

```bash
# Initialize (first time only)
azd init

# Provision infrastructure
azd provision

# Deploy application
azd deploy
```

## Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `environment` | No | `dev` | Environment name (dev, staging, prod) |
| `location` | No | Resource group location | Azure region |
| `appName` | No | `poomad` | Application name prefix |
| `budgetAlertEmail` | **Yes** | - | Email for budget alerts |
| `budgetAmount` | No | `5` | Monthly budget limit (USD) |
| `googleClientId` | No | - | Google OAuth Client ID |
| `googleClientSecret` | No | - | Google OAuth Client Secret |

## Cost Optimization

- **Free Tier (F1)**: Used for dev/staging environments
- **Basic Tier (B1)**: Used for production
- **Budget Alert**: Configured at $5/month with 80% and 100% thresholds
- **Log Retention**: 30 days to minimize storage costs

## Security Features

- **Managed Identity**: App Service uses system-assigned identity
- **HTTPS Only**: Enforced on App Service
- **TLS 1.2**: Minimum TLS version
- **FTPS Disabled**: No FTP access
- **Storage**: Private access only, TLS 1.2 minimum
