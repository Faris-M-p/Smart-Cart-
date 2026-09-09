/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Select brand List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_brand_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_brand_ids TEXT DEFAULT '',
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

    IF v_sort_col IN ('brandname', 'brand_name') THEN
        v_order := format('b.brand_name %s', v_sort_mode);
    ELSIF v_sort_col IN ('brandid', 'brand_id', 'id_brand') THEN
        v_order := format('b.id_brand %s', v_sort_mode);
    ELSE
        v_order := 'b.id_brand DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_brands (
        rn BIGINT,
        brand_id INT,
        brandname TEXT,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_brands;

    v_sql := format($q$
        INSERT INTO tmp_brands (rn, brand_id, brandname, cancelled, cancelledon, cancelledreason)
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            b.id_brand,
            b.brandname,
            b.cancelled,
            b.cancelledon,
            b.cancelledreason
        FROM brand b
        WHERE 1 = 1
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR b.brandname ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR b.id_brand IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_brand_ids;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    IF (p_page_index > 0 AND p_page_size > 0) THEN
        OPEN p_result FOR
            SELECT brand_id, brandname, cancelled, cancelledon, cancelledreason
            FROM tmp_brands
            WHERE rn >= ((p_page_index - 1) * p_page_size) + 1
              AND rn <= (((p_page_index - 1) * p_page_size) + p_page_size);
    ELSE
        OPEN p_result FOR
            SELECT brand_id, brandname, cancelled, cancelledon, cancelledreason
            FROM tmp_brands;
    END IF;

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
