<#
.SYNOPSIS
    Build Clicky for Windows from source.
    Run this on your Windows PC - it installs .NET SDK and compiles everything.
#>

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Building Clicky for Windows"

$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionPath = Join-Path $ScriptPath "ClickyWindows.sln"
$ProjectPath = Join-Path $ScriptPath "ClickyWindows\ClickyWindows.csproj"
$OutputExe = Join-Path $ScriptPath "Clicky.exe"

Clear-Host
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Building Clicky for Windows" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ================================================================
# STEP 1: Check/Install .NET 8 SDK
# ================================================================
Write-Host "[1/4] Checking .NET 8 SDK..." -ForegroundColor Yellow

$sdkInstalled = $false
try {
    $dotnet = Get-Command "dotnet.exe" -ErrorAction SilentlyContinue -Type Application
    if ($dotnet) {
        $sdkVersion = & $dotnet.Source --version 2>$null
        if ($sdkVersion -and $sdkVersion.StartsWith("8.")) {
            Write-Host "  [OK] .NET 8 SDK $sdkVersion detected" -ForegroundColor Green
            $sdkInstalled = $true
        }
    }
} catch {}

if (-not $sdkInstalled) {
    Write-Host "  [MISSING] .NET 8 SDK not found. Downloading..." -ForegroundColor Yellow
    $sdkUrl = "https://aka.ms/dotnet/8.0/dotnet-sdk-win-x64.exe"
    $installerPath = "$env:TEMP\dotnet-sdk-8-installer.exe"
    
    try {
        $wc = New-Object System.Net.WebClient
        Write-Host "  Downloading .NET 8 SDK (~200MB)..." -ForegroundColor White
        $wc.DownloadFile($sdkUrl, $installerPath)
        Write-Host "  Installing SDK (this may take a minute)..." -ForegroundColor Yellow
        $proc = Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -Verb RunAs
        
        # Refresh PATH
        $env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
        
        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
            Write-Host "  [OK] .NET 8 SDK installed!" -ForegroundColor Green
            # Need to restart to use dotnet command
            Write-Host "  [INFO] You need to restart this script or reopen your terminal." -ForegroundColor Yellow
            Write-Host "  After restart, run this script again from Step 2."
            Read-Host "Press Enter to exit"
            exit
        }
    } catch {
        Write-Host "  [FAILED] $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Install SDK manually from: $sdkUrl" -ForegroundColor Cyan
        Read-Host "Press Enter to exit"
        exit
    }
}

# ================================================================
# STEP 2: Restore NuGet packages
# ================================================================
Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
try {
    & dotnet restore "$SolutionPath" --runtime win-x64
    Write-Host "  [OK] Packages restored" -ForegroundColor Green
} catch {
    Write-Host "  [FAILED] Restore failed: $($_.Exception.Message)" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

# ================================================================
# STEP 3: Build Clicky
# ================================================================
Write-Host "[3/4] Building Clicky (framework-dependent)..." -ForegroundColor Yellow
try {
    & dotnet build "$ProjectPath" -c Release --runtime win-x64 --no-restore
    Write-Host "  [OK] Build complete!" -ForegroundColor Green
} catch {
    Write-Host "  [FAILED] Build failed: $($_.Exception.Message)" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

# Copy EXE to root folder
$BuildExe = Join-Path $ScriptPath "ClickyWindows\bin\Release\net8.0-windows\win-x64\Clicky.exe"
if (Test-Path $BuildExe) {
    Copy-Item $BuildExe $OutputExe -Force
    Write-Host "  [OK] Clicky.exe copied to: $OutputExe" -ForegroundColor Green
}

# ================================================================
# STEP 4: Check .NET Runtime and launch
# ================================================================
Write-Host "[4/4] Checking .NET 8 Runtime..." -ForegroundColor Yellow

$runtimeInstalled = $false
try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes -match "Microsoft.WindowsDesktop.App 8\.") {
        $runtimeInstalled = $true
    }
} catch {}

if (-not $runtimeInstalled) {
    Write-Host "  [MISSING] .NET 8 Desktop Runtime required to run Clicky." -ForegroundColor Yellow
    Write-Host "  Download from: https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe" -ForegroundColor Cyan
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "  [OK] .NET 8 Runtime found" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE! Launching Clicky..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    Start-Process -FilePath $OutputExe -WindowStyle Normal
    Start-Sleep -Milliseconds 1000
    $proc = Get-Process -Name "Clicky" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "[OK] Clicky is running! PID: $($proc.Id)" -ForegroundColor Green
    }
} catch {
    Write-Host "[FAILED] Launch error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Look for the blue triangle icon in your system tray." -ForegroundColor Cyan
Write-Host "Press Ctrl+Alt to start talking!" -ForegroundColor Cyan
Read-Host "Press Enter to exit"
