-- Baseline metadata comment. patch_history's table definition lives in 01_Tables.
-- The V001 baseline runner inserts its tracking row after all tables and required
-- seed data have completed successfully, within the same transaction.

COMMENT ON TABLE patch_history IS
    'SmartCartPro baseline V001; successful follow-up patches are recorded in patch_history.';
