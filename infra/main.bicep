// PoOmad Infrastructure - Main Bicep Template
// Deploys: App Service, Storage Account, Application Insights, Log Analytics, Budget, Action Group
targetScope = 'resourceGroup'

// ============================================================================
// PARAMETERS
// ============================================================================

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Application name used for resource naming')
param appName string = 'poomad'

@description('Email address for budget alerts')
param budgetAlertEmail string

@description('Monthly budget limit in USD (default $5)')
param budgetAmount int = 5

@description('Google OAuth Client ID (stored in Key Vault)')
@secure()
param googleClientId string = ''

@description('Google OAuth Client Secret (stored in Key Vault)')
@secure()
param googleClientSecret string = ''

// ============================================================================
// VARIABLES
// ============================================================================

var resourcePrefix = '${appName}-${environment}'
var tags = {
  Application: 'PoOmad'
  Environment: environment
  ManagedBy: 'Bicep'
}

// ============================================================================
// MODULES
// ============================================================================

// Log Analytics Workspace (required by Application Insights)
module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.10.1' = {
  name: 'logAnalyticsDeployment'
  params: {
    name: '${resourcePrefix}-law'
    location: location
    tags: tags
    skuName: 'PerGB2018'
    dataRetention: 30
    dailyQuotaGb: 1
  }
}

// Storage Account (Table Storage for OMAD data)
module storageAccount 'br/public:avm/res/storage/storage-account:0.19.0' = {
  name: 'storageAccountDeployment'
  params: {
    name: replace('${resourcePrefix}st', '-', '')
    location: location
    tags: tags
    skuName: 'Standard_LRS'
    kind: 'StorageV2'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    tableServices: {
      tables: [
        { name: 'DailyLogs' }
        { name: 'UserProfiles' }
      ]
    }
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

// Application Insights
module appInsights 'br/public:avm/res/insights/component:0.6.0' = {
  name: 'appInsightsDeployment'
  params: {
    name: '${resourcePrefix}-ai'
    location: location
    tags: tags
    workspaceResourceId: logAnalytics.outputs.resourceId
    kind: 'web'
    applicationType: 'web'
    retentionInDays: 30
    disableIpMasking: false
    disableLocalAuth: false
  }
}

// App Service Plan
module appServicePlan 'br/public:avm/res/web/serverfarm:0.4.1' = {
  name: 'appServicePlanDeployment'
  params: {
    name: '${resourcePrefix}-asp'
    location: location
    tags: tags
    skuName: environment == 'prod' ? 'B1' : 'F1'
    skuCapacity: 1
    kind: 'linux'
    reserved: true
  }
}

// App Service (Web App)
module appService 'br/public:avm/res/web/site:0.15.1' = {
  name: 'appServiceDeployment'
  params: {
    name: '${resourcePrefix}-app'
    location: location
    tags: tags
    kind: 'app,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: environment == 'prod'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      healthCheckPath: '/api/health'
    }
    appSettingsKeyValuePairs: {
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      ApplicationInsights__ConnectionString: appInsights.outputs.connectionString
      ASPNETCORE_ENVIRONMENT: environment == 'prod' ? 'Production' : 'Development'
      'ConnectionStrings__TableStorage': 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.outputs.name};AccountKey=${storageAccount.outputs.primaryBlobEndpoint};EndpointSuffix=core.windows.net'
      'Authentication__Google__ClientId': googleClientId
      'Authentication__Google__ClientSecret': googleClientSecret
    }
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalytics.outputs.resourceId
        logCategoriesAndGroups: [
          { categoryGroup: 'allLogs' }
        ]
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ]
  }
}

// Action Group for Budget Alerts
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${resourcePrefix}-ag'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'BudgetAlert'
    enabled: true
    emailReceivers: [
      {
        name: 'BudgetAlertEmail'
        emailAddress: budgetAlertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

// Budget with $5/month limit
resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: '${resourcePrefix}-budget'
  properties: {
    category: 'Cost'
    amount: budgetAmount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: '${substring(utcNow(), 0, 7)}-01'
    }
    notifications: {
      Actual_GreaterThan_80_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 80
        contactEmails: [budgetAlertEmail]
        contactGroups: [actionGroup.id]
        thresholdType: 'Actual'
      }
      Actual_GreaterThan_100_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        contactEmails: [budgetAlertEmail]
        contactGroups: [actionGroup.id]
        thresholdType: 'Actual'
      }
      Forecasted_GreaterThan_100_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        contactEmails: [budgetAlertEmail]
        contactGroups: [actionGroup.id]
        thresholdType: 'Forecasted'
      }
    }
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

@description('App Service default hostname')
output appServiceHostname string = appService.outputs.defaultHostname

@description('App Service resource ID')
output appServiceResourceId string = appService.outputs.resourceId

@description('Storage Account name')
output storageAccountName string = storageAccount.outputs.name

@description('Application Insights connection string')
output appInsightsConnectionString string = appInsights.outputs.connectionString

@description('Log Analytics Workspace ID')
output logAnalyticsWorkspaceId string = logAnalytics.outputs.logAnalyticsWorkspaceId
