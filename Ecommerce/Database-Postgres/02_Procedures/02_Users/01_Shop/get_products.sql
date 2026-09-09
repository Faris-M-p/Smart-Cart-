/* =============================================================================
   Procedure : get_products
   Source    : GetProducts (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_products(
    p_page_index       INT DEFAULT 1,
    p_page_size        INT DEFAULT 10,
    p_search_name      TEXT DEFAULT NULL,
    p_sort_column      INT DEFAULT 4,
    p_sort_mode        TEXT DEFAULT 'DESC',
    p_category_ids     TEXT DEFAULT NULL,
    p_sub_category_ids TEXT DEFAULT NULL,
    p_brand_ids        TEXT DEFAULT NULL,
    p_price_from       NUMERIC(18, 2) DEFAULT NULL,
    p_price_to         NUMERIC(18, 2) DEFAULT NULL,
    INOUT p_result     refcursor DEFAULT 'p_result',
    INOUT p_result2    refcursor DEFAULT 'p_result2'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_swap         NUMERIC(18, 2);
    v_has_sub_filter BOOLEAN := FALSE;
    v_total_count  INT;
BEGIN
    IF p_page_index IS NULL OR p_page_index < 1 THEN
        p_page_index := 1;
    END IF;
    IF p_page_size IS NULL OR p_page_size < 1 THEN
        p_page_size := 10;
    END IF;
    IF p_page_size > 50 THEN
        p_page_size := 50;
    END IF;
    IF p_search_name = '' THEN
        p_search_name := NULL;
    END IF;
    IF p_category_ids = '' THEN
        p_category_ids := NULL;
    END IF;
    IF p_sub_category_ids = '' THEN
        p_sub_category_ids := NULL;
    END IF;
    IF p_brand_ids = '' THEN
        p_brand_ids := NULL;
    END IF;
    IF p_price_from IS NOT NULL AND p_price_from <= 0 THEN
        p_price_from := NULL;
    END IF;
    IF p_price_to IS NOT NULL AND p_price_to <= 0 THEN
        p_price_to := NULL;
    END IF;
    IF p_price_from IS NOT NULL AND p_price_to IS NOT NULL AND p_price_from > p_price_to THEN
        v_swap := p_price_from;
        p_price_from := p_price_to;
        p_price_to := v_swap;
    END IF;

    DROP TABLE IF EXISTS tmp_sub_ids;
    DROP TABLE IF EXISTS tmp_from_cats;
    DROP TABLE IF EXISTS tmp_brand_filter;
    DROP TABLE IF EXISTS tmp_page_rows;

    CREATE TEMP TABLE tmp_sub_ids (
        id INT PRIMARY KEY
    ) ON COMMIT DROP;

    INSERT INTO tmp_sub_ids (id)
    SELECT DISTINCT x.val
    FROM (
        SELECT unnest(string_to_array(COALESCE(p_sub_category_ids, ''), ',')) AS raw
    ) s
    CROSS JOIN LATERAL (
        SELECT TRIM(s.raw) AS t
    ) t
    CROSS JOIN LATERAL (
        SELECT CASE WHEN t.t ~ '^\d+$' THEN t.t::INT ELSE NULL END AS val
    ) x
    WHERE x.val IS NOT NULL AND x.val > 0;

    IF p_sub_category_ids IS NOT NULL THEN
        v_has_sub_filter := TRUE;
    END IF;

    IF p_category_ids IS NOT NULL THEN
        v_has_sub_filter := TRUE;

        CREATE TEMP TABLE tmp_from_cats (
            id INT PRIMARY KEY
        ) ON COMMIT DROP;

        INSERT INTO tmp_from_cats (id)
        SELECT DISTINCT sc.id_subcategory
        FROM subcategory AS sc
        WHERE COALESCE(sc.cancelled, FALSE) = FALSE
          AND sc.fk_category IN (
              SELECT CASE WHEN TRIM(u) ~ '^\d+$' THEN TRIM(u)::INT ELSE NULL END
              FROM unnest(string_to_array(p_category_ids, ',')) AS u
              WHERE TRIM(u) ~ '^\d+$'
                AND TRIM(u)::INT > 0
          );

        IF EXISTS (SELECT 1 FROM tmp_sub_ids) THEN
            DELETE FROM tmp_sub_ids
            WHERE id NOT IN (SELECT id FROM tmp_from_cats);
        ELSE
            INSERT INTO tmp_sub_ids (id)
            SELECT id FROM tmp_from_cats;
        END IF;
    END IF;

    CREATE TEMP TABLE tmp_brand_filter (
        id INT PRIMARY KEY
    ) ON COMMIT DROP;

    INSERT INTO tmp_brand_filter (id)
    SELECT DISTINCT CASE WHEN TRIM(u) ~ '^\d+$' THEN TRIM(u)::INT ELSE NULL END
    FROM unnest(string_to_array(COALESCE(p_brand_ids, ''), ',')) AS u
    WHERE TRIM(u) ~ '^\d+$'
      AND TRIM(u)::INT > 0;

    CREATE TEMP TABLE tmp_page_rows ON COMMIT DROP AS
    WITH sku_price AS (
        SELECT
            pv.fk_product,
            MIN(pv.selling_price) AS min_price
        FROM product_variants AS pv
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
        GROUP BY pv.fk_product
    ),
    cheapest_sku AS (
        SELECT
            pv.fk_product,
            pv.selling_price,
            pv.mrp,
            ROW_NUMBER() OVER (
                PARTITION BY pv.fk_product
                ORDER BY pv.selling_price ASC, pv.is_default DESC, pv.id_product_variant ASC
            ) AS rn
        FROM product_variants AS pv
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
    ),
    stock_by_product AS (
        SELECT
            pv.fk_product,
            SUM(s.quantity) AS qty
        FROM product_variants AS pv
        INNER JOIN stock AS s
            ON s.fk_product_variant = pv.id_product_variant
           AND COALESCE(s.cancelled, FALSE) = FALSE
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
        GROUP BY pv.fk_product
    ),
    product_image AS (
        SELECT
            pm.fk_product,
            pm.media_url,
            ROW_NUMBER() OVER (
                PARTITION BY pm.fk_product
                ORDER BY pm.is_primary DESC, pm.display_order ASC, pm.id_product_media ASC
            ) AS rn
        FROM product_media AS pm
        WHERE pm.media_type = 'Image'
          AND pm.media_url IS NOT NULL
          AND pm.media_url <> ''
    ),
    sku_image AS (
        SELECT
            pv.fk_product,
            sm.media_url,
            ROW_NUMBER() OVER (
                PARTITION BY pv.fk_product
                ORDER BY sm.is_primary DESC, sm.display_order ASC
            ) AS rn
        FROM product_variants AS pv
        INNER JOIN sku_media AS sm
            ON sm.fk_product_sku = pv.id_product_variant
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
          AND sm.media_type = 'Image'
          AND sm.media_url IS NOT NULL
          AND sm.media_url <> ''
    ),
    filtered AS (
        SELECT
            p.id_product AS product_id,
            p.name,
            p.slug,
            c.id_category AS category_id,
            c.name AS category_name,
            p.fk_subcategory AS sub_category_id,
            COALESCE(p.fk_brand, 0) AS brand_id,
            COALESCE(b.brand_name, '') AS brand_name,
            COALESCE(pi.media_url, si.media_url) AS image_url,
            0 AS rating,
            ''::TEXT AS gender,
            COALESCE(cs.selling_price, 0) AS price,
            COALESCE(cs.mrp, 0) AS mrp,
            CASE WHEN COALESCE(st.qty, 0) > 0 THEN TRUE ELSE FALSE END AS in_stock,
            p.created_at
        FROM products AS p
        INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
        INNER JOIN category AS c ON c.id_category = sc.fk_category
        LEFT JOIN brand AS b ON b.id_brand = p.fk_brand
        LEFT JOIN sku_price AS sp ON sp.fk_product = p.id_product
        LEFT JOIN cheapest_sku AS cs ON cs.fk_product = p.id_product AND cs.rn = 1
        LEFT JOIN stock_by_product AS st ON st.fk_product = p.id_product
        LEFT JOIN product_image AS pi ON pi.fk_product = p.id_product AND pi.rn = 1
        LEFT JOIN sku_image AS si ON si.fk_product = p.id_product AND si.rn = 1
        WHERE COALESCE(p.cancelled, FALSE) = FALSE
          AND p.is_active = TRUE
          AND COALESCE(p.sell_online, FALSE) = TRUE
          AND (
                p_search_name IS NULL
                OR p.name ILIKE '%' || p_search_name || '%'
                OR p.slug ILIKE '%' || p_search_name || '%'
              )
          AND (NOT v_has_sub_filter OR p.fk_subcategory IN (SELECT id FROM tmp_sub_ids))
          AND (
                NOT EXISTS (SELECT 1 FROM tmp_brand_filter)
                OR p.fk_brand IN (SELECT id FROM tmp_brand_filter)
              )
          AND (p_price_from IS NULL OR sp.min_price >= p_price_from)
          AND (p_price_to IS NULL OR sp.min_price <= p_price_to)
    )
    SELECT
        product_id AS "ProductId",
        name AS "Name",
        slug AS "Slug",
        category_id AS "CategoryId",
        category_name AS "CategoryName",
        sub_category_id AS "SubCategoryId",
        brand_id AS "BrandId",
        brand_name AS "BrandName",
        image_url AS "ImageUrl",
        rating AS "Rating",
        gender AS "Gender",
        price AS "Price",
        mrp AS "MRP",
        in_stock AS "InStock",
        created_at AS "CreatedAt"
    FROM filtered;

    SELECT COUNT(*) INTO v_total_count FROM tmp_page_rows;

    OPEN p_result FOR
    SELECT
        "ProductId",
        "Name",
        "Slug",
        "CategoryId",
        "CategoryName",
        "SubCategoryId",
        "BrandId",
        "BrandName",
        "ImageUrl",
        "Rating",
        "Gender",
        "Price",
        "MRP",
        "InStock"
    FROM tmp_page_rows
    ORDER BY
        CASE WHEN p_sort_column = 1 AND p_sort_mode = 'ASC' THEN "Name" END ASC,
        CASE WHEN p_sort_column = 1 AND p_sort_mode = 'DESC' THEN "Name" END DESC,
        CASE WHEN p_sort_column = 2 AND p_sort_mode = 'ASC' THEN "Price" END ASC,
        CASE WHEN p_sort_column = 2 AND p_sort_mode = 'DESC' THEN "Price" END DESC,
        CASE WHEN p_sort_column = 3 AND p_sort_mode = 'ASC' THEN "SubCategoryId" END ASC,
        CASE WHEN p_sort_column = 3 AND p_sort_mode = 'DESC' THEN "SubCategoryId" END DESC,
        CASE WHEN p_sort_column = 4 AND p_sort_mode = 'ASC' THEN "CreatedAt" END ASC,
        CASE WHEN p_sort_column = 4 AND p_sort_mode = 'DESC' THEN "CreatedAt" END DESC,
        CASE WHEN p_sort_column = 0 AND p_sort_mode = 'ASC' THEN "ProductId" END ASC,
        CASE WHEN COALESCE(p_sort_column, 4) NOT IN (0, 1, 2, 3) AND p_sort_mode = 'ASC' THEN "CreatedAt" END ASC,
        CASE WHEN COALESCE(p_sort_column, 4) NOT IN (0, 1, 2, 3) AND p_sort_mode <> 'ASC' THEN "CreatedAt" END DESC,
        "ProductId" DESC
    OFFSET (p_page_index - 1) * p_page_size
    LIMIT p_page_size;

    OPEN p_result2 FOR
    SELECT
        v_total_count AS "TotalCount",
        p_page_size AS "PageSize",
        p_page_index AS "PageIndex";
END;
$$;
