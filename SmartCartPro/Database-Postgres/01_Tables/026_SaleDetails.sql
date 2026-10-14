-- =============================================================================
-- SmartCartPro | 026_SaleDetails.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS sale_details (
    id_saledetail   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_sale         INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_sale_details_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_sale_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sale_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_sale_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_sale_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_sale_details_fk_sale ON sale_details (fk_sale);
CREATE INDEX IF NOT EXISTS ix_sale_details_fk_store ON sale_details (fk_store);
