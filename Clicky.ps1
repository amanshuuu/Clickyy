<#
.SYNOPSIS
    Clicky - AI Buddy for Windows
    Launcher that auto-installs .NET 8 Runtime if needed.
    Double-click Clicky.ps1 or Clicky.bat to run.
#>

$ErrorActionPreference = "Continue"
$Host.UI.RawUI.WindowTitle = "Clicky - AI Buddy for Windows"

$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$ExePath = Join-Path $ScriptPath "Clicky.exe"
$ExeUrl = "https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"
$DotNetUrl = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

Clear-Host
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Clicky - AI Buddy for Windows" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ================================================================
# STEP 1: Check .NET 8 Runtime
# ================================================================
Write-Host "[1/3] Checking for .NET 8 Runtime..." -ForegroundColor Yellow

function Test-DotNet8 {
    try {
        $dotnet = Get-Command "dotnet.exe" -ErrorAction SilentlyContinue -Type Application
        if ($dotnet) {
            $runtimes = & $dotnet.Source --list-runtimes 2>$null
            if ($runtimes -match "Microsoft.WindowsDesktop.App 8\.") {
                return $true
            }
        }
    } catch {}
    try {
        $path = "HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x64\Microsoft.WindowsDesktop.App"
        if (Test-Path $path) {
            $items = Get-ChildItem $path -ErrorAction SilentlyContinue
            if ($items) { return $true }
        }
    } catch {}
    return $false
}

if (Test-DotNet8) {
    Write-Host "  [OK] .NET 8 Runtime is installed" -ForegroundColor Green
}
else {
    Write-Host "  [MISSING] .NET 8 Desktop Runtime is required" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Option 1: Install automatically (requires admin)"
    Write-Host "  Option 2: Download and install manually"
    Write-Host ""

    # Try automatic install first
    Write-Host "  Attempting automatic install..." -ForegroundColor Yellow
    $installerPath = "$env:TEMP\dotnet-runtime-desktop-8-installer.exe"
    
    try {
        Write-Host "  Downloading .NET 8 (~80MB)..." -ForegroundColor White
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($DotNetUrl, $installerPath)
    } catch {
        Write-Host "  [FAILED] Download error: $($_.Exception.Message)" -ForegroundColor Red
        $installerPath = $null
    }
    
    if ($installerPath -and (Test-Path $installerPath)) {
        Write-Host "  Installing .NET 8..." -ForegroundColor Yellow
        Write-Host "  If a UAC prompt appears, click YES" -ForegroundColor Cyan
        try {
            $proc = Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -Verb RunAs -ErrorAction Stop
            if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
                Write-Host "  [OK] .NET 8 Runtime installed!" -ForegroundColor Green
                # Remove installer temp file
                Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
            } else {
                Write-Host "  [WARN] Installer returned code $($proc.ExitCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  [FAILED] Installation cancelled or failed" -ForegroundColor Red
        }
    }
    
    # Verify install
    if (-not (Test-DotNet8)) {
        Write-Host ""
        Write-Host "  .NET 8 is still not detected. Please install manually:" -ForegroundColor Yellow
        Write-Host "  $DotNetUrl" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  After installing, run this script again." -ForegroundColor White
        Write-Host ""
        Write-Host "  Or just double-click Clicky.exe directly once .NET is installed." -ForegroundColor White
        Write-Host ""
        Read-Host "Press Enter to exit"
        exit
    }
}

# ================================================================
# STEP 2: Download Clicky.exe if missing
# ================================================================
if (Test-Path $ExePath) {
    Write-Host "[OK] Clicky.exe found" -ForegroundColor Green
}
else {
    Write-Host "[2/3] Downloading Clicky.exe (72MB)..." -ForegroundColor Yellow
    try {
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($ExeUrl, $ExePath)
        Write-Host "  [OK] Downloaded!" -ForegroundColor Green
    } catch {
        Write-Host "  [FAILED] Download error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Download manually from:" -ForegroundColor Yellow
        Write-Host "https://github.com/amanshuuu/Clickyy/releases" -ForegroundColor Cyan
        Read-Host "Press Enter to exit"
        exit
    }
}

# ================================================================
# STEP 3: Launch Clicky
# ================================================================
Write-Host "[3/3] Launching Clicky..." -ForegroundColor Green
Write-Host ""
Write-Host "Look for the blue triangle icon in your system tray (near clock)." -ForegroundColor Cyan
Write-Host "Press Ctrl+Alt to start talking!" -ForegroundColor Cyan
Write-Host ""

try {
    Start-Process -FilePath $ExePath -WindowStyle Normal
    Start-Sleep -Milliseconds 500
    
    # Check if process started
    $proc = Get-Process -Name "Clicky" -ErrorAction SilentlyContinue
    if (-not $proc) {
        Write-Host "[WARN] Clicky may not have started." -ForegroundColor Yellow
        Write-Host "Try right-clicking this script and selecting 'Run as Administrator'" -ForegroundColor White
    } else {
        Write-Host "[OK] Clicky is running! PID: $($proc.Id)" -ForegroundColor Green
    }
} catch {
    Write-Host "[FAILED] Could not launch Clicky: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "You can close this window. Clicky will keep running in the background." -ForegroundColor White
Read-Host "Press Enter to exit"
