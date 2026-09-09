@echo off
setlocal
cd /d "%~dp0"

set PSQL="C:\Program Files\PostgreSQL\18\bin\psql.exe"
set DB_NAME=smartcart
set DB_USER=postgres
set DB_HOST=localhost
set DB_PORT=5432
set PGPASSWORD=8816

echo Seeding supermarket catalog into %DB_NAME%...
%PSQL% -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -v ON_ERROR_STOP=1 -f 05_Seed/super_market_catalog.sql
if %ERRORLEVEL% NEQ 0 exit /b 1
echo SUCCESS: Supermarket catalog seeded.
