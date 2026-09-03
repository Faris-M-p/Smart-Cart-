@echo off
setlocal EnableExtensions

REM =============================================================================
REM  SmartCart - Incremental database patch
REM  Executes Database-Patch.sql only. Does not recreate the full database.
REM =============================================================================

REM --- Connection (edit these values; do not commit real passwords) ---
set "SQL_SERVER=localhost"
set "SQL_DATABASE=SmartCart"

REM Authentication: WINDOWS  (trusted / Integrated Security)
REM                 SQL      (SQL Server login; set SQL_USER / SQL_PASSWORD)
set "SQL_AUTH_MODE=WINDOWS"
set "SQL_USER="
set "SQL_PASSWORD="

REM ---------------------------------------------------------------------------
cd /d "%~dp0"

echo.
echo ============================================
echo  SmartCart database patch
echo ============================================
echo  Server   : %SQL_SERVER%
echo  Database : %SQL_DATABASE%
echo  Auth     : %SQL_AUTH_MODE%
echo ============================================
echo.

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo ERROR: sqlcmd was not found on PATH.
    echo Install SQL Server Command Line Utilities and try again.
    exit /b 1
)

if not exist "Database-Patch.sql" (
    echo ERROR: Database-Patch.sql was not found in "%CD%".
    exit /b 1
)

if /I "%SQL_AUTH_MODE%"=="SQL" (
    if "%SQL_USER%"=="" (
        echo ERROR: SQL_USER is required when SQL_AUTH_MODE=SQL.
        echo Set SQL_USER and SQL_PASSWORD at the top of this file.
        exit /b 1
    )
    sqlcmd -S "%SQL_SERVER%" -U "%SQL_USER%" -P "%SQL_PASSWORD%" -d master -I -b -f 65001 -i "Database-Patch.sql" -v DatabaseName="%SQL_DATABASE%"
) else (
    sqlcmd -S "%SQL_SERVER%" -E -d master -I -b -f 65001 -i "Database-Patch.sql" -v DatabaseName="%SQL_DATABASE%"
)

if errorlevel 1 (
    echo.
    echo ERROR: Database patch failed. Review the sqlcmd output above.
    exit /b 1
)

echo.
echo SUCCESS: Patch applied to [%SQL_DATABASE%] on [%SQL_SERVER%].
exit /b 0
