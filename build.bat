@echo off
echo GlassPane - Windows 11 Virtual Desktop Manager
echo =============================================

echo Checking for .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 6.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET SDK found. Building application...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages!
    pause
    exit /b 1
)

dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo Build completed successfully!
echo.
echo To run the application:
echo 1. Right-click on the executable and "Run as Administrator"
echo 2. Or use: dotnet run --configuration Release
echo.
echo Press any key to run the application now...
pause >nul

dotnet run --configuration Release 