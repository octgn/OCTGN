@echo off
REM Update Heat File List - OCTGN Installer
REM This script updates the WiX Heat-generated file list while preserving GUIDs

setlocal

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Default to Debug configuration
set "CONFIG=Debug"
set "FORCE_PARAM="

REM Parse parameters
:parse_args
if "%1"=="" goto :done_parsing
if /i "%1"=="Debug" set "CONFIG=Debug" & shift & goto :parse_args
if /i "%1"=="Release" set "CONFIG=Release" & shift & goto :parse_args
if /i "%1"=="-Force" set "FORCE_PARAM=-Force" & shift & goto :parse_args
if /i "%1"=="Force" set "FORCE_PARAM=-Force" & shift & goto :parse_args
REM If not recognized, assume it's configuration
set "CONFIG=%1" & shift & goto :parse_args

:done_parsing

echo.
echo OCTGN Heat File List Updater
echo ============================
echo Configuration: %CONFIG%
if not "%FORCE_PARAM%"=="" echo Force mode: ON
echo.

REM Run the PowerShell script
powershell.exe -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Update-HeatFileList.ps1" -Configuration "%CONFIG%" %FORCE_PARAM%

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Failed to update heat file list
    pause
    exit /b 1
)

echo.
echo SUCCESS: Heat file list updated
echo You can now build the installer projects.
echo.
pause
