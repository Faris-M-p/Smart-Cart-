/**********************************************************************
Stored Procedure : pro_purchase_list_select
Source           : ProPurchaseListSelect (SQL Server)
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Admin listing of purchase invoices with search, supplier JSON filter,
  date range, sorting, and pagination.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_purchase_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_supplier_ids TEXT DEFAULT '',
    IN p_from_date DATE DEFAULT NULL,
    IN p_to_date DATE DEFAULT NULL,
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 20,
    IN p_sort_column VARCHAR(50) DEFAULT '',
    IN p_sort_mode VARCHAR(5) DEFAULT 'DESC',
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_meta REFCURSOR DEFAULT 'p_meta'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_count BIGINT;
    v_sort_col TEXT := LOWER(TRIM(COALESCE(p_sort_column, '')));
    v_sort_mode TEXT := UPPER(TRIM(COALESCE(p_sort_mode, '')));
    v_sql TEXT;
    v_order TEXT;
BEGIN
    IF COALESCE(p_page_index, 0) < 1 THEN
        p_page_index := 1;
    END IF;
    IF COALESCE(p_page_size, 0) < 1 THEN
        p_page_size := 20;
    END IF;

    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'DESC';
    END IF;

    IF v_sort_col IN ('purchasedate', 'purchase_date') THEN
        v_order := format('p.purchase_date %s', v_sort_mode);
    ELSIF v_sort_col IN ('suppliername', 'supplier_name') THEN
        v_order := format('s.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('totalamount', 'total_amount') THEN
        v_order := format('p.total_amount %s', v_sort_mode);
    ELSIF v_sort_col IN ('invoicenumber', 'invoice_number') THEN
        v_order := format('p.invoice_number %s', v_sort_mode);
    ELSE
        v_order := 'p.id_purchase DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_purchases (
        rn BIGINT,
        id_purchase INT,
        supplier_name TEXT,
        invoice_number TEXT,
        purchase_date DATE,
        total_amount NUMERIC(12,2),
        created_on TIMESTAMP,
        cancelled BOOLEAN,
        cancelled_on TIMESTAMP,
        cancelled_reason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_purchases;

    v_sql := format($q$
        INSERT INTO tmp_purchases (
            rn, id_purchase, supplier_name, invoice_number, purchase_date,
            total_amount, created_on, cancelled, cancelled_on, cancelled_reason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            p.id_purchase,
            s.name,
            p.invoice_number,
            p.purchase_date,
            p.total_amount,
            p.created_on,
            p.cancelled,
            p.cancelled_on,
            p.cancelled_reason
        FROM purchase p
        INNER JOIN supplier s ON s.id_supplier = p.fk_supplier
        WHERE 1 = 1
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR s.name ILIKE '%%' || $1 || '%%'
                OR COALESCE(p.invoice_number, '') ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR p.fk_supplier IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
          AND ($3::DATE IS NULL OR p.purchase_date >= $3::DATE)
          AND ($4::DATE IS NULL OR p.purchase_date <= $4::DATE)
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_supplier_ids, p_from_date, p_to_date;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    OPEN p_result FOR
        SELECT
            id_purchase,
            supplier_name,
            invoice_number,
            purchase_date,
            total_amount,
            created_on,
            cancelled,
            cancelled_on,
            cancelled_reason
        FROM tmp_purchases
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                      AND (p_page_index * p_page_size);

    OPEN p_meta FOR
        SELECT v_total_count AS total_count,
               p_page_index AS page_index,
               p_page_size AS page_size;
END;
$$;
