/**********************************************************************
Stored Procedure : pro_product_variant_stock_select
Source           : ProProductVariantStockSelect (SQL Server)
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Paginated stock batches for a product variant (sku), with purchase /
  supplier context and totals summary.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_stock_select(
    IN p_fk_product_variant INT,
    IN p_include_cancelled BOOLEAN DEFAULT FALSE,
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 20,
    IN p_sort_column VARCHAR(50) DEFAULT '',
    IN p_sort_mode VARCHAR(5) DEFAULT 'ASC',
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_meta REFCURSOR DEFAULT 'p_meta'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_batch_count INT;
    v_total_stock INT;
    v_sort_col TEXT := LOWER(TRIM(COALESCE(p_sort_column, '')));
    v_sort_mode TEXT := UPPER(TRIM(COALESCE(p_sort_mode, '')));
    v_order TEXT;
BEGIN
    IF COALESCE(p_fk_product_variant, 0) = 0 THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Invalid FK_ProductVariant.' AS response_msg,
                   FALSE AS status_code;
        OPEN p_meta FOR
            SELECT 0 AS total_stock, 0 AS total_batch_count,
                   p_page_index AS page_index, p_page_size AS page_size
            WHERE FALSE;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM productvariants
        WHERE id_productvariant = p_fk_product_variant
          AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'ProductVariant does not exist or is deleted.' AS response_msg,
                   FALSE AS status_code;
        OPEN p_meta FOR
            SELECT 0 AS total_stock, 0 AS total_batch_count,
                   p_page_index AS page_index, p_page_size AS page_size
            WHERE FALSE;
        RETURN;
    END IF;

    IF COALESCE(p_page_index, 0) < 1 THEN
        p_page_index := 1;
    END IF;
    IF COALESCE(p_page_size, 0) < 1 THEN
        p_page_size := 20;
    END IF;

    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'ASC';
    END IF;

    IF v_sort_col = 'quantity' THEN
        v_order := format('s.quantity %s', v_sort_mode);
    ELSIF v_sort_col IN ('createdon', 'created_on') THEN
        v_order := format('s.created_on %s', v_sort_mode);
    ELSIF v_sort_col IN ('purchasedate', 'purchase_date') THEN
        v_order := format('p.purchase_date %s', v_sort_mode);
    ELSIF v_sort_col IN ('expirydate', 'expiry_date') THEN
        v_order := format('pd.expiry_date %s', v_sort_mode);
    ELSE
        v_order := format('s.created_on %s', v_sort_mode);
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_stock (
        rn BIGINT,
        id_stock INT,
        fk_productvariant INT,
        fk_purchasedetail INT,
        quantity INT,
        createdon TIMESTAMP,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT,
        purchasedate DATE,
        purchaseprice NUMERIC(10,2),
        mrp NUMERIC(10,2),
        expirydate DATE,
        supplier_name TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_stock;

    EXECUTE format($q$
        INSERT INTO tmp_stock (
            rn, id_stock, fk_productvariant, fk_purchasedetail, quantity,
            createdon, cancelled, cancelledon, cancelledreason,
            purchasedate, purchaseprice, mrp, expirydate, supplier_name
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            s.id_stock,
            s.fk_productvariant,
            s.fk_purchasedetail,
            s.quantity,
            s.createdon,
            s.cancelled,
            s.cancelledon,
            s.cancelledreason,
            p.purchasedate,
            pd.purchaseprice,
            pd.mrp,
            pd.expirydate,
            sup.name
        FROM stock s
        INNER JOIN purchasedetail pd ON pd.id_purchasedetail = s.fk_purchasedetail
        INNER JOIN purchase p ON p.id_purchase = pd.fk_purchase
        INNER JOIN supplier sup ON sup.id_supplier = p.fk_supplier
        WHERE s.fk_productvariant = $1
          AND ($2 OR s.cancelled = FALSE)
    $q$, v_order)
    USING p_fk_product_variant, COALESCE(p_include_cancelled, FALSE);

    SELECT COUNT(*) INTO v_total_batch_count FROM tmp_stock;
    SELECT COALESCE(SUM(quantity), 0) INTO v_total_stock
    FROM tmp_stock
    WHERE cancelled = FALSE;

    OPEN p_result FOR
        SELECT
            id_stock,
            fk_productvariant,
            fk_purchasedetail,
            quantity,
            createdon,
            cancelled,
            cancelledon,
            cancelledreason,
            purchasedate,
            purchaseprice,
            mrp,
            expirydate,
            supplier_name
        FROM tmp_stock
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                      AND (p_page_index * p_page_size);

    OPEN p_meta FOR
        SELECT v_total_stock AS total_stock,
               v_total_batch_count AS total_batch_count,
               p_page_index AS page_index,
               p_page_size AS page_size;
END;
$$;
