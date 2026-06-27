<#
.SYNOPSIS
    Clicky - AI Buddy for Windows
    Auto-downloads Clicky.exe on first run, then launches it.
    Just double-click this file - everything else is automatic.
#>

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Clicky - AI Buddy for Windows"

$ExeUrl = "https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ExePath = Join-Path $ScriptDir "Clicky.exe"

function Write-Header {
    Clear-Host
    Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║         Clicky - AI Buddy for Windows          ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

# If Clicky.exe already exists, launch it
if (Test-Path $ExePath) {
    Write-Header
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    Start-Process -FilePath $ExePath
    exit
}

# First run - download Clicky.exe
Write-Header
Write-Host "📦 This is your first time running Clicky!" -ForegroundColor Yellow
Write-Host ""
Write-Host "I'll download Clicky.exe (69MB) automatically." -ForegroundColor White
Write-Host "This is a one-time download." -ForegroundColor White
Write-Host ""

try {
    Write-Host "⬇️  Downloading Clicky.exe..." -ForegroundColor Cyan
    
    # Download with progress bar
    $wc = New-Object System.Net.WebClient
    $wc.DownloadProgressChanged += {
        param($sender, $e)
        $pct = $e.ProgressPercentage
        $bar = "█" * [math]::Floor($pct / 5) + "░" * (20 - [math]::Floor($pct / 5))
        Write-Progress -Activity "Downloading Clicky (69MB)" -Status "$pct%" -PercentComplete $pct
    }
    
    $wc.DownloadFileAsync($ExeUrl, $ExePath)
    
    # Wait for download to finish
    while ($wc.IsBusy) {
        Start-Sleep -Milliseconds 200
    }
    
    Write-Progress -Activity "Downloading Clicky (69MB)" -Completed
    
    if (-not (Test-Path $ExePath)) {
        throw "Download failed - file not found"
    }
    
    Write-Host ""
    Write-Host "✅ Download complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    
    # Get API key if needed
    Start-Process -FilePath $ExePath
}
catch {
    Write-Host ""
    Write-Host "❌ Download failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please download manually from:" -ForegroundColor Yellow
    Write-Host "https://github.com/amanshuuu/Clickyy/releases" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Save Clicky.exe in the same folder as this script." -ForegroundColor White
    Write-Host ""
    pause
}
