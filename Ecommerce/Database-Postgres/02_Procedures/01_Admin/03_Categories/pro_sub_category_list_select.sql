/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Select SubCategory List with Filtering & Pagination
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_sub_category_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_category_ids TEXT DEFAULT '',
    IN p_filter_sub_category_ids TEXT DEFAULT '',
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

    IF v_sort_col IN ('subcategoryname', 'name') THEN
        v_order := format('s.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('id_subcategory', 'subcategoryid') THEN
        v_order := format('s.id_subcategory %s', v_sort_mode);
    ELSE
        v_order := 's.id_subcategory DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_subcat (
        rn BIGINT,
        id_subcategory INT,
        subcategory_name TEXT,
        fk_category INT,
        description TEXT,
        created_date TIMESTAMP,
        cancelled BOOLEAN,
        cancelled_on TIMESTAMP,
        cancelled_reason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_subcat;

    v_sql := format($q$
        INSERT INTO tmp_subcat (
            rn, id_subcategory, subcategory_name, fk_category, description,
            created_date, cancelled, cancelled_on, cancelled_reason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            s.id_subcategory,
            s.name,
            s.fk_category,
            s.description,
            NULL::TIMESTAMP,
            s.cancelled,
            s.cancelled_on,
            s.cancelled_reason
        FROM subcategory s
        WHERE s.cancelled = FALSE
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR s.name ILIKE '%%' || $1 || '%%'
              )
          AND (
                CASE
                    WHEN COALESCE($3, '') <> '' AND COALESCE($3, '') <> '[]' THEN
                        s.id_subcategory IN (
                            SELECT (elem->>'ID_Value')::INT
                            FROM jsonb_array_elements($3::jsonb) AS elem
                            WHERE (elem->>'ID_Value') ~ '^\d+$'
                        )
                    WHEN COALESCE($2, '') <> '' AND COALESCE($2, '') <> '[]' THEN
                        s.fk_category IN (
                            SELECT (elem->>'ID_Value')::INT
                            FROM jsonb_array_elements($2::jsonb) AS elem
                            WHERE (elem->>'ID_Value') ~ '^\d+$'
                        )
                    ELSE TRUE
                END
              )
    $q$, v_order);

    EXECUTE v_sql USING p_search_text, p_filter_category_ids, p_filter_sub_category_ids;
    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    OPEN p_result FOR
        SELECT id_subcategory, subcategory_name, fk_category, description,
               created_date, cancelled, cancelled_on, cancelled_reason
        FROM tmp_subcat
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                     AND ((p_page_index - 1) * p_page_size + p_page_size);

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
