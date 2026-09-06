/* =============================================================================
   SmartCart - Master database build script
   -----------------------------------------------------------------------------
   Builds a complete database from organized object scripts.

   Run with sqlcmd (see RunDatabase.bat). SQLCMD mode is required.

   Example:
     sqlcmd -S localhost -E -d master -I -b -f 65001
            -i Database.sql -v DatabaseName="SmartCart"

   Flow:
     Database.sql
         → 01_Tables/Patch.sql          (all table :r lines uncommented)
         → 05_Seed/Patch.sql            (all seed :r lines uncommented)
         → each procedure module Patch.sql  (all procedure :r lines uncommented)

   For incremental updates to an existing database, use Database-Patch.sql
   and comment unused :r lines inside the relevant Patch.sql files.
   ============================================================================= */

SET NOCOUNT ON;
GO

PRINT '=== SmartCart database build starting ===';
PRINT 'Target database: $(DatabaseName)';
GO

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    PRINT 'Creating database [$(DatabaseName)]...';
    CREATE DATABASE [$(DatabaseName)];
    PRINT 'Database [$(DatabaseName)] created.';
END
ELSE
BEGIN
    PRINT 'Database [$(DatabaseName)] already exists. Object scripts will run in that database.';
END
GO

USE [$(DatabaseName)];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* =============================================================================
   1) TABLES
   ============================================================================= */
PRINT '--- Creating tables ---';
GO
:r .\01_Tables\Patch.sql
GO

/* =============================================================================
   1b) SEED DATA  (authentication / authorization)
   ============================================================================= */
PRINT '--- Seeding authentication data ---';
GO
:r .\05_Seed\Patch.sql
GO

/* =============================================================================
   2) FUNCTIONS
   No functions currently exist. Add files under 03_Functions when needed.
   ============================================================================= */
PRINT '--- Functions (none) ---';
GO

/* =============================================================================
   3) STORED PROCEDURES
   ============================================================================= */
PRINT '--- Creating stored procedures ---';
GO

PRINT 'Admin / Products';
GO
:r .\02_Procedures\01_Admin\01_Products\Patch.sql
GO

PRINT 'Admin / Brands';
GO
:r .\02_Procedures\01_Admin\02_Brands\Patch.sql
GO

PRINT 'Admin / Categories';
GO
:r .\02_Procedures\01_Admin\03_Categories\Patch.sql
GO

PRINT 'Admin / Variants';
GO
:r .\02_Procedures\01_Admin\04_Variants\Patch.sql
GO

PRINT 'Admin / Suppliers';
GO
:r .\02_Procedures\01_Admin\05_Suppliers\Patch.sql
GO

PRINT 'Admin / Purchases';
GO
:r .\02_Procedures\01_Admin\06_Purchases\Patch.sql
GO

PRINT 'Admin / Stock';
GO
:r .\02_Procedures\01_Admin\07_Stock\Patch.sql
GO

PRINT 'Admin / UserRoles';
GO
:r .\02_Procedures\01_Admin\08_UserRoles\Patch.sql
GO

PRINT 'Admin / Permissions';
GO
:r .\02_Procedures\01_Admin\09_Permissions\Patch.sql
GO

PRINT 'Admin / Employees';
GO
:r .\02_Procedures\01_Admin\10_Employees\Patch.sql
GO

PRINT 'Admin / Auth';
GO
:r .\02_Procedures\01_Admin\11_Auth\Patch.sql
GO

PRINT 'Users / Shop';
GO
:r .\02_Procedures\02_Users\01_Shop\Patch.sql
GO

PRINT 'Common / Stock';
GO
:r .\02_Procedures\03_Common\01_Stock\Patch.sql
GO

/* =============================================================================
   4) VIEWS
   No views currently exist. Add files under 04_Views when needed.
   ============================================================================= */
PRINT '--- Views (none) ---';
GO

PRINT '=== SmartCart database build completed successfully ===';
GO
