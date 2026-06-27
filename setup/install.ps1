<#
.SYNOPSIS
    One-click installer for Clicky - AI Buddy for Windows
.DESCRIPTION
    Auto-installs .NET 8 Runtime if missing, installs Clicky,
    creates shortcuts, adds to startup, and configures everything.
.NOTES
    Right-click → "Run with PowerShell"
#>

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Installing Clicky - AI Buddy for Windows"

$DotNetUrl = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Installing Clicky for Windows          ║" -ForegroundColor Cyan
Write-Host "║          AI Buddy That Lives on Your Screen    ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ================================================================
# STEP 1: Check .NET 8 Desktop Runtime
# ================================================================
Write-Host "[1/4] Checking .NET 8 Runtime..." -ForegroundColor Yellow

$dotnetInstalled = $false
try {
    $dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
    if ($dotnet) {
        $runtimes = & dotnet --list-runtimes 2>$null
        if ($runtimes -match "Microsoft.WindowsDesktop.App 8\.") {
            $dotnetInstalled = $true
        }
    }
} catch {}

if (-not $dotnetInstalled) {
    Write-Host "      .NET 8 Runtime not found. Downloading installer..." -ForegroundColor White
    $installerPath = "$env:TEMP\dotnet-runtime-desktop-8-installer.exe"
    try {
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($DotNetUrl, $installerPath)
        Write-Host "      Installing .NET 8 Runtime silently..." -ForegroundColor White
        $proc = Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -NoNewWindow
        Write-Host "✅ .NET 8 Runtime installed!" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Could not install .NET Runtime automatically." -ForegroundColor Yellow
        Write-Host "   Install manually: https://dotnet.microsoft.com/en-us/download/dotnet/8.0" -ForegroundColor White
    }
} else {
    Write-Host "✅ .NET 8 Runtime detected" -ForegroundColor Green
}

# ================================================================
# STEP 2: Install Clicky
# ================================================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InstallDir = "$env:LOCALAPPDATA\Clicky"
$ExeSource = "$ScriptDir\Clicky.exe"
$ExeTarget = "$InstallDir\Clicky.exe"
$StartMenuShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Clicky.lnk"
$DesktopShortcut = "$env:USERPROFILE\Desktop\Clicky.lnk"
$StartupShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Clicky.lnk"
$SettingsDir = "$env:APPDATA\Clicky"

Write-Host "[2/4] Installing Clicky to: $InstallDir" -ForegroundColor Yellow
if (!(Test-Path $InstallDir)) { New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null }
Copy-Item -Path $ExeSource -Destination $ExeTarget -Force

# ================================================================
# STEP 3: Create shortcuts
# ================================================================
Write-Host "[3/4] Creating shortcuts..." -ForegroundColor Yellow
$WScriptShell = New-Object -ComObject WScript.Shell

$shortcut = $WScriptShell.CreateShortcut($StartMenuShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - Press Ctrl+Alt to talk to your AI buddy"
$shortcut.Save()
Write-Host "      Start Menu shortcut created" -ForegroundColor Green

$shortcut = $WScriptShell.CreateShortcut($DesktopShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - AI Buddy"
$shortcut.Save()
Write-Host "      Desktop shortcut created" -ForegroundColor Green

$shortcut = $WScriptShell.CreateShortcut($StartupShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - Auto-start on login"
$shortcut.Save()
Write-Host "      Added to startup" -ForegroundColor Green

# Settings directory
if (!(Test-Path $SettingsDir)) { New-Item -ItemType Directory -Path $SettingsDir -Force | Out-Null }

# ================================================================
# STEP 4: Create uninstaller
# ================================================================
Write-Host "[4/4] Creating uninstaller..." -ForegroundColor Yellow
$UninstallScript = @"
`$InstallDir = "$InstallDir"
`$StartMenuShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Clicky.lnk"
`$DesktopShortcut = "$env:USERPROFILE\Desktop\Clicky.lnk"
`$StartupShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Clicky.lnk"
`$SettingsDir = "$env:APPDATA\Clicky"

Write-Host "Uninstalling Clicky..." -ForegroundColor Yellow
foreach (`$p in @(`$StartMenuShortcut, `$DesktopShortcut, `$StartupShortcut)) {
    if (Test-Path `$p) { Remove-Item `$p -Force }
}
if (Test-Path `$InstallDir) { Remove-Item `$InstallDir -Recurse -Force }
`$removeSettings = Read-Host "Remove settings and API keys? (y/N)"
if (`$removeSettings -eq "y" -and (Test-Path `$SettingsDir)) {
    Remove-Item `$SettingsDir -Recurse -Force
}
Write-Host "Clicky uninstalled" -ForegroundColor Green
"@
$UninstallScript | Out-File -FilePath "$InstallDir\uninstall.ps1" -Encoding UTF8

# Firewall rule
try {
    New-NetFirewallRule -DisplayName "Clicky - AI Buddy" -Direction Outbound -Program "$ExeTarget" -Action Allow -ErrorAction SilentlyContinue | Out-Null
} catch {}

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║              ✅  INSTALLATION COMPLETE!         ║" -ForegroundColor Cyan
Write-Host "╠══════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║                                                  ║" -ForegroundColor Cyan
Write-Host "║  1. Launch Clicky from Start Menu or Desktop     ║" -ForegroundColor White
Write-Host "║  2. Blue triangle icon appears in system tray   ║" -ForegroundColor White
Write-Host "║  3. Press Ctrl+Alt to start talking             ║" -ForegroundColor White
Write-Host "║                                                  ║" -ForegroundColor Cyan
Write-Host "║  Next step: Get a free API key at:              ║" -ForegroundColor Cyan
Write-Host "║  https://openrouter.ai (free credits on signup) ║" -ForegroundColor Cyan
Write-Host "║                                                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$launch = Read-Host "Launch Clicky now? (Y/n)"
if ($launch -ne "n") {
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    Start-Process -FilePath $ExeTarget
}
