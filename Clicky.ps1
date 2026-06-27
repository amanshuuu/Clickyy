<#
.SYNOPSIS
    Clicky - AI Buddy for Windows
    Auto-installs .NET 8 Runtime if missing, auto-downloads Clicky.exe
    if missing, then launches. Works standalone or from ZIP.
#>

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Clicky - AI Buddy for Windows"

$ExeUrl = "https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"
$DotNetUrl = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ExePath = Join-Path $ScriptDir "Clicky.exe"

function Write-Header {
    Clear-Host
    Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║         Clicky - AI Buddy for Windows          ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

Write-Header

# ================================================================
# STEP 1: Check .NET 8 Desktop Runtime
# ================================================================
function Test-DotNet8Runtime {
    try {
        $dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
        if ($dotnet) {
            $runtimes = & dotnet --list-runtimes 2>$null
            if ($runtimes -match "Microsoft.WindowsDesktop.App 8\.") {
                return $true
            }
        }
    } catch {}
    return $false
}

if (-not (Test-DotNet8Runtime)) {
    Write-Host "[1/3] .NET 8 Runtime not found. Installing..." -ForegroundColor Yellow
    Write-Host "Downloading .NET 8 Desktop Runtime (~80MB)..." -ForegroundColor White
    
    $installerPath = "$env:TEMP\dotnet-runtime-desktop-8-installer.exe"
    
    try {
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($DotNetUrl, $installerPath)
        Write-Host "Installing .NET 8 Runtime (silent)..." -ForegroundColor Yellow
        $proc = Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -NoNewWindow
        
        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
            Write-Host "✅ .NET 8 Runtime installed!" -ForegroundColor Green
        } else {
            Write-Host "⚠️ .NET install returned code $($proc.ExitCode). Proceeding anyway..." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Could not install .NET Runtime: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   Install manually from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime-desktop-8.0.14-windows-x64-installer" -ForegroundColor White
    }
} else {
    Write-Host "✅ .NET 8 Runtime detected" -ForegroundColor Green
}

# ================================================================
# STEP 2: Check Clicky.exe
# ================================================================
if (-not (Test-Path $ExePath)) {
    Write-Host "[2/3] Clicky.exe not found. Downloading..." -ForegroundColor Yellow
    try {
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($ExeUrl, $ExePath)
        Write-Host "✅ Downloaded Clicky.exe" -ForegroundColor Green
    } catch {
        Write-Host "❌ Download failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Download manually from: https://github.com/amanshuuu/Clickyy/releases" -ForegroundColor Cyan
        pause
        exit
    }
} else {
    Write-Host "✅ Clicky.exe found" -ForegroundColor Green
}

# ================================================================
# STEP 3: Launch
# ================================================================
Write-Host "[3/3] Launching Clicky..." -ForegroundColor Green
Write-Host ""
Write-Host "🚀 Look for the blue triangle icon in your system tray" -ForegroundColor Cyan
Write-Host "🎤 Press Ctrl+Alt to start talking to your AI buddy!" -ForegroundColor Cyan
Write-Host ""

Start-Process -FilePath $ExePath
