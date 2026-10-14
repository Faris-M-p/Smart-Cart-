-- =============================================================================
-- SmartCartPro | 024_PurchaseDetails.sql
-- Scope: Store-scoped (inherits store via purchase)
-- =============================================================================

CREATE TABLE IF NOT EXISTS purchase_details (
    id_purchasedetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchase     INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitcost        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_purchase_details_purchase
        FOREIGN KEY (fk_purchase) REFERENCES purchases (id_purchase),
    CONSTRAINT fk_purchase_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_purchase_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_purchase_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_purchase_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_purchase_details_fk_purchase ON purchase_details (fk_purchase);
CREATE INDEX IF NOT EXISTS ix_purchase_details_fk_store ON purchase_details (fk_store);
