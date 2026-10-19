-- =============================================================================
-- V007__Phase7OnlineOrders.sql
-- Phase 7: Add fk_onlineorderitem to product_reviews
--   Enables one-review-per-order-item enforcement + unique constraint.
-- Idempotent via IF NOT EXISTS / DO blocks.
-- =============================================================================
\set ON_ERROR_STOP on

-- 1. Add fk_onlineorderitem column to product_reviews (nullable — existing rows keep NULL)
ALTER TABLE product_reviews
    ADD COLUMN IF NOT EXISTS fk_onlineorderitem INT NULL;

-- 2. Add FK constraint (idempotent guard via pg_constraint)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_product_reviews_onlineorderitem'
    ) THEN
        ALTER TABLE product_reviews
            ADD CONSTRAINT fk_product_reviews_onlineorderitem
                FOREIGN KEY (fk_onlineorderitem)
                REFERENCES online_order_items (id_onlineorderitem);
    END IF;
END;
$$;

-- 3. Unique: one review per (customer, order item) — partial index (where NOT NULL)
CREATE UNIQUE INDEX IF NOT EXISTS uq_product_reviews_customer_orderitem
    ON product_reviews (fk_customer, fk_onlineorderitem)
    WHERE fk_onlineorderitem IS NOT NULL;

-- 4. Index for order lookup
CREATE INDEX IF NOT EXISTS ix_product_reviews_fk_onlineorderitem
    ON product_reviews (fk_onlineorderitem)
    WHERE fk_onlineorderitem IS NOT NULL;
