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
            MIN(pv.sellingprice) AS min_price
        FROM productvariants AS pv
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
        GROUP BY pv.fk_product
    ),
    cheapest_sku AS (
        SELECT
            pv.fk_product,
            pv.sellingprice,
            pv.mrp,
            ROW_NUMBER() OVER (
                PARTITION BY pv.fk_product
                ORDER BY pv.sellingprice ASC, pv.isdefault DESC, pv.id_productvariant ASC
            ) AS rn
        FROM productvariants AS pv
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
    ),
    stock_by_product AS (
        SELECT
            pv.fk_product,
            SUM(s.quantity) AS qty
        FROM productvariants AS pv
        INNER JOIN stock AS s
            ON s.fk_productvariant = pv.id_productvariant
           AND COALESCE(s.cancelled, FALSE) = FALSE
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
        GROUP BY pv.fk_product
    ),
    product_image AS (
        SELECT
            pm.fk_product,
            pm.mediaurl,
            ROW_NUMBER() OVER (
                PARTITION BY pm.fk_product
                ORDER BY pm.isprimary DESC, pm.displayorder ASC, pm.id_productmedia ASC
            ) AS rn
        FROM productmedia AS pm
        WHERE pm.mediatype = 'Image'
          AND pm.mediaurl IS NOT NULL
          AND pm.mediaurl <> ''
    ),
    sku_image AS (
        SELECT
            pv.fk_product,
            sm.mediaurl,
            ROW_NUMBER() OVER (
                PARTITION BY pv.fk_product
                ORDER BY sm.isprimary DESC, sm.displayorder ASC
            ) AS rn
        FROM productvariants AS pv
        INNER JOIN skumedia AS sm
            ON sm.fk_productsku = pv.id_productvariant
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
          AND sm.mediatype = 'Image'
          AND sm.mediaurl IS NOT NULL
          AND sm.mediaurl <> ''
    ),
    filtered AS (
        SELECT
            p.id_product AS productid,
            p.name,
            p.slug,
            c.id_category AS category_id,
            c.name AS category_name,
            p.fk_subcategory AS sub_category_id,
            COALESCE(p.fk_brand, 0) AS brand_id,
            COALESCE(b.brandname, '') AS brandname,
            COALESCE(pi.mediaurl, si.mediaurl) AS imageurl,
            0 AS rating,
            ''::TEXT AS gender,
            COALESCE(cs.sellingprice, 0) AS price,
            COALESCE(cs.mrp, 0) AS mrp,
            CASE WHEN COALESCE(st.qty, 0) > 0 THEN TRUE ELSE FALSE END AS in_stock,
            p.createdat
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
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE
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
        productid AS productid,
        name AS name,
        slug AS slug,
        category_id AS categoryid,
        category_name AS categoryname,
        sub_category_id AS subcategoryid,
        brand_id AS brandid,
        brandname AS brandname,
        imageurl AS imageurl,
        rating AS rating,
        gender AS gender,
        price AS price,
        mrp AS mrp,
        in_stock AS instock,
        createdat AS createdat
    FROM filtered;

    SELECT COUNT(*) INTO v_total_count FROM tmp_page_rows;

    OPEN p_result FOR
    SELECT
        productid,
        name,
        slug,
        categoryid,
        categoryname,
        subcategoryid,
        brandid,
        brandname,
        imageurl,
        rating,
        gender,
        price,
        mrp,
        instock
    FROM tmp_page_rows
    ORDER BY
        CASE WHEN p_sort_column = 1 AND p_sort_mode = 'ASC' THEN name END ASC,
        CASE WHEN p_sort_column = 1 AND p_sort_mode = 'DESC' THEN name END DESC,
        CASE WHEN p_sort_column = 2 AND p_sort_mode = 'ASC' THEN price END ASC,
        CASE WHEN p_sort_column = 2 AND p_sort_mode = 'DESC' THEN price END DESC,
        CASE WHEN p_sort_column = 3 AND p_sort_mode = 'ASC' THEN subcategoryid END ASC,
        CASE WHEN p_sort_column = 3 AND p_sort_mode = 'DESC' THEN subcategoryid END DESC,
        CASE WHEN p_sort_column = 4 AND p_sort_mode = 'ASC' THEN createdat END ASC,
        CASE WHEN p_sort_column = 4 AND p_sort_mode = 'DESC' THEN createdat END DESC,
        CASE WHEN p_sort_column = 0 AND p_sort_mode = 'ASC' THEN productid END ASC,
        CASE WHEN COALESCE(p_sort_column, 4) NOT IN (0, 1, 2, 3) AND p_sort_mode = 'ASC' THEN createdat END ASC,
        CASE WHEN COALESCE(p_sort_column, 4) NOT IN (0, 1, 2, 3) AND p_sort_mode <> 'ASC' THEN createdat END DESC,
        productid DESC
    OFFSET (p_page_index - 1) * p_page_size
    LIMIT p_page_size;

    OPEN p_result2 FOR
    SELECT
        v_total_count AS totalcount,
        p_page_size AS pagesize,
        p_page_index AS pageindex;
END;
$$;
