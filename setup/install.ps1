<#
.SYNOPSIS
    One-click installer for Clicky - AI Buddy for Windows
.DESCRIPTION
    Installs Clicky, creates shortcuts, adds to startup, and configures everything.
    Run this script to get everything set up automatically.
.NOTES
    Right-click → "Run with PowerShell" or open PowerShell as Admin and run:
    PS> Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; .\install.ps1
#>

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Installing Clicky - AI Buddy for Windows"

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Installing Clicky for Windows          ║" -ForegroundColor Cyan
Write-Host "║          AI Buddy That Lives on Your Screen    ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InstallDir = "$env:LOCALAPPDATA\Clicky"
$ExeSource = "$ScriptDir\Clicky.exe"
$ExeTarget = "$InstallDir\Clicky.exe"
$StartMenuShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Clicky.lnk"
$DesktopShortcut = "$env:USERPROFILE\Desktop\Clicky.lnk"
$StartupShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Clicky.lnk"
$SettingsDir = "$env:APPDATA\Clicky"

# 1. Install Clicky
Write-Host "📁 Installing to: $InstallDir" -ForegroundColor Yellow
if (!(Test-Path $InstallDir)) { New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null }

Write-Host "📦 Copying Clicky.exe..." -ForegroundColor Yellow
Copy-Item -Path $ExeSource -Destination $ExeTarget -Force

# 2. Create shortcuts
$WScriptShell = New-Object -ComObject WScript.Shell

# Start Menu
$shortcut = $WScriptShell.CreateShortcut($StartMenuShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - Press Ctrl+Alt to talk to your AI buddy"
$shortcut.Save()
Write-Host "📌 Start Menu shortcut created" -ForegroundColor Green

# Desktop
$shortcut = $WScriptShell.CreateShortcut($DesktopShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - AI Buddy"
$shortcut.Save()
Write-Host "🖥️ Desktop shortcut created" -ForegroundColor Green

# 3. Add to startup
$shortcut = $WScriptShell.CreateShortcut($StartupShortcut)
$shortcut.TargetPath = $ExeTarget
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description = "Clicky - Auto-start on login"
$shortcut.Save()
Write-Host "🚀 Added to startup (launches automatically when you log in)" -ForegroundColor Green

# 4. Create settings directory
if (!(Test-Path $SettingsDir)) { New-Item -ItemType Directory -Path $SettingsDir -Force | Out-Null }
Write-Host "⚙️ Settings directory created at: $SettingsDir" -ForegroundColor Green

# 5. Create uninstaller script
$UninstallScript = @"
`$InstallDir = "$InstallDir"
`$StartMenuShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Clicky.lnk"
`$DesktopShortcut = "$env:USERPROFILE\Desktop\Clicky.lnk"
`$StartupShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Clicky.lnk"
`$SettingsDir = "$env:APPDATA\Clicky"

Write-Host "🗑️  Uninstalling Clicky..." -ForegroundColor Yellow

# Remove shortcuts
if (Test-Path `$StartMenuShortcut) { Remove-Item `$StartMenuShortcut -Force }
if (Test-Path `$DesktopShortcut) { Remove-Item `$DesktopShortcut -Force }
if (Test-Path `$StartupShortcut) { Remove-Item `$StartupShortcut -Force }

# Remove installation
if (Test-Path `$InstallDir) { Remove-Item `$InstallDir -Recurse -Force }

# Ask about settings
`$removeSettings = Read-Host "Remove settings and API keys? (y/N)"
if (`$removeSettings -eq "y" -and (Test-Path `$SettingsDir)) {
    Remove-Item `$SettingsDir -Recurse -Force
    Write-Host "✅ Settings removed" -ForegroundColor Green
}

Write-Host "✅ Clicky has been uninstalled" -ForegroundColor Green
"@

$UninstallScript | Out-File -FilePath "$InstallDir\uninstall.ps1" -Encoding UTF8
Write-Host "🗑️  Uninstaller created at: $InstallDir\uninstall.ps1" -ForegroundColor Green

# 6. Create a "Start Clicky" batch file
@"
@echo off
start "" "$ExeTarget"
"@ | Out-File -FilePath "$InstallDir\start_clicky.bat" -Encoding ASCII

# 7. Firewall rule (for network access to OpenRouter API)
try {
    New-NetFirewallRule -DisplayName "Clicky - AI Buddy" -Direction Outbound -Program "$ExeTarget" -Action Allow -ErrorAction SilentlyContinue | Out-Null
    Write-Host "🔒 Firewall rule added" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Could not add firewall rule (run as Admin for this)" -ForegroundColor Yellow
}

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
Write-Host "║  First time? Double-click Clicky.exe to start!  ║" -ForegroundColor Green
Write-Host "║                                                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 8. Launch Clicky
$launch = Read-Host "Launch Clicky now? (Y/n)"
if ($launch -ne "n") {
    Write-Host "🚀 Launching Clicky..." -ForegroundColor Green
    Start-Process -FilePath $ExeTarget
}

Read-Host "Press Enter to exit"
