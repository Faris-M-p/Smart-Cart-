-- =============================================================================
-- SmartCartPro | 030_SalesReturnDetails.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS sales_return_details (
    id_salesreturndetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_salesreturn  INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_sales_return_details_return
        FOREIGN KEY (fk_salesreturn) REFERENCES sales_returns (id_salesreturn),
    CONSTRAINT fk_sales_return_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_return_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_sales_return_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_sales_return_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_sales_return_details_fk_return ON sales_return_details (fk_salesreturn);
CREATE INDEX IF NOT EXISTS ix_sales_return_details_fk_store ON sales_return_details (fk_store);
