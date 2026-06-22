@echo off
REM ISRA Process Simulate Add-Ons Installer
REM Double-click to install

echo ========================================
echo ISRA Add-Ons Installer
echo ========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
	echo ERROR: Administrator privileges required!
	echo.
	echo Please right-click this file and select "Run as Administrator"
	echo.
	pause
	exit /b 1
)

REM Run PowerShell installer
PowerShell.exe -ExecutionPolicy Bypass -File "%~dp0Install-ISRA-Addons.ps1"

pause
