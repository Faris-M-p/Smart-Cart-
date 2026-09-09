/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 09/12/2025
Purpose     : Product List with Search, Filters & Pagination
Source      : ProProductListSelect (SQL Server)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_product_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_category_ids TEXT DEFAULT '',
    IN p_filter_subcategory_ids TEXT DEFAULT '',
    IN p_filter_brand_ids TEXT DEFAULT '',
    IN p_filter_status_ids TEXT DEFAULT '',
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
    v_order TEXT;
    v_sql TEXT;
BEGIN
    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'DESC';
    END IF;

    IF v_sort_col IN ('name') THEN
        v_order := format('p.name %s', v_sort_mode);
    ELSIF v_sort_col IN ('createdon', 'created_at', 'createdat') THEN
        v_order := format('p.created_at %s', v_sort_mode);
    ELSIF v_sort_col IN ('price') THEN
        v_order := format($o$(
            SELECT MIN(pv.selling_price) FROM product_variants pv
            WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
        ) %s$o$, v_sort_mode);
    ELSIF v_sort_col IN ('mrp') THEN
        v_order := format($o$(
            SELECT MIN(pv.mrp) FROM product_variants pv
            WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
        ) %s$o$, v_sort_mode);
    ELSIF v_sort_col IN ('id_product', 'idproduct', 'productid') THEN
        v_order := format('p.id_product %s', v_sort_mode);
    ELSE
        v_order := 'p.id_product DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_product (
        rn BIGINT,
        id_product INT,
        name TEXT,
        description TEXT,
        price NUMERIC(10,2),
        mrp NUMERIC(10,2),
        fk_category INT,
        fk_subcategory INT,
        fk_brand INT,
        rating NUMERIC(3,1),
        gender TEXT,
        fk_status INT,
        created_on TIMESTAMP,
        updated_on TIMESTAMP,
        image_data TEXT,
        is_base64 BOOLEAN
    ) ON COMMIT DROP;

    DELETE FROM tmp_product;

    v_sql := format($q$
        INSERT INTO tmp_product (
            rn, id_product, name, description, price, mrp,
            fk_category, fk_subcategory, fk_brand, rating, gender,
            fk_status, created_on, updated_on, image_data, is_base64
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s) AS rn,
            p.id_product,
            p.name,
            p.description,
            COALESCE((
                SELECT MIN(pv.selling_price)
                FROM product_variants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0),
            COALESCE((
                SELECT MIN(pv.mrp)
                FROM product_variants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0),
            sc.fk_category,
            p.fk_subcategory,
            p.fk_brand,
            NULL::NUMERIC(3,1),
            NULL::TEXT,
            CASE WHEN p.is_active THEN 1 ELSE 0 END,
            p.created_at,
            p.modified_at,
            (
                SELECT pm.media_url
                FROM product_media pm
                WHERE pm.fk_product = p.id_product
                  AND pm.media_type = 'Image'
                ORDER BY pm.is_primary DESC, pm.display_order ASC, pm.id_product_media ASC
                LIMIT 1
            ),
            FALSE
        FROM products p
        INNER JOIN subcategory sc ON sc.id_subcategory = p.fk_subcategory
        WHERE p.cancelled = FALSE
          AND (
                COALESCE($1, '') = ''
                OR LENGTH($1) < 2
                OR p.name ILIKE '%%' || $1 || '%%'
              )
          AND (
                -- Subcategory filter takes priority (matches SQL Server IF/ELSE)
                CASE
                    WHEN COALESCE($3, '') <> '' AND COALESCE($3, '') <> '[]' THEN
                        p.fk_subcategory IN (
                            SELECT (elem->>'ID_Value')::INT
                            FROM jsonb_array_elements($3::jsonb) AS elem
                            WHERE (elem->>'ID_Value') ~ '^\d+$'
                        )
                    WHEN COALESCE($2, '') <> '' AND COALESCE($2, '') <> '[]' THEN
                        sc.fk_category IN (
                            SELECT (elem->>'ID_Value')::INT
                            FROM jsonb_array_elements($2::jsonb) AS elem
                            WHERE (elem->>'ID_Value') ~ '^\d+$'
                        )
                    ELSE TRUE
                END
              )
          AND (
                COALESCE($4, '') = ''
                OR COALESCE($4, '') = '[]'
                OR p.fk_brand IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($4::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
          AND (
                COALESCE($5, '') = ''
                OR COALESCE($5, '') = '[]'
                OR (CASE WHEN p.is_active THEN 1 ELSE 0 END) IN (
                    SELECT (elem->>'ID_Value')::INT
                    FROM jsonb_array_elements($5::jsonb) AS elem
                    WHERE (elem->>'ID_Value') ~ '^\d+$'
                )
              )
    $q$, v_order);

    EXECUTE v_sql
        USING p_search_text, p_filter_category_ids, p_filter_subcategory_ids,
              p_filter_brand_ids, p_filter_status_ids;

    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    IF (p_page_index > 0 AND p_page_size > 0) THEN
        OPEN p_result FOR
            SELECT id_product, name, description, price, mrp,
                   fk_category, fk_subcategory, fk_brand, rating, gender,
                   fk_status, created_on, updated_on, image_data, is_base64
            FROM tmp_product
            WHERE rn >= ((p_page_index - 1) * p_page_size) + 1
              AND rn <= (((p_page_index - 1) * p_page_size) + p_page_size);
    ELSE
        OPEN p_result FOR
            SELECT id_product, name, description, price, mrp,
                   fk_category, fk_subcategory, fk_brand, rating, gender,
                   fk_status, created_on, updated_on, image_data, is_base64
            FROM tmp_product;
    END IF;

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
