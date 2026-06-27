@echo off
title Clicky - AI Buddy for Windows
cd /d "%~dp0"

:: Launch the PowerShell setup script
powershell -ExecutionPolicy Bypass -File "%~dp0Clicky.ps1"

:: If PowerShell script exited, pause so user can see messages
echo.
pause
