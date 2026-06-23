@echo off
title Clicky - AI Buddy
echo Starting Clicky...

:: Look for Clicky.exe in same directory, then setup\, then PATH
if exist "%~dp0Clicky.exe" (
    start "" "%~dp0Clicky.exe"
) else if exist "%~dp0setup\Clicky.exe" (
    start "" "%~dp0setup\Clicky.exe"
) else (
    echo Clicky.exe not found! Make sure it's in the same folder as this script.
    echo Looked in: "%~dp0"
    pause
    exit /b 1
)

echo Clicky is running! Look for the blue triangle in your system tray.
echo Press Ctrl+Alt to talk to your AI buddy.
