# Seed Test Data for PoOmad
# Generates 90 days of realistic OMAD tracking data for 3 test users

param(
    [string]$ApiUrl = "https://localhost:7001",
    [switch]$UseAzurite = $true
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PoOmad Test Data Seeding Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test user profiles
$testUsers = @(
    @{
        GoogleId = "test-user-001-google-id-guid"
        Email = "user1@test.com"
        Height = "5'10`""
        StartingWeight = 180
        StartDate = (Get-Date).AddDays(-90).ToString("yyyy-MM-dd")
    },
    @{
        GoogleId = "test-user-002-google-id-guid"
        Email = "user2@test.com"
        Height = "5'6`""
        StartingWeight = 165
        StartDate = (Get-Date).AddDays(-90).ToString("yyyy-MM-dd")
    },
    @{
        GoogleId = "test-user-003-google-id-guid"
        Email = "user3@test.com"
        Height = "6'2`""
        StartingWeight = 210
        StartDate = (Get-Date).AddDays(-90).ToString("yyyy-MM-dd")
    }
)

# Function to generate realistic OMAD compliance pattern
function Get-OmadCompliance {
    param([int]$Day)
    
    # 70% success rate with some streaks
    $rand = Get-Random -Minimum 0 -Maximum 100
    
    # Weekends are harder (60% success)
    if ((Get-Date).AddDays(-$Day).DayOfWeek -in @("Saturday", "Sunday")) {
        return $rand -lt 60
    }
    
    # Weekdays are easier (80% success)
    return $rand -lt 80
}

# Function to generate realistic alcohol consumption
function Get-AlcoholConsumption {
    param([int]$Day, [bool]$OmadCompliant)
    
    # Less likely to drink if OMAD was successful
    if ($OmadCompliant) {
        $rand = Get-Random -Minimum 0 -Maximum 100
        return $rand -lt 20  # 20% chance
    }
    
    # More likely to drink if OMAD failed
    $rand = Get-Random -Minimum 0 -Maximum 100
    return $rand -lt 40  # 40% chance
}

# Function to generate realistic weight progression
function Get-Weight {
    param(
        [decimal]$StartingWeight,
        [int]$Day,
        [bool]$OmadCompliant,
        [bool]$AlcoholConsumed,
        [decimal]$PreviousWeight
    )
    
    # Trend: gradual weight loss if mostly OMAD compliant
    $trend = -0.05  # -0.05 lbs per day average
    
    # Daily variance
    $variance = Get-Random -Minimum -1.5 -Maximum 1.5
    
    # Weight increases if alcohol consumed
    if ($AlcoholConsumed) {
        $variance += 1.0
    }
    
    # Weight decreases if OMAD compliant
    if ($OmadCompliant) {
        $variance -= 0.5
    }
    
    # Calculate new weight
    $newWeight = $PreviousWeight + $trend + $variance
    
    # Ensure weight stays in realistic range (don't go below 20 lbs of starting)
    if ($newWeight -lt ($StartingWeight - 20)) {
        $newWeight = $StartingWeight - 20
    }
    
    return [Math]::Round($newWeight, 1)
}

# Function to insert profile via Azure Table Storage SDK (direct)
function New-UserProfile {
    param($User)
    
    Write-Host "Creating profile for $($User.Email)..." -ForegroundColor Yellow
    
    # In a real scenario, you would use Azure.Data.Tables SDK here
    # For now, we'll output the data structure
    
    $profile = @{
        PartitionKey = $User.GoogleId
        RowKey = "profile"
        Email = $User.Email
        Height = $User.Height
        StartingWeight = $User.StartingWeight
        StartDate = $User.StartDate
    }
    
    Write-Host "  GoogleId: $($User.GoogleId)" -ForegroundColor Gray
    Write-Host "  Email: $($User.Email)" -ForegroundColor Gray
    Write-Host "  Height: $($User.Height)" -ForegroundColor Gray
    Write-Host "  Starting Weight: $($User.StartingWeight) lbs" -ForegroundColor Gray
    Write-Host "  Start Date: $($User.StartDate)" -ForegroundColor Gray
    
    return $profile
}

# Function to insert daily log via Azure Table Storage SDK (direct)
function New-DailyLog {
    param(
        [string]$GoogleId,
        [string]$Date,
        [bool]$OmadCompliant,
        [bool]$AlcoholConsumed,
        [decimal]$Weight
    )
    
    $log = @{
        PartitionKey = $GoogleId
        RowKey = $Date
        OmadCompliant = $OmadCompliant
        AlcoholConsumed = $AlcoholConsumed
        Weight = $Weight
        ServerTimestamp = [DateTimeOffset]::UtcNow
    }
    
    return $log
}

# Main seeding logic
Write-Host "Seeding test data for 3 users with 90 days of logs..." -ForegroundColor Green
Write-Host ""

if ($UseAzurite) {
    Write-Host "Using Azurite (local Azure Storage Emulator)" -ForegroundColor Cyan
    Write-Host "Make sure Azurite is running (it should start with Aspire AppHost)" -ForegroundColor Cyan
    Write-Host ""
}

# Load Azure.Data.Tables SDK (requires NuGet package)
# Note: This script assumes you have Azure.Data.Tables DLL available
# In production, you would install it: Install-Package Azure.Data.Tables

try {
    Add-Type -Path "$PSScriptRoot/../src/PoOmad.Api/PoOmad.Api/bin/Debug/net10.0/Azure.Data.Tables.dll" -ErrorAction SilentlyContinue
} catch {
    Write-Host "WARNING: Could not load Azure.Data.Tables SDK. Using mock data output instead." -ForegroundColor Yellow
    Write-Host "To actually seed data, ensure the API project is built." -ForegroundColor Yellow
    Write-Host ""
}

$connectionString = "UseDevelopmentStorage=true"  # Azurite connection string

foreach ($user in $testUsers) {
    Write-Host "Processing user: $($user.Email)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    # Create profile
    $profile = New-UserProfile -User $user
    
    # Generate 90 days of logs
    $currentWeight = $user.StartingWeight
    $logsCreated = 0
    $streakCount = 0
    $maxStreak = 0
    
    for ($i = 89; $i -ge 0; $i--) {
        $date = (Get-Date).AddDays(-$i)
        $dateString = $date.ToString("yyyy-MM-dd")
        
        # Skip some days randomly (10% chance) to simulate unlogged days
        $skipDay = (Get-Random -Minimum 0 -Maximum 100) -lt 10
        if ($skipDay -and $i -gt 0) {
            # Don't skip today
            continue
        }
        
        $omadCompliant = Get-OmadCompliance -Day $i
        $alcoholConsumed = Get-AlcoholConsumption -Day $i -OmadCompliant $omadCompliant
        $currentWeight = Get-Weight -StartingWeight $user.StartingWeight -Day $i -OmadCompliant $omadCompliant -AlcoholConsumed $alcoholConsumed -PreviousWeight $currentWeight
        
        $log = New-DailyLog -GoogleId $user.GoogleId -Date $dateString -OmadCompliant $omadCompliant -AlcoholConsumed $alcoholConsumed -Weight $currentWeight
        
        # Track streak
        if ($omadCompliant) {
            $streakCount++
            if ($streakCount -gt $maxStreak) {
                $maxStreak = $streakCount
            }
        } else {
            $streakCount = 0
        }
        
        $logsCreated++
        
        # Output sample data every 10 days
        if ($i % 10 -eq 0) {
            Write-Host "  Day -$($i): OMAD=$omadCompliant, Alcohol=$alcoholConsumed, Weight=$currentWeight lbs" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Summary for $($user.Email):" -ForegroundColor Green
    Write-Host "  Logs Created: $logsCreated" -ForegroundColor Gray
    Write-Host "  Current Weight: $currentWeight lbs (started at $($user.StartingWeight) lbs)" -ForegroundColor Gray
    Write-Host "  Weight Change: $([Math]::Round($currentWeight - $user.StartingWeight, 1)) lbs" -ForegroundColor Gray
    Write-Host "  Max Streak: $maxStreak days" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "Test data seeding complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Launch the app with F5 (Aspire AppHost)" -ForegroundColor White
Write-Host "2. Sign in with one of these test emails:" -ForegroundColor White
Write-Host "   - user1@test.com" -ForegroundColor Yellow
Write-Host "   - user2@test.com" -ForegroundColor Yellow
Write-Host "   - user3@test.com" -ForegroundColor Yellow
Write-Host "3. View calendar dashboard and analytics" -ForegroundColor White
Write-Host ""
Write-Host "NOTE: This script currently outputs data structures." -ForegroundColor Yellow
Write-Host "To actually insert data into Azurite, implement Azure.Data.Tables SDK calls." -ForegroundColor Yellow
Write-Host "Alternatively, use the API endpoints after authentication." -ForegroundColor Yellow
