-- =============================================================================
-- SmartCartPro | 046_PurchaseManagement.sql
-- Simplified Production Purchase Architecture:
-- 1. Purchases & Purchase Details (evolved core tables)
-- 2. Purchase Returns & Purchase Return Details (mirrors sales_returns)
-- 3. Purchase Orders & Purchase Order Details (internal requirement list)
-- 4. Supplier Transactions (single minimal signed financial history)
-- =============================================================================

-- 1. Upgrade existing purchases table
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS invoicedate DATE NULL;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS taxableamount NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS charges NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS roundoff NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS approvedby INT NULL;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS approvedat TIMESTAMP NULL;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS cancelledby INT NULL;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS amountpaid NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchases ADD COLUMN IF NOT EXISTS paymentmethod TEXT NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_purchases_approvedby') THEN
        ALTER TABLE purchases ADD CONSTRAINT fk_purchases_approvedby FOREIGN KEY (approvedby) REFERENCES users(id_user);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_purchases_cancelledby') THEN
        ALTER TABLE purchases ADD CONSTRAINT fk_purchases_cancelledby FOREIGN KEY (cancelledby) REFERENCES users(id_user);
    END IF;
END $$;

-- Update status check to include Pending, Approved, Cancelled (and legacy statuses)
ALTER TABLE purchases DROP CONSTRAINT IF EXISTS ck_purchases_status;
ALTER TABLE purchases ADD CONSTRAINT ck_purchases_status 
    CHECK (status IN ('Pending','Approved','Cancelled','draft','posted','partially_received','received','cancelled'));

-- 2. Upgrade existing purchase_details table
ALTER TABLE purchase_details ADD COLUMN IF NOT EXISTS discountamount NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchase_details ADD COLUMN IF NOT EXISTS taxableamount NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE purchase_details ADD COLUMN IF NOT EXISTS taxrate NUMERIC(7,4) NOT NULL DEFAULT 0;
ALTER TABLE purchase_details ADD COLUMN IF NOT EXISTS taxinclusive BOOLEAN NOT NULL DEFAULT FALSE;

-- 3. Purchase Returns
CREATE TABLE IF NOT EXISTS purchase_returns (
    id_purchasereturn INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store INT NOT NULL REFERENCES store(id_store),
    fk_purchase INT NOT NULL REFERENCES purchases(id_purchase),
    fk_supplier INT NOT NULL REFERENCES suppliers(id_supplier),
    returnnumber TEXT NOT NULL,
    returndate DATE NOT NULL DEFAULT CURRENT_DATE,
    status TEXT NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Approved','Cancelled')),
    totalamount NUMERIC(12,2) NOT NULL DEFAULT 0,
    reason TEXT NULL,
    notes TEXT NULL,
    createdby INT NOT NULL REFERENCES users(id_user),
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),
    postedby INT NULL REFERENCES users(id_user),
    postedat TIMESTAMP NULL,
    cancelled BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_purchase_returns_number UNIQUE (fk_store, returnnumber)
);

CREATE INDEX IF NOT EXISTS ix_purchase_returns_store ON purchase_returns(fk_store);
CREATE INDEX IF NOT EXISTS ix_purchase_returns_purchase ON purchase_returns(fk_purchase);
CREATE INDEX IF NOT EXISTS ix_purchase_returns_supplier ON purchase_returns(fk_supplier);
CREATE INDEX IF NOT EXISTS ix_purchase_returns_date ON purchase_returns(fk_store, returndate);

CREATE TABLE IF NOT EXISTS purchase_return_details (
    id_purchasereturndetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchasereturn INT NOT NULL REFERENCES purchase_returns(id_purchasereturn) ON DELETE CASCADE,
    fk_purchasedetail INT NOT NULL REFERENCES purchase_details(id_purchasedetail),
    fk_store INT NOT NULL REFERENCES store(id_store),
    fk_product INT NOT NULL REFERENCES products(id_product),
    fk_productvariant INT NULL REFERENCES product_variants(id_productvariant),
    quantity NUMERIC(14,3) NOT NULL CHECK (quantity > 0),
    unitcost NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal NUMERIC(12,2) NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_purchase_return_details_return ON purchase_return_details(fk_purchasereturn);
CREATE INDEX IF NOT EXISTS ix_purchase_return_details_detail ON purchase_return_details(fk_purchasedetail);

-- 4. Simple Supplier Financial Transaction History
-- Convention: Positive (+) = supplier owes our business money / available credit; Negative (-) = our business owes the supplier money (payable).
CREATE TABLE IF NOT EXISTS supplier_transactions (
    id_suppliertransaction INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_supplier INT NOT NULL REFERENCES suppliers(id_supplier),
    fk_store INT NOT NULL REFERENCES store(id_store),
    transactionnumber TEXT NOT NULL,
    transactiondate DATE NOT NULL DEFAULT CURRENT_DATE,
    transactiontype TEXT NOT NULL CHECK (transactiontype IN ('Purchase','Return','Payment','Refund','Opening','Reversal')),
    sourcetype TEXT NULL,
    sourceid INT NULL,
    amount NUMERIC(12,2) NOT NULL CHECK (amount >= 0),
    signedamount NUMERIC(12,2) NOT NULL,
    paymentmethod TEXT NULL,
    referenceno TEXT NULL,
    notes TEXT NULL,
    fk_reversal_of INT NULL REFERENCES supplier_transactions(id_suppliertransaction),
    createdby INT NOT NULL REFERENCES users(id_user),
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_supplier_transactions_number UNIQUE (fk_store, transactionnumber)
);

CREATE INDEX IF NOT EXISTS ix_supplier_transactions_supplier ON supplier_transactions(fk_supplier, transactiondate);
CREATE INDEX IF NOT EXISTS ix_supplier_transactions_store ON supplier_transactions(fk_store);

-- 5. Simple Purchase Orders (Internal product requirements / order list)
CREATE TABLE IF NOT EXISTS purchase_orders (
    id_purchaseorder INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store INT NOT NULL REFERENCES store(id_store),
    ordernumber TEXT NOT NULL,
    orderdate DATE NOT NULL DEFAULT CURRENT_DATE,
    fk_supplier INT NULL REFERENCES suppliers(id_supplier),
    status TEXT NOT NULL DEFAULT 'Open' CHECK (status IN ('Open','Closed','Cancelled')),
    notes TEXT NULL,
    createdby INT NOT NULL REFERENCES users(id_user),
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat TIMESTAMP NULL,
    CONSTRAINT uq_purchase_orders_number UNIQUE (fk_store, ordernumber)
);

CREATE INDEX IF NOT EXISTS ix_purchase_orders_store ON purchase_orders(fk_store);
CREATE INDEX IF NOT EXISTS ix_purchase_orders_supplier ON purchase_orders(fk_supplier);
CREATE INDEX IF NOT EXISTS ix_purchase_orders_date ON purchase_orders(fk_store, orderdate);

CREATE TABLE IF NOT EXISTS purchase_order_details (
    id_purchaseorderdetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchaseorder INT NOT NULL REFERENCES purchase_orders(id_purchaseorder) ON DELETE CASCADE,
    fk_product INT NOT NULL REFERENCES products(id_product),
    fk_productvariant INT NULL REFERENCES product_variants(id_productvariant),
    quantity NUMERIC(14,3) NOT NULL CHECK (quantity > 0),
    unitcost NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes TEXT NULL,
    createdat TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_purchase_order_details_order ON purchase_order_details(fk_purchaseorder);


