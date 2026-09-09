/**********************************************************************
Stored Procedure : pro_supplier_list_select
Created By       : Muhammed Faris
Created On       : 12/12/2025
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_supplier_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_supplier_ids TEXT DEFAULT '',
    IN p_show_cancelled BOOLEAN DEFAULT FALSE,
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
    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'DESC';
    END IF;

    IF v_sort_col IN ('suppliername', 'name') THEN
        v_order := format('s.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('createdon', 'created_at', 'created_on') THEN
        v_order := format('s.created_at %s', v_sort_mode);
    ELSIF v_sort_col = 'phone' THEN
        v_order := format('s.phone %s', v_sort_mode);
    ELSE
        v_order := 's.id_supplier DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_suppliers (
        rn BIGINT,
        id_supplier BIGINT,
        supplier_name TEXT,
        contact_person TEXT,
        phone TEXT,
        email TEXT,
        gst_number TEXT,
        address TEXT,
        createdon TIMESTAMP,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_suppliers;

    v_sql := format($q$
        INSERT INTO tmp_suppliers (
            rn, id_supplier, supplier_name, contact_person, phone, email,
            gst_number, address, createdon, cancelled, cancelledon, cancelledreason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            s.id_supplier,
            s.name,
            NULL::TEXT,
            s.phone,
            s.email,
            NULL::TEXT,
            s.address,
            s.createdat,
            s.cancelled,
            s.cancelledon,
            s.cancelledreason
        FROM supplier s
        WHERE 1 = 1
          AND ($3 OR s.cancelled = FALSE)
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR s.name ILIKE '%%' || $1 || '%%'
                OR s.phone ILIKE '%%' || $1 || '%%'
                OR s.email ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR s.id_supplier IN (
                    SELECT (elem->>'ID_Value')::BIGINT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_supplier_ids, COALESCE(p_show_cancelled, FALSE);
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    OPEN p_result FOR
        SELECT id_supplier, supplier_name, contact_person, phone, email,
               gst_number, address, createdon, cancelled, cancelledon, cancelledreason
        FROM tmp_suppliers
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                      AND (p_page_index * p_page_size);

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
