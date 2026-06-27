@echo off
title Clicky - AI Buddy
setlocal enabledelayedexpansion

:: Download URL for the latest release
set "EXE_URL=https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"
set "EXE_FILE=%~dp0Clicky.exe"

:: If Clicky.exe is already here, just launch it
if exist "%EXE_FILE%" (
    echo Launching Clicky...
    start "" "%EXE_FILE%"
    exit /b 0
)

:: Otherwise, download it
echo.
echo ╔══════════════════════════════════════════════════╗
echo ║         Clicky - AI Buddy for Windows           ║
echo ║                                                 ║
echo ║  Downloading Clicky.exe (69MB)...               ║
echo ║  This is a one-time download.                   ║
echo ╚══════════════════════════════════════════════════╝
echo.

:: Download using PowerShell (built-in on Windows 10/11)
powershell -Command "
    \$url = '%EXE_URL%';
    \$out = '%EXE_FILE%';
    Write-Host 'Downloading Clicky...' -ForegroundColor Cyan;
    \$wc = New-Object System.Net.WebClient;
    \$wc.DownloadProgressChanged = {
        param(\$sender, \$e)
        \$pct = \$e.ProgressPercentage;
        Write-Progress -Activity 'Downloading Clicky (69MB)' -Status '\$pct%' -PercentComplete \$pct;
    };
    \$wc.DownloadFileAsync(\$url, \$out);
    while (\$wc.IsBusy) { Start-Sleep -Milliseconds 100 };
    Write-Host '';
    Write-Host 'Download complete!' -ForegroundColor Green;
"

:: Check if download succeeded
if exist "%EXE_FILE%" (
    echo.
    echo Launching Clicky...
    start "" "%EXE_FILE%"
) else (
    echo.
    echo Download failed. Trying alternative method...
    powershell -Command "Invoke-WebRequest -Uri '%EXE_URL%' -OutFile '%EXE_FILE%'"
    
    if exist "%EXE_FILE%" (
        echo Launching Clicky...
        start "" "%EXE_FILE%"
    ) else (
        echo.
        echo ERROR: Could not download Clicky.exe
        echo Please download manually from:
        echo https://github.com/amanshuuu/Clickyy/releases
        pause
    )
)
