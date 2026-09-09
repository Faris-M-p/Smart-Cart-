/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Select Category List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_category_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_category_ids TEXT DEFAULT '',
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

    IF v_sort_col IN ('categoryname', 'name') THEN
        v_order := format('c.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('categoryid', 'id_category') THEN
        v_order := format('c.id_category %s', v_sort_mode);
    ELSE
        v_order := 'c.id_category DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_categories (
        rn BIGINT,
        category_id INT,
        category_name TEXT,
        description TEXT,
        created_date TIMESTAMP,
        cancelled BOOLEAN,
        cancelled_on TIMESTAMP,
        cancelled_reason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_categories;

    v_sql := format($q$
        INSERT INTO tmp_categories (
            rn, category_id, category_name, description, created_date,
            cancelled, cancelled_on, cancelled_reason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            c.id_category,
            c.name,
            c.description,
            NULL::TIMESTAMP,
            c.cancelled,
            c.cancelled_on,
            c.cancelled_reason
        FROM category c
        WHERE 1 = 1
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR c.name ILIKE '%%' || $1 || '%%'
              )
          AND (
                COALESCE($2, '') = ''
                OR COALESCE($2, '') = '[]'
                OR c.id_category IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($2::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_category_ids;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    IF (p_page_index > 0 AND p_page_size > 0) THEN
        OPEN p_result FOR
            SELECT category_id, category_name, description, created_date,
                   cancelled, cancelled_on, cancelled_reason
            FROM tmp_categories
            WHERE rn >= ((p_page_index - 1) * p_page_size) + 1
              AND rn <= (((p_page_index - 1) * p_page_size) + p_page_size);
    ELSE
        OPEN p_result FOR
            SELECT category_id, category_name, description, created_date,
                   cancelled, cancelled_on, cancelled_reason
            FROM tmp_categories;
    END IF;

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
