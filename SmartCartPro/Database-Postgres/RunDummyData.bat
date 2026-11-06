@echo off
setlocal EnableExtensions DisableDelayedExpansion

REM This is the only entry point for the optional, manual dummy-data scripts.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0DatabaseRunner.ps1" -Action RunDummyData
set "RUNNER_EXIT_CODE=%ERRORLEVEL%"

if not "%RUNNER_EXIT_CODE%"=="0" (
    echo.
    echo ERROR: Dummy-data run stopped with exit code %RUNNER_EXIT_CODE%.
)

echo.
if /I not "%SC_NO_PAUSE%"=="1" pause
exit /b %RUNNER_EXIT_CODE%
