<#
.SYNOPSIS
    Clicky - AI Buddy for Windows
    Launcher that checks for .NET 8 Runtime, downloads Clicky.exe
    if missing, then launches Clicky.
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
# STEP 1: Check .NET 8 Desktop Runtime via multiple methods
# ================================================================
Write-Host "[1/3] Checking for .NET 8 Runtime..." -ForegroundColor Yellow

function Test-DotNet8Installed {
    # Method 1: Check registry (most reliable for Desktop Runtime only)
    $registryPaths = @(
        "HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App",
        "HKLM:\SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App",
        "HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.WindowsDesktop.App"
    )
    foreach ($path in $registryPaths) {
        if (Test-Path $path) {
            $items = Get-ChildItem $path -ErrorAction SilentlyContinue
            if ($items) {
                Write-Host "  (found registry: $path)" -ForegroundColor Gray
                return $true
            }
        }
    }

    # Method 2: Check install directory
    $installPaths = @(
        "$env:ProgramFiles\dotnet\shared\Microsoft.WindowsDesktop.App",
        "${env:ProgramFiles(x86)}\dotnet\shared\Microsoft.WindowsDesktop.App"
    )
    foreach ($path in $installPaths) {
        if (Test-Path $path) {
            $items = Get-ChildItem $path -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "8.*" }
            if ($items) {
                Write-Host "  (found files: $path)" -ForegroundColor Gray
                return $true
            }
        }
    }

    # Method 3: Check via dotnet CLI (if SDK is installed)
    try {
        $dotnet = Get-Command "dotnet.exe" -ErrorAction SilentlyContinue -Type Application
        if ($dotnet) {
            $runtimes = & $dotnet.Source --list-runtimes 2>$null
            if ($runtimes -match "Microsoft.WindowsDesktop.App 8\.") {
                return $true
            }
        }
    } catch {}

    return $false
}

$dotnetInstalled = Test-DotNet8Installed

if ($dotnetInstalled) {
    Write-Host "  [OK] .NET 8 Runtime is installed" -ForegroundColor Green
}
else {
    Write-Host "  [MISSING] .NET 8 Desktop Runtime is required" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Please download and install from Microsoft:" -ForegroundColor White
    Write-Host "  $DotNetUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  After installing, double-click Clicky.exe directly to run." -ForegroundColor White
    Write-Host "  (No need to run this script again - the EXE works once .NET is installed)" -ForegroundColor White
    Write-Host ""

    # Offer automatic install as backup
    Write-Host "  Or try automatic install (requires admin):" -ForegroundColor Yellow
    $installChoice = Read-Host "  Download and install .NET 8 automatically? (y/N)"
    
    if ($installChoice -eq "y" -or $installChoice -eq "Y") {
        $installerPath = "$env:TEMP\dotnet-runtime-desktop-8-installer.exe"
        try {
            Write-Host "  Downloading .NET 8 (~80MB)..." -ForegroundColor White
            $wc = New-Object System.Net.WebClient
            $wc.DownloadFile($DotNetUrl, $installerPath)
            Write-Host "  Download complete!" -ForegroundColor Green
            Write-Host "  Installing... (UAC prompt may appear - click YES)" -ForegroundColor Yellow
            $proc = Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -Verb RunAs -ErrorAction Stop
            
            if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
                Write-Host "  Installer finished. Checking installation..." -ForegroundColor Yellow
                # Remove installer
                Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
                
                # Check again
                if (Test-DotNet8Installed) {
                    Write-Host "  [OK] .NET 8 Runtime installed and detected!" -ForegroundColor Green
                    $dotnetInstalled = $true
                } else {
                    Write-Host "  [WARN] .NET installer ran but could not verify." -ForegroundColor Yellow
                    Write-Host "  Please restart your computer, then run Clicky.exe directly." -ForegroundColor White
                }
            } else {
                Write-Host "  [WARN] Installer returned code $($proc.ExitCode)" -ForegroundColor Yellow
                Write-Host "  Please install manually from: $DotNetUrl" -ForegroundColor Cyan
            }
        } catch {
            Write-Host "  [FAILED] $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "  Install manually from: $DotNetUrl" -ForegroundColor Cyan
        }
    }
    
    if (-not $dotnetInstalled) {
        Write-Host ""
        Write-Host "  Step-by-step:" -ForegroundColor White
        Write-Host "  1. Open this link in your browser:" -ForegroundColor White
        Write-Host "     $DotNetUrl" -ForegroundColor Cyan
        Write-Host "  2. Run the downloaded installer" -ForegroundColor White
        Write-Host "  3. Double-click Clicky.exe" -ForegroundColor White
        Write-Host ""
        Read-Host "  Press Enter to exit"
        exit
    }
}

# ================================================================
# STEP 2: Download Clicky.exe if missing
# ================================================================
$exeFound = Test-Path $ExePath

if ($exeFound) {
    Write-Host "[OK] Clicky.exe found" -ForegroundColor Green
}
else {
    Write-Host "[2/3] Downloading Clicky.exe (72MB)..." -ForegroundColor Yellow
    try {
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($ExeUrl, $ExePath)
        Write-Host "  [OK] Downloaded!" -ForegroundColor Green
        $exeFound = $true
    } catch {
        Write-Host "  [FAILED] Download error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Download manually:" -ForegroundColor Yellow
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
    Start-Sleep -Milliseconds 1000
    
    $proc = Get-Process -Name "Clicky" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "[OK] Clicky is running! PID: $($proc.Id)" -ForegroundColor Green
    } else {
        Write-Host "[WARN] Clicky may not have started." -ForegroundColor Yellow
        Write-Host "Try right-clicking Clicky.exe and select 'Run as Administrator'" -ForegroundColor White
    }
} catch {
    Write-Host "[FAILED] Could not launch: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "You can close this window - Clicky keeps running in the background." -ForegroundColor White
Read-Host "Press Enter to exit"
