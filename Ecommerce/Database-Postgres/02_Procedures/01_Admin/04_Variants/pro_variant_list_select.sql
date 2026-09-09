/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Variant List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_variant_list_select(
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

    IF v_sort_col IN ('variantname', 'name') THEN
        v_order := format('v.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('displayorder', 'display_order') THEN
        v_order := format('v.display_order %s', v_sort_mode);
    ELSIF v_sort_col IN ('id_variant') THEN
        v_order := format('v.id_variant %s', v_sort_mode);
    ELSE
        v_order := 'v.id_variant DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_variant (
        rn BIGINT,
        id_variant INT,
        variant_name TEXT,
        description TEXT,
        displayorder INT,
        createdon TIMESTAMP,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_variant;

    v_sql := format($q$
        INSERT INTO tmp_variant (
            rn, id_variant, variant_name, description, displayorder,
            createdon, cancelled, cancelledon, cancelledreason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            v.id_variant,
            v.name,
            v.description,
            v.displayorder,
            NULL::TIMESTAMP,
            v.cancelled,
            v.cancelledon,
            NULL::TEXT
        FROM variants v
        WHERE 1 = 1
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR v.name ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR v.id_variant IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_variant_ids;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    IF (p_page_index > 0 AND p_page_size > 0) THEN
        OPEN p_result FOR
            SELECT id_variant, variant_name, description, displayorder,
                   createdon, cancelled, cancelledon, cancelledreason
            FROM tmp_variant
            WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                          AND (p_page_index * p_page_size);
    ELSE
        OPEN p_result FOR
            SELECT id_variant, variant_name, description, displayorder,
                   createdon, cancelled, cancelledon, cancelledreason
            FROM tmp_variant;
    END IF;

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
