# Run End-to-End Tests for PoOmad
# Launches API + runs Playwright tests in sequence

param(
    [switch]$Headless = $false,
    [string]$Browser = "chromium"  # chromium, firefox, webkit
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PoOmad E2E Test Runner" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 10.0.100 or later." -ForegroundColor Red
    exit 1
}
Write-Host "  .NET SDK: $dotnetVersion" -ForegroundColor Green

# Check Node.js for Playwright
$nodeVersion = node --version 2>$null
if (-not $nodeVersion) {
    Write-Host "ERROR: Node.js not found. Please install Node.js 20+ for Playwright." -ForegroundColor Red
    exit 1
}
Write-Host "  Node.js: $nodeVersion" -ForegroundColor Green

# Check if Playwright is installed
$playwrightPath = "$PSScriptRoot/../tests/PoOmad.E2E.Tests"
if (-not (Test-Path "$playwrightPath/package.json")) {
    Write-Host "ERROR: Playwright tests not found at $playwrightPath" -ForegroundColor Red
    exit 1
}
Write-Host "  Playwright tests: Found" -ForegroundColor Green

Write-Host ""

# Step 1: Build the solution
Write-Host "Step 1: Building solution..." -ForegroundColor Cyan
Push-Location "$PSScriptRoot/.."
try {
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "  Build successful" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Build failed. Fix build errors before running E2E tests." -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host ""

# Step 2: Start Azurite (Azure Storage Emulator)
Write-Host "Step 2: Starting Azurite..." -ForegroundColor Cyan
$azuriteProcess = $null
try {
    # Check if Azurite is already running
    $azuriteRunning = Get-Process -Name "azurite" -ErrorAction SilentlyContinue
    if ($azuriteRunning) {
        Write-Host "  Azurite already running (PID: $($azuriteRunning.Id))" -ForegroundColor Yellow
    } else {
        # Start Azurite in background
        Write-Host "  Starting Azurite (this will run in background)..." -ForegroundColor Yellow
        $azuriteProcess = Start-Process "azurite" -ArgumentList "--silent" -PassThru -WindowStyle Hidden
        Start-Sleep -Seconds 3  # Give Azurite time to start
        Write-Host "  Azurite started (PID: $($azuriteProcess.Id))" -ForegroundColor Green
    }
} catch {
    Write-Host "WARNING: Could not start Azurite. Ensure it's installed: npm install -g azurite" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Start API (AppHost)
Write-Host "Step 3: Starting API via Aspire AppHost..." -ForegroundColor Cyan
$apiProcess = $null
Push-Location "$PSScriptRoot/../src/PoOmad.AppHost/PoOmad.AppHost"
try {
    Write-Host "  Launching Aspire AppHost..." -ForegroundColor Yellow
    $apiProcess = Start-Process "dotnet" -ArgumentList "run --no-build --configuration Release" -PassThru -WindowStyle Hidden
    
    # Wait for API to be ready (check health endpoint)
    Write-Host "  Waiting for API to be ready..." -ForegroundColor Yellow
    $apiReady = $false
    $retries = 0
    $maxRetries = 30
    
    while (-not $apiReady -and $retries -lt $maxRetries) {
        Start-Sleep -Seconds 2
        try {
            $response = Invoke-WebRequest -Uri "https://localhost:7001/api/health" -SkipCertificateCheck -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                $apiReady = $true
            }
        } catch {
            # API not ready yet
        }
        $retries++
        Write-Host "    Retry $retries/$maxRetries..." -ForegroundColor Gray
    }
    
    if (-not $apiReady) {
        throw "API did not become ready within timeout"
    }
    
    Write-Host "  API ready at https://localhost:7001" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to start API. Check logs for details." -ForegroundColor Red
    if ($apiProcess) {
        Stop-Process -Id $apiProcess.Id -Force
    }
    Pop-Location
    exit 1
}
Pop-Location
Write-Host ""

# Step 4: Install Playwright dependencies
Write-Host "Step 4: Installing Playwright dependencies..." -ForegroundColor Cyan
Push-Location $playwrightPath
try {
    npm install
    if ($LASTEXITCODE -ne 0) {
        throw "npm install failed"
    }
    Write-Host "  Dependencies installed" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to install Playwright dependencies." -ForegroundColor Red
    Pop-Location
    if ($apiProcess) { Stop-Process -Id $apiProcess.Id -Force }
    exit 1
}
Pop-Location
Write-Host ""

# Step 5: Run Playwright tests
Write-Host "Step 5: Running Playwright E2E tests..." -ForegroundColor Cyan
Push-Location $playwrightPath

$playwrightArgs = @("test")
if ($Headless) {
    $playwrightArgs += "--headed"
}
if ($Browser -ne "chromium") {
    $playwrightArgs += "--project=$Browser"
}

try {
    Write-Host "  Command: npx playwright $($playwrightArgs -join ' ')" -ForegroundColor Gray
    npx playwright @playwrightArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "All E2E tests passed!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "Some E2E tests failed" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: Playwright tests failed." -ForegroundColor Red
} finally {
    Pop-Location
}

Write-Host ""

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
if ($apiProcess) {
    Write-Host "  Stopping API (PID: $($apiProcess.Id))..." -ForegroundColor Gray
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
}
if ($azuriteProcess) {
    Write-Host "  Stopping Azurite (PID: $($azuriteProcess.Id))..." -ForegroundColor Gray
    Stop-Process -Id $azuriteProcess.Id -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "E2E test run complete." -ForegroundColor Cyan
