-- =============================================================================
-- SmartCartPro | V002__CompanyStoreRelationships.sql
-- Reserved for incremental company/store integrity enhancements.
-- V001 already creates company, store, uq_store_one_default, and fk_store FKs.
-- This patch is a no-op marker so ApplyPatches.bat has a second sample version
-- after V001; future ALTER TABLE statements for composite FKs may land here.
-- =============================================================================
\set ON_ERROR_STOP on

-- Documented application invariants (no DDL change required yet):
-- * Create company + default store in one transaction.
-- * Promote a new default store before cancelling/deactivating the current default.
-- * Store-scoped rows must reference a store that belongs to the same client DB company.

SELECT 1;
