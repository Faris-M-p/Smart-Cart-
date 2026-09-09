/* =============================================================================
   01_Admin / 04_Variants / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_variant_update ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_update.sql
\echo 'pro_variant_update patch successfully completed.'

\echo '--- pro_variant_list_select ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_list_select.sql
\echo 'pro_variant_list_select patch successfully completed.'

\echo '--- pro_variant_select_by_id ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_select_by_id.sql
\echo 'pro_variant_select_by_id patch successfully completed.'

\echo '--- pro_variant_value_update ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_value_update.sql
\echo 'pro_variant_value_update patch successfully completed.'

\echo '--- pro_variant_value_list_select ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_value_list_select.sql
\echo 'pro_variant_value_list_select patch successfully completed.'

\echo '--- pro_variant_value_select_by_id ---'
\i ./02_Procedures/01_Admin/04_Variants/pro_variant_value_select_by_id.sql
\echo 'pro_variant_value_select_by_id patch successfully completed.'
