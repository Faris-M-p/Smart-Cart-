@echo off
setlocal EnableExtensions DisableDelayedExpansion

echo ========================================
echo  SmartCartPro PostgreSQL Apply Patches
echo ========================================
echo.
echo 1. Local PostgreSQL
echo 2. Aiven Cloud PostgreSQL
echo.
set "TARGET_CHOICE=1"
set /p "TARGET_CHOICE=Select database target (1 or 2) [default: 1]: "

if "%TARGET_CHOICE%"=="2" (
    echo.
    echo Target: Aiven Cloud PostgreSQL
    echo Host:   pg-3b6aeb4c-smartcartpro.d.aivencloud.com:15307
    echo User:   avnadmin
    echo Target: smartcartpro
    echo SSL:    require
    echo.
    set "SC_TARGET_ENV=Aiven"
    set "SC_PGHOST=pg-3b6aeb4c-smartcartpro.d.aivencloud.com"
    set "SC_PGPORT=15307"
    set "SC_PGUSER=avnadmin"
    set "SC_PGMAINTENANCEDATABASE=defaultdb"
    set "SC_PGDATABASE=smartcartpro"
    set "SC_PGSSLMODE=require"
    set "PGSSLMODE=require"
) else (
    echo.
    echo Target: Local PostgreSQL
    set "SC_TARGET_ENV=Local"
)

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
