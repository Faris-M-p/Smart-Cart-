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
         → 01_Tables/Patch.sql   (incremental / IF NOT EXISTS only)
         → 05_Seed/Patch.sql     (idempotent seeds)
         → procedure Patch.sql   (CREATE OR ALTER)

   Do not include 01_Tables/Create.sql here. That file is for RunDatabase.bat only.
   ============================================================================= */

SET NOCOUNT ON;
GO

PRINT N'=== SmartCart database patch starting ===';
PRINT N'Target database: $(DatabaseName)';
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

PRINT N'--- Applying table patches ---';
GO
:r .\01_Tables\Patch.sql
GO
PRINT N'Tables patch module successfully completed.';
GO

PRINT N'--- Applying seed patches ---';
GO
:r .\05_Seed\Patch.sql
GO
PRINT N'Seed patch module successfully completed.';
GO

PRINT N'--- Applying stored procedure patches ---';
GO

PRINT N'--- Admin / Products ---';
GO
:r .\02_Procedures\01_Admin\01_Products\Patch.sql
GO
PRINT N'Admin / Products patch module successfully completed.';
GO

PRINT N'--- Admin / Brands ---';
GO
:r .\02_Procedures\01_Admin\02_Brands\Patch.sql
GO
PRINT N'Admin / Brands patch module successfully completed.';
GO

PRINT N'--- Admin / Categories ---';
GO
:r .\02_Procedures\01_Admin\03_Categories\Patch.sql
GO
PRINT N'Admin / Categories patch module successfully completed.';
GO

PRINT N'--- Admin / Variants ---';
GO
:r .\02_Procedures\01_Admin\04_Variants\Patch.sql
GO
PRINT N'Admin / Variants patch module successfully completed.';
GO

PRINT N'--- Admin / Suppliers ---';
GO
:r .\02_Procedures\01_Admin\05_Suppliers\Patch.sql
GO
PRINT N'Admin / Suppliers patch module successfully completed.';
GO

PRINT N'--- Admin / Purchases ---';
GO
:r .\02_Procedures\01_Admin\06_Purchases\Patch.sql
GO
PRINT N'Admin / Purchases patch module successfully completed.';
GO

PRINT N'--- Admin / Stock ---';
GO
:r .\02_Procedures\01_Admin\07_Stock\Patch.sql
GO
PRINT N'Admin / Stock patch module successfully completed.';
GO

PRINT N'--- Admin / UserRoles ---';
GO
:r .\02_Procedures\01_Admin\08_UserRoles\Patch.sql
GO
PRINT N'Admin / UserRoles patch module successfully completed.';
GO

PRINT N'--- Admin / Permissions ---';
GO
:r .\02_Procedures\01_Admin\09_Permissions\Patch.sql
GO
PRINT N'Admin / Permissions patch module successfully completed.';
GO

PRINT N'--- Admin / Employees ---';
GO
:r .\02_Procedures\01_Admin\10_Employees\Patch.sql
GO
PRINT N'Admin / Employees patch module successfully completed.';
GO

PRINT N'--- Admin / Auth ---';
GO
:r .\02_Procedures\01_Admin\11_Auth\Patch.sql
GO
PRINT N'Admin / Auth patch module successfully completed.';
GO

PRINT N'--- Users / Shop ---';
GO
:r .\02_Procedures\02_Users\01_Shop\Patch.sql
GO
PRINT N'Users / Shop patch module successfully completed.';
GO

PRINT N'--- Users / Cart ---';
GO
:r .\02_Procedures\02_Users\02_Cart\Patch.sql
GO
PRINT N'Users / Cart patch module successfully completed.';
GO

PRINT N'--- Users / Wishlist ---';
GO
:r .\02_Procedures\02_Users\03_Wishlist\Patch.sql
GO
PRINT N'Users / Wishlist patch module successfully completed.';
GO

PRINT N'--- Users / Auth ---';
GO
:r .\02_Procedures\02_Users\04_Auth\Patch.sql
GO
PRINT N'Users / Auth patch module successfully completed.';
GO

PRINT N'--- Common / Stock ---';
GO
:r .\02_Procedures\03_Common\01_Stock\Patch.sql
GO
PRINT N'Common / Stock patch module successfully completed.';
GO

PRINT N'=== SmartCart database patch completed successfully ===';
GO
