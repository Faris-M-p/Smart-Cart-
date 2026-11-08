-- Required production system/reference data in dependency order.
-- Optional/sample business data belongs in 06_DummyData and is never included here.
\set ON_ERROR_STOP on

\echo Seeding 001_DefaultRoles.sql
\ir ./001_DefaultRoles.sql

\echo Seeding 002_DefaultPermissions.sql
\ir ./002_DefaultPermissions.sql

\echo Seeding 003_DefaultSettings.sql
\ir ./003_DefaultSettings.sql

\echo Seeding 004_PlatformSuperAdmin.sql
\ir ./004_PlatformSuperAdmin.sql

\echo Seeding 005_ModulePresets.sql
\ir ./005_ModulePresets.sql

\echo Seeding 007_ModuleFeatures.sql
\ir ./007_ModuleFeatures.sql

\echo Seeding 008_PurchaseAccess.sql
\ir ./008_PurchaseAccess.sql
