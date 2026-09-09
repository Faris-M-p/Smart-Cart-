/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Variant Value List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_variant_value_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_variant_ids TEXT DEFAULT '',
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10,
    IN p_sort_column VARCHAR(50) DEFAULT '',
    IN p_sort_mode VARCHAR(5) DEFAULT '',
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

    IF v_sort_col IN ('valuename', 'name') THEN
        v_order := format('vv.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('displayorder', 'display_order') THEN
        v_order := format('vv.display_order %s', v_sort_mode);
    ELSIF v_sort_col IN ('id_variantvalue', 'id_variant_value') THEN
        v_order := format('vv.id_variant_value %s', v_sort_mode);
    ELSE
        v_order := 'vv.id_variant_value DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_variant_value (
        rn BIGINT,
        id_variantvalue INT,
        fk_variant INT,
        value_name TEXT,
        description TEXT,
        value_icon TEXT,
        displayorder INT,
        createdon TIMESTAMP,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_variant_value;

    v_sql := format($q$
        INSERT INTO tmp_variant_value (
            rn, id_variantvalue, fk_variant, value_name, description, value_icon,
            displayorder, createdon, cancelled, cancelledon, cancelledreason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            vv.id_variantvalue,
            vv.fk_variant,
            vv.name,
            vv.description,
            NULL::TEXT,
            vv.displayorder,
            NULL::TIMESTAMP,
            vv.cancelled,
            vv.cancelledon,
            NULL::TEXT
        FROM variantvalues vv
        WHERE 1 = 1
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR vv.name ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR vv.fk_variant IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_variant_ids;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    OPEN p_result FOR
        SELECT id_variantvalue, fk_variant, value_name, description, value_icon,
               displayorder, createdon, cancelled, cancelledon, cancelledreason
        FROM tmp_variant_value
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                      AND (p_page_index * p_page_size);

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
