@echo off
title Building Clicky for Windows

echo ========================================
echo   Building Clicky for Windows
echo ========================================
echo.

:: Check if dotnet is available
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET 8 SDK not found!
    echo.
    echo Download and install from:
    echo https://aka.ms/dotnet/8.0/dotnet-sdk-win-x64.exe
    echo.
    echo After installing, close and reopen this terminal, then run build again.
    pause
    exit /b 1
)

:: Check SDK version
dotnet --version | findstr /B "8." >nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Need .NET 8 SDK. Current version:
    dotnet --version
    echo.
    echo Download from: https://aka.ms/dotnet/8.0/dotnet-sdk-win-x64.exe
    pause
    exit /b 1
)

echo [1/2] Restoring packages...
dotnet restore "%~dp0ClickyWindows.sln" --runtime win-x64
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Package restore failed
    pause
    exit /b 1
)

echo [2/2] Building Clicky (framework-dependent)...
dotnet build "%~dp0ClickyWindows\ClickyWindows.csproj" -c Release --runtime win-x64 --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)

:: Copy EXE to root
copy /Y "%~dp0ClickyWindows\bin\Release\net8.0-windows\win-x64\Clicky.exe" "%~dp0Clicky.exe" >nul

echo.
echo ========================================
echo   BUILD COMPLETE!
echo ========================================
echo.
echo Clicky.exe has been built at:
echo %~dp0Clicky.exe
echo.
echo You can now double-click Clicky.exe to run it.
echo.
pause
