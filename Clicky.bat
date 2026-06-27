@echo off
title Clicky - AI Buddy for Windows
cd /d "%~dp0"

echo.
echo ========================================
echo   Clicky - AI Buddy for Windows
echo ========================================
echo.

:: Try launching Clicky directly
if exist "Clicky.exe" (
    echo Launching Clicky...
    start "" "Clicky.exe"
    echo.
    echo Look for the blue triangle icon in your system tray.
    echo Press Ctrl+Alt to start talking!
    echo.
    exit /b 0
)

:: Clicky.exe not found
echo Clicky.exe not found in this folder.
echo.
echo Download the complete ZIP from:
echo https://github.com/amanshuuu/Clickyy/releases
echo.
pause
