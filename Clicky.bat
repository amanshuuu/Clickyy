@echo off
title Clicky - AI Buddy for Windows
setlocal enabledelayedexpansion

:: ===================================================================
:: Clicky - AI Buddy for Windows
:: Auto-installs .NET 8 Runtime if missing, then launches Clicky.exe
:: ===================================================================

set "SCRIPT_DIR=%~dp0"
set "EXE_FILE=%SCRIPT_DIR%Clicky.exe"
set "EXE_URL=https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"
set "DOTNET_URL=https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

:: ===========================================================
:: Check .NET 8 Desktop Runtime
:: ===========================================================
echo.
echo ╔══════════════════════════════════════════════════╗
echo ║         Clicky - AI Buddy for Windows           ║
echo ╚══════════════════════════════════════════════════╝
echo.

:check_dotnet
where dotnet >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    dotnet --list-runtimes 2>nul | find "Microsoft.WindowsDesktop.App 8." >nul
    if !ERRORLEVEL! EQU 0 goto :check_exe
)

echo [1/3] .NET 8 Runtime not found. Installing...
echo Downloading .NET 8 Desktop Runtime (silent install)...
echo.
powershell -Command "
    \$url = '%DOTNET_URL%';
    \$out = '%TEMP%\dotnet-runtime-desktop-8-installer.exe';
    Write-Host 'Downloading .NET 8 Desktop Runtime (~80MB)...' -ForegroundColor Cyan;
    \$wc = New-Object System.Net.WebClient;
    \$wc.DownloadFile(\$url, \$out);
    Write-Host 'Installing .NET 8 Runtime...' -ForegroundColor Yellow;
    \$proc = Start-Process -FilePath \$out -ArgumentList '/install', '/quiet', '/norestart' -Wait -PassThru -NoNewWindow;
    Write-Host '.NET 8 Runtime installed!' -ForegroundColor Green;
"

:: ===========================================================
:: Check if Clicky.exe exists, download if missing
:: ===========================================================
:check_exe
if exist "%EXE_FILE%" goto :launch

echo [2/3] Clicky.exe not found. Downloading...
powershell -Command "
    \$url = '%EXE_URL%';
    \$out = '%EXE_FILE%';
    Write-Host 'Downloading Clicky.exe (72MB)...' -ForegroundColor Cyan;
    \$wc = New-Object System.Net.WebClient;
    \$wc.DownloadFile(\$url, \$out);
    Write-Host 'Download complete!' -ForegroundColor Green;
"

:: ===========================================================
:: Launch Clicky
:: ===========================================================
:launch
echo [3/3] Launching Clicky...
echo.
echo Look for the blue triangle icon in your system tray.
echo Press Ctrl+Alt to start talking to your AI buddy!
echo.
if exist "%EXE_FILE%" (
    start "" "%EXE_FILE%"
) else (
    echo ERROR: Clicky.exe not found. Download manually from:
    echo https://github.com/amanshuuu/Clickyy/releases
    pause
)
