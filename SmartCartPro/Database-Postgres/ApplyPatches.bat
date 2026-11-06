@echo off
setlocal EnableExtensions DisableDelayedExpansion

REM Applies pending reviewed V002+ SQL only; dummy data is never discovered here.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0DatabaseRunner.ps1" -Action ApplyPatches
set "RUNNER_EXIT_CODE=%ERRORLEVEL%"

if not "%RUNNER_EXIT_CODE%"=="0" (
    echo.
    echo ERROR: Patch application stopped with exit code %RUNNER_EXIT_CODE%.
)

echo.
if /I not "%SC_NO_PAUSE%"=="1" pause
exit /b %RUNNER_EXIT_CODE%
