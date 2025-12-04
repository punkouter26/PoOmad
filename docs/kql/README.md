# PoOmad KQL Monitoring Queries

This folder contains KQL (Kusto Query Language) queries for monitoring the PoOmad application in Azure Application Insights and Log Analytics.

## Query Files

| File | Purpose |
|------|---------|
| `api-health.kql` | API health and availability metrics |
| `errors.kql` | Error tracking and exception analysis |
| `latency.kql` | Response time and performance analysis |
| `user-activity.kql` | User engagement and feature usage |

## Usage

1. Open Azure Portal → Application Insights → Logs
2. Paste the query content
3. Adjust time range as needed
4. Click "Run"

## Alerting

These queries can be used to create Azure Monitor alerts:
1. Run query in Application Insights
2. Click "New alert rule"
3. Configure threshold and notification settings

## Dashboard Integration

Add these queries to Azure Dashboard:
1. Run query
2. Click "Pin to dashboard"
3. Choose existing or create new dashboard
