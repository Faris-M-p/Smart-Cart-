@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

REM =============================================================================
REM SmartCartPro | ApplyPatches.bat
REM Applies pending files in 03_Patches\ named V###__Description.sql
REM Records success only in patch_history. Never marks a failed patch successful.
REM Uses pg_advisory_lock to reduce concurrent double-application risk.
REM =============================================================================

echo ============================================
echo   SmartCartPro PostgreSQL Apply Patches
echo ============================================

if not defined SC_PGHOST set "SC_PGHOST=localhost"
if not defined SC_PGPORT set "SC_PGPORT=5432"
if not defined SC_PGUSER set "SC_PGUSER=postgres"
if not defined SC_PGDATABASE set "SC_PGDATABASE=smartcart_pro"
if not defined PGPASSWORD set "PGPASSWORD=8816"

set "PSQL_CMD=psql"
if defined SC_PSQL set "PSQL_CMD=%SC_PSQL%"

where "%PSQL_CMD%" >nul 2>&1
if errorlevel 1 (
    if exist "C:\Program Files\PostgreSQL\18\bin\psql.exe" (
        set "PSQL_CMD=C:\Program Files\PostgreSQL\18\bin\psql.exe"
        goto :psql_ok
    )
    if exist "C:\Program Files\PostgreSQL\17\bin\psql.exe" (
        set "PSQL_CMD=C:\Program Files\PostgreSQL\17\bin\psql.exe"
        goto :psql_ok
    )
    if exist "C:\Program Files\PostgreSQL\16\bin\psql.exe" (
        set "PSQL_CMD=C:\Program Files\PostgreSQL\16\bin\psql.exe"
        goto :psql_ok
    )
    if exist "%PSQL_CMD%" goto :psql_ok
    echo ERROR: psql not found. Set SC_PSQL or add psql to PATH.
    pause
    exit /b 1
)
:psql_ok

echo Host     : %SC_PGHOST%
echo Port     : %SC_PGPORT%
echo User     : %SC_PGUSER%
echo Database : %SC_PGDATABASE%
echo PSQL     : %PSQL_CMD%
echo.

"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -c "SELECT 1;" >nul
if errorlevel 1 (
    echo ERROR: Cannot connect to "%SC_PGDATABASE%".
    pause
    exit /b 1
)

REM Ensure patch_history exists (bootstrap without re-running full schema)
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -f "01_Tables\042_PatchHistory.sql"
if errorlevel 1 (
    echo ERROR: Could not ensure patch_history table.
    pause
    exit /b 1
)

REM Advisory lock key: stable hash for SmartCartPro patches
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -c "SELECT pg_advisory_lock(87245001);"
if errorlevel 1 (
    echo ERROR: Could not acquire patch advisory lock.
    pause
    exit /b 1
)

set "FAILED=0"
for /f "delims=" %%F in ('dir /b /on "03_Patches\V*.sql" 2^>nul') do (
    set "PATCH_FILE=%%F"
    for /f "tokens=1 delims=_" %%V in ("%%F") do set "VERSION=%%V"

    "%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -tAc "SELECT 1 FROM patch_history WHERE version = '!VERSION!' AND success = TRUE;" > "%TEMP%\sc_patch_check.txt"
    set /p ALREADY=<"%TEMP%\sc_patch_check.txt"
    if "!ALREADY!"=="1" (
        echo SKIP already applied: !VERSION! ^(%%F^)
    ) else (
        echo APPLY !VERSION! from %%F ...
        "%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -f "03_Patches\%%F"
        if errorlevel 1 (
            echo ERROR: Patch failed: %%F
            echo Recovery: fix the SQL, then re-run ApplyPatches.bat.
            echo Do NOT insert into patch_history manually for a failed patch.
            set "FAILED=1"
            goto :release
        )
        "%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -v ON_ERROR_STOP=1 -c "INSERT INTO patch_history (version, description, appliedby, success) VALUES ('!VERSION!', '%%F', current_user, TRUE);"
        if errorlevel 1 (
            echo ERROR: Patch SQL succeeded but recording patch_history failed for !VERSION!.
            echo Investigate before re-running. Do not invent a success row.
            set "FAILED=1"
            goto :release
        )
        echo OK !VERSION!
    )
)

:release
"%PSQL_CMD%" -h "%SC_PGHOST%" -p "%SC_PGPORT%" -U "%SC_PGUSER%" -d "%SC_PGDATABASE%" -c "SELECT pg_advisory_unlock(87245001);" >nul

if "!FAILED!"=="1" (
    echo.
    echo FAILED: One or more patches did not complete successfully.
    pause
    exit /b 1
)

echo.
echo SUCCESS: All pending patches applied ^(or none pending^).
pause
exit /b 0

