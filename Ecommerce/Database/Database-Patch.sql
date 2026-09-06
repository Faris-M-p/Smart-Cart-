/* =============================================================================
   SmartCart - Incremental database patch script
   -----------------------------------------------------------------------------
   Applies changes to an EXISTING database. Does not recreate the full schema.

   Run with sqlcmd (see RunPatch.bat). SQLCMD mode is required.

   Example:
     sqlcmd -S localhost -E -d master -I -b -f 65001
            -i Database-Patch.sql -v DatabaseName="SmartCart"

   Flow:
     Database-Patch.sql
         → required Patch.sql files
         → only uncommented :r scripts execute

   Before running a patch:
     1. In 01_Tables/Patch.sql, comment every table :r except tables being added
        or replaced.
     2. In each procedure module Patch.sql, comment every procedure :r except
        the procedures being created or replaced.
     3. You may also comment entire module :r lines below to skip that module.
     4. After the patch succeeds, restore :r lines for the next full create,
        or leave only the next change uncommented.
   ============================================================================= */

SET NOCOUNT ON;
GO

PRINT '=== SmartCart database patch starting ===';
PRINT 'Target database: $(DatabaseName)';
GO

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    RAISERROR('Database [%s] does not exist. Create it with RunDatabase.bat first.', 16, 1, N'$(DatabaseName)');
    SET NOEXEC ON;
END
GO

USE [$(DatabaseName)];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

PRINT '--- Applying incremental patches ---';
GO

/* Tables: comment unused :r lines inside 01_Tables\Patch.sql */
:r .\01_Tables\Patch.sql
GO

/* Seed: comment unused :r lines inside 05_Seed\Patch.sql */
:r .\05_Seed\Patch.sql
GO

/* Procedures: comment unused :r lines inside each module Patch.sql.
   To skip a whole module, comment that module's :r line below. */

:r .\02_Procedures\01_Admin\01_Products\Patch.sql
GO
:r .\02_Procedures\01_Admin\02_Brands\Patch.sql
GO
:r .\02_Procedures\01_Admin\03_Categories\Patch.sql
GO
:r .\02_Procedures\01_Admin\04_Variants\Patch.sql
GO
:r .\02_Procedures\01_Admin\05_Suppliers\Patch.sql
GO
:r .\02_Procedures\01_Admin\06_Purchases\Patch.sql
GO
:r .\02_Procedures\01_Admin\07_Stock\Patch.sql
GO
:r .\02_Procedures\01_Admin\08_UserRoles\Patch.sql
GO
:r .\02_Procedures\01_Admin\09_Permissions\Patch.sql
GO
:r .\02_Procedures\01_Admin\10_Employees\Patch.sql
GO
:r .\02_Procedures\01_Admin\11_Auth\Patch.sql
GO
:r .\02_Procedures\02_Users\01_Shop\Patch.sql
GO
:r .\02_Procedures\03_Common\01_Stock\Patch.sql
GO

PRINT '=== SmartCart database patch completed successfully ===';
GO
