/* =============================================================================
   02_Users / 04_Auth / Patch.sql
   ============================================================================= */

\i ./02_Procedures/02_Users/04_Auth/register_user.sql
\echo 'register_user patch successfully completed.'

\i ./02_Procedures/02_Users/04_Auth/get_user_by_email.sql
\echo 'get_user_by_email patch successfully completed.'

\i ./02_Procedures/02_Users/04_Auth/get_user_by_id.sql
\echo 'get_user_by_id patch successfully completed.'
