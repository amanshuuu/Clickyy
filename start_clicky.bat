@echo off
title Clicky - AI Buddy
cd /d "%~dp0"
if exist "Clicky.exe" (
    start "" "Clicky.exe"
) else if exist "setup\Clicky.exe" (
    start "" "setup\Clicky.exe"
) else (
    echo Clicky.exe not found. Download from:
    echo https://github.com/amanshuuu/Clickyy/releases
    pause
)
