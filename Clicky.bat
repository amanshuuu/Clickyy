@echo off
title Clicky - AI Buddy for Windows
setlocal enabledelayedexpansion

:: ===================================================================
:: Clicky - AI Buddy for Windows
:: Quick launcher. Works in two modes:
::   1. ZIP distribution: Clicky.exe is right next to this script
::   2. Standalone: Downloads Clicky.exe from GitHub on first run
:: ===================================================================

set "SCRIPT_DIR=%~dp0"
set "EXE_FILE=%SCRIPT_DIR%Clicky.exe"
set "EXE_URL=https://github.com/amanshuuu/Clickyy/releases/download/v1.0.0/Clicky.exe"

:: ===================================================================
:: MODE 1 — Clicky.exe already in same folder (ZIP distribution)
:: ===================================================================
if exist "%EXE_FILE%" (
    start "" "%EXE_FILE%"
    exit /b 0
)

:: ===================================================================
:: MODE 2 — Download Clicky.exe first (standalone script)
:: ===================================================================
echo.
echo ╔══════════════════════════════════════════════════╗
echo ║         Clicky - AI Buddy for Windows           ║
echo ║                                                 ║
echo ║  Downloading Clicky.exe (69MB)...               ║
echo ║  This is a one-time download.                   ║
echo ╚══════════════════════════════════════════════════╝
echo.

powershell -Command "
    $url = '%EXE_URL%';
    $out = '%EXE_FILE%';
    Write-Host 'Downloading Clicky...' -ForegroundColor Cyan;
    $wc = New-Object System.Net.WebClient;
    $wc.DownloadProgressChanged = {
        param($sender, $e)
        $pct = $e.ProgressPercentage;
        Write-Progress -Activity 'Downloading Clicky (69MB)' -Status '$pct%' -PercentComplete $pct;
    };
    $wc.DownloadFileAsync($url, $out);
    while ($wc.IsBusy) { Start-Sleep -Milliseconds 100 };
    Write-Host '';
    Write-Host 'Download complete!' -ForegroundColor Green;
"

if exist "%EXE_FILE%" (
    echo.
    echo Launching Clicky...
    start "" "%EXE_FILE%"
) else (
    echo.
    echo ERROR: Could not download Clicky.exe
    echo Please download manually from:
    echo https://github.com/amanshuuu/Clickyy/releases
    pause
)
