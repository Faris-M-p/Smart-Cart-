@echo off
setlocal
cd /d "%~dp0"

set PSQL="C:\Program Files\PostgreSQL\18\bin\psql.exe"
set DB_NAME=smartcart
set DB_USER=postgres
set DB_HOST=localhost
set DB_PORT=5432
set PGPASSWORD=8816

echo ============================================
echo      SmartCart PostgreSQL Database Patch
echo ============================================
echo Host     : %DB_HOST%
echo Port     : %DB_PORT%
echo User     : %DB_USER%
echo Database : %DB_NAME%
echo ============================================
echo.

%PSQL% -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -v ON_ERROR_STOP=1 -v dbname=%DB_NAME% -f Database-Patch.sql

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Database patch failed.
    pause
    exit /b 1
)

echo.
echo SUCCESS: Patch applied.
pause
