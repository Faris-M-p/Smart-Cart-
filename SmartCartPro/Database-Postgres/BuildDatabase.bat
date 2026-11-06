@echo off
setlocal EnableExtensions DisableDelayedExpansion

REM Double-click entry point. Paths are always resolved beside this file.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0DatabaseRunner.ps1" -Action BuildDatabase
set "RUNNER_EXIT_CODE=%ERRORLEVEL%"

if not "%RUNNER_EXIT_CODE%"=="0" (
    echo.
    echo ERROR: Database setup stopped with exit code %RUNNER_EXIT_CODE%.
)

echo.
if /I not "%SC_NO_PAUSE%"=="1" pause
exit /b %RUNNER_EXIT_CODE%
