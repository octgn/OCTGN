@echo off
REM OCTGN Electron Launcher for Windows
REM This batch file calls the PowerShell launcher

echo.
echo  Starting OCTGN Electron...
echo.

PowerShell -NoProfile -ExecutionPolicy Bypass -File "%~dp0launch.ps1" %*

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  If you see an execution policy error, run this command as Administrator:
    echo  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
    echo.
    pause
)
