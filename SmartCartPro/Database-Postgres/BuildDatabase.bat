@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

REM =============================================================================
REM SmartCartPro | BuildDatabase.bat
REM Applies 01_Tables (create.sql) then 02_Seed scripts to an EXISTING database.
REM Does NOT drop/recreate the database by default.
REM Does NOT run on application startup — invoke manually from a developer machine.
REM
REM Required environment variables (or set below before running):
REM   SC_PGHOST, SC_PGPORT, SC_PGUSER, SC_PGDATABASE, PGPASSWORD
REM Optional:
REM   SC_PSQL   Full path to psql.exe if not on PATH
REM =============================================================================

echo ============================================
echo   SmartCartPro PostgreSQL Database Build
echo ============================================

if not defined SC_PGHOST set "SC_PGHOST=localhost"
if not defined SC_PGPORT set "SC_PGPORT=5432"
if not defined SC_PGUSER set "SC_PGUSER=postgres"
if not defined SC_PGDATABASE (
    echo ERROR: SC_PGDATABASE is not set. Example: set SC_PGDATABASE=smartcartpro
    exit /b 1
)
if not defined PGPASSWORD (
    echo ERROR: PGPASSWORD is not set. Do not hard-code passwords in this file.
    exit /b 1
)

set "PSQL_CMD=psql"
if defined SC_PSQL set "PSQL_CMD=%SC_PSQL%"

where "%PSQL_CMD%" >nul 2>&1
if errorlevel 1 (
    if exist "%PSQL_CMD%" goto :psql_ok
    echo ERROR: psql not found. Install PostgreSQL client tools or set SC_PSQL to psql.exe.
    exit /b 1
)
:psql_ok

echo Host     : %SC_PGHOST%
echo Port     : %SC_PGPORT%
echo User     : %SC_PGUSER%
echo Database : %SC_PGDATABASE%
echo.
echo NOTE: Target database must already exist.
echo       Create it manually if needed, e.g.:
echo         createdb -h %%SC_PGHOST%% -p %%SC_PGPORT%% -U %%SC_PGUSER%% %%SC_PGDATABASE%%
echo       (requires CREATEDB privilege; not performed by this script.)
echo.

"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -c "SELECT 1;" >nul
if errorlevel 1 (
    echo ERROR: Cannot connect to database "%SC_PGDATABASE%".
    exit /b 1
)

echo --- Creating / ensuring tables ---
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -f "01_Tables\create.sql"
if errorlevel 1 (
    echo ERROR: Table creation failed.
    exit /b 1
)

echo --- Seeding reference data ---
for %%F in (
    "02_Seed\001_DefaultRoles.sql"
    "02_Seed\002_DefaultPermissions.sql"
    "02_Seed\003_DefaultSettings.sql"
) do (
    echo Applying %%~F
    "%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -f "%%~F"
    if errorlevel 1 (
        echo ERROR: Seed script failed: %%~F
        exit /b 1
    )
)

echo --- Recording schema version ---
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -f "DatabaseVersion.sql"
if errorlevel 1 (
    echo ERROR: DatabaseVersion.sql failed.
    exit /b 1
)

REM Mark V001 applied if patch_history exists and row missing (build path, not re-apply DDL)
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -c "INSERT INTO patch_history (version, description, appliedby) SELECT 'V001', 'Initial schema via BuildDatabase.bat', current_user WHERE NOT EXISTS (SELECT 1 FROM patch_history WHERE version = 'V001');"
if errorlevel 1 (
    echo ERROR: Failed to record V001 in patch_history.
    exit /b 1
)

echo.
echo SUCCESS: Database build completed for %SC_PGDATABASE%.
exit /b 0
