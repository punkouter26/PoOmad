# PoOmad API

## Google OAuth Setup

To enable Google authentication, you need to configure Google OAuth credentials:

### 1. Create Google OAuth Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to **Credentials** → **Create Credentials** → **OAuth 2.0 Client ID**
5. Configure the OAuth consent screen
6. Choose **Web application** as the application type
7. Add authorized redirect URIs:
   - Local development: `https://localhost:7001/api/auth/google/callback`
   - Production: `https://your-app-domain.com/api/auth/google/callback`

### 2. Configure User Secrets

Store your Google OAuth credentials securely using .NET user secrets:

```powershell
# Navigate to API project directory
cd src/PoOmad.Api/PoOmad.Api

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Set Google OAuth credentials
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID_HERE"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET_HERE"
```

### 3. Verify Configuration

The API will read these values from user secrets in development. In production, set these as environment variables or use Azure Key Vault.

**Note**: Never commit OAuth credentials to source control. The `appsettings.json` file intentionally has empty values for these settings.

## Running the API

```powershell
# Run with Aspire orchestration (recommended)
dotnet run --project ../PoOmad.AppHost/PoOmad.AppHost/PoOmad.AppHost.csproj

# Or run API directly
dotnet run
```

## Health Check

Verify the API is running:

```
GET https://localhost:7001/api/health
```
