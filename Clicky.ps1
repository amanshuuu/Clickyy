<#
.SYNOPSIS
    Clicky - AI Buddy for Windows
    Quick launcher. Auto-detects Clicky.exe in the same folder (ZIP distro)
    or downloads it from GitHub on first run (standalone script).
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

# If Clicky.exe already exists (ZIP distro or previously downloaded), launch it
if (Test-Path $ExePath) {
    Write-Header
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    Start-Process -FilePath $ExePath
    exit
}

# First run — download Clicky.exe
Write-Header
Write-Host "📦 First time! Downloading Clicky.exe (69MB)..." -ForegroundColor Yellow
Write-Host ""

try {
    $wc = New-Object System.Net.WebClient
    $wc.DownloadProgressChanged += {
        param($sender, $e)
        $pct = $e.ProgressPercentage
        Write-Progress -Activity "Downloading Clicky (69MB)" -Status "$pct%" -PercentComplete $pct
    }
    $wc.DownloadFileAsync($ExeUrl, $ExePath)
    while ($wc.IsBusy) { Start-Sleep -Milliseconds 200 }
    Write-Progress -Activity "Downloading Clicky (69MB)" -Completed

    if (-not (Test-Path $ExePath)) { throw "Download failed" }

    Write-Host "✅ Download complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    Start-Process -FilePath $ExePath
}
catch {
    Write-Host "❌ Download failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please download the ZIP from:" -ForegroundColor Yellow
    Write-Host "https://github.com/amanshuuu/Clickyy/releases" -ForegroundColor Cyan
    pause
}
