@echo off
setlocal EnableExtensions

REM =============================================================================
REM SmartCart - Full Database Build
REM Executes Database.sql using sqlcmd
REM =============================================================================

REM ---------------------------------------------------------------------------
REM SQL Server Configuration
REM ---------------------------------------------------------------------------

set "SQL_SERVER=(localdb)\MSSQLLocalDB"
set "SQL_DATABASE=SmartCart"

REM Authentication
set "SQL_AUTH_MODE=WINDOWS"
set "SQL_USER="
set "SQL_PASSWORD="

REM ---------------------------------------------------------------------------
REM Move to the folder containing this BAT file
REM ---------------------------------------------------------------------------

cd /d "%~dp0"

echo.
echo ============================================
echo       SmartCart Database Build
echo ============================================
echo Server   : %SQL_SERVER%
echo Database : %SQL_DATABASE%
echo Auth     : %SQL_AUTH_MODE%
echo ============================================
echo.

REM ---------------------------------------------------------------------------
REM Check sqlcmd
REM ---------------------------------------------------------------------------

where sqlcmd >nul 2>&1

if errorlevel 1 (
echo ERROR: sqlcmd was not found on PATH.
echo Install SQL Server Command Line Utilities and try again.
pause
exit /b 1
)

REM ---------------------------------------------------------------------------
REM Check Database.sql
REM ---------------------------------------------------------------------------

if not exist "Database.sql" (
echo ERROR: Database.sql was not found in "%CD%".
pause
exit /b 1
)

REM ---------------------------------------------------------------------------
REM Execute Database.sql
REM IMPORTANT:
REM DatabaseName is passed as a SQLCMD scripting variable.
REM This is required because Database.sql uses $(DatabaseName).
REM ---------------------------------------------------------------------------

echo Running Database.sql...
echo.

if /I "%SQL_AUTH_MODE%"=="SQL" (
    if "%SQL_USER%"=="" (
        echo ERROR: SQL_USER is required when SQL_AUTH_MODE=SQL.
        pause
        exit /b 1
    )

    sqlcmd ^
        -S "%SQL_SERVER%" ^
        -U "%SQL_USER%" ^
        -P "%SQL_PASSWORD%" ^
        -d master ^
        -I ^
        -b ^
        -m-1 ^
        -f 65001 ^
        -i "Database.sql" ^
        -v DatabaseName="%SQL_DATABASE%"
) else (
    sqlcmd ^
        -S "%SQL_SERVER%" ^
        -E ^
        -d master ^
        -I ^
        -b ^
        -m-1 ^
        -f 65001 ^
        -i "Database.sql" ^
        -v DatabaseName="%SQL_DATABASE%"
)

if errorlevel 1 (
echo.
echo ============================================
echo ERROR: Database build failed.
echo ============================================
echo Review the SQLCMD output above.
echo ============================================
pause
exit /b 1
)

echo.
echo ============================================
echo SUCCESS: SmartCart database built successfully.
echo ============================================

pause

endlocal
exit /b 0
