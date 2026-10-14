-- =============================================================================
-- SmartCartPro | 022_StockMovements.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS stock_movements (
    id_stockmovement INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    movementtype    TEXT NOT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    referencetype   TEXT NULL,
    referenceid     INT NULL,
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    createdby       INT NULL,
    CONSTRAINT fk_stock_movements_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_stock_movements_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_stock_movements_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT fk_stock_movements_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_stock_movements_type
        CHECK (movementtype IN ('purchase','sale','adjustment','transfer_in','transfer_out','return_in','return_out','opening'))
);

CREATE INDEX IF NOT EXISTS ix_stock_movements_fk_store ON stock_movements (fk_store);
CREATE INDEX IF NOT EXISTS ix_stock_movements_fk_product ON stock_movements (fk_product);
CREATE INDEX IF NOT EXISTS ix_stock_movements_createdat ON stock_movements (createdat);
