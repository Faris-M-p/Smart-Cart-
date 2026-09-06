/* =============================================================================
   05_Seed / Patch.sql
   -----------------------------------------------------------------------------
   Controls which authentication seed scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\05_Seed\Modules.sql
:r .\05_Seed\Permissions.sql
:r .\05_Seed\UserRoles.sql
:r .\05_Seed\UserRolePermissions.sql
:r .\05_Seed\AdminUsers.sql

/* Development sample catalog (LocalDB / testing only).
   Do not uncomment this line for production patches.
   Apply separately against SmartCart LocalDB when listing screens need data:
     sqlcmd -S "(localdb)\MSSQLLocalDB" -d SmartCart -E -I -b -f 65001 -i 05_Seed\DevSampleCatalog.sql
*/
-- :r .\05_Seed\DevSampleCatalog.sql
