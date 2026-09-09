/**********************************************************************
Stored Procedure : pro_product_variant_select
Created By       : Muhammed Faris
Created On       : 10/12/2025
Source           : ProProductVariantSelect (SQL Server)
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_select(
    IN p_fk_product INT,
    IN p_search_text TEXT DEFAULT '',
    IN p_filter_variant_ids TEXT DEFAULT '',
    IN p_filter_variant_value_ids TEXT DEFAULT '',
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 20,
    IN p_sort_column VARCHAR(50) DEFAULT '',
    IN p_sort_mode VARCHAR(5) DEFAULT 'ASC',
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_meta REFCURSOR DEFAULT 'p_meta'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_count BIGINT;
    v_sort_col TEXT := LOWER(TRIM(COALESCE(p_sort_column, '')));
    v_sort_mode TEXT := UPPER(TRIM(COALESCE(p_sort_mode, 'ASC')));
    v_order TEXT;
BEGIN
    IF (COALESCE(p_fk_product, 0) = 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid FK_Product' AS response_msg, FALSE AS status_code;
        OPEN p_meta FOR
            SELECT 0::BIGINT AS total_count, p_page_index AS page_index, p_page_size AS page_size;
        RETURN;
    END IF;

    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'ASC';
    END IF;

    IF v_sort_col IN ('priceadjustment', 'price_adjustment', 'selling_price') THEN
        v_order := format('price_adjustment %s', v_sort_mode);
    ELSIF v_sort_col IN ('createdon', 'created_on', 'created_at') THEN
        v_order := format('created_on %s', v_sort_mode);
    ELSIF v_sort_col IN ('stockavailable', 'stock_available') THEN
        v_order := format('stock_available %s', v_sort_mode);
    ELSE
        v_order := format('id_product_variant %s', v_sort_mode);
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_sku (
        rn BIGINT,
        id_product_variant INT,
        price_adjustment NUMERIC(10,2),
        is_default BOOLEAN,
        created_on TIMESTAMP,
        attribute_signature TEXT,
        stock_available INT,
        image_url TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_sku;

    INSERT INTO tmp_sku (
        rn, id_product_variant, price_adjustment, is_default, created_on,
        attribute_signature, stock_available, image_url
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY pv.id_product_variant) AS rn,
        pv.id_product_variant,
        pv.selling_price,
        pv.is_default,
        pv.created_at,
        (
            SELECT string_agg(v.name || ':' || vv.name, ' | ' ORDER BY pva.fk_variant)
            FROM product_variant_attributes pva
            INNER JOIN variants v ON pva.fk_variant = v.id_variant
            INNER JOIN variant_values vv ON pva.fk_variant_value = vv.id_variant_value
            WHERE pva.fk_product_variant = pv.id_product_variant
        ),
        COALESCE((
            SELECT SUM(s.quantity)
            FROM stock s
            WHERE s.fk_product_variant = pv.id_product_variant
              AND COALESCE(s.cancelled, FALSE) = FALSE
        ), 0),
        (
            SELECT sm.media_url
            FROM sku_media sm
            WHERE sm.fk_product_sku = pv.id_product_variant
            ORDER BY sm.is_primary DESC, sm.display_order ASC, sm.id_sku_media ASC
            LIMIT 1
        )
    FROM product_variants pv
    WHERE pv.fk_product = p_fk_product
      AND COALESCE(pv.cancelled, FALSE) = FALSE;

    IF (COALESCE(p_search_text, '') <> '' AND LENGTH(p_search_text) >= 2) THEN
        DELETE FROM tmp_sku
        WHERE COALESCE(attribute_signature, '') NOT ILIKE '%' || p_search_text || '%';
    END IF;

    IF (COALESCE(p_filter_variant_ids, '') <> '' AND COALESCE(p_filter_variant_ids, '') <> '[]') THEN
        DELETE FROM tmp_sku
        WHERE id_product_variant NOT IN (
            SELECT DISTINCT pva.fk_product_variant
            FROM product_variant_attributes pva
            WHERE pva.fk_variant IN (
                SELECT (elem->>'ID_Value')::INT
                FROM jsonb_array_elements(p_filter_variant_ids::jsonb) AS elem
                WHERE (elem->>'ID_Value') ~ '^\d+$'
            )
        );
    END IF;

    IF (COALESCE(p_filter_variant_value_ids, '') <> '' AND COALESCE(p_filter_variant_value_ids, '') <> '[]') THEN
        DELETE FROM tmp_sku
        WHERE id_product_variant NOT IN (
            SELECT DISTINCT pva.fk_product_variant
            FROM product_variant_attributes pva
            WHERE pva.fk_variant_value IN (
                SELECT (elem->>'ID_Value')::INT
                FROM jsonb_array_elements(p_filter_variant_value_ids::jsonb) AS elem
                WHERE (elem->>'ID_Value') ~ '^\d+$'
            )
        );
    END IF;

    -- Re-number after filters for stable pagination
    CREATE TEMP TABLE IF NOT EXISTS tmp_sku_sorted (
        rn BIGINT,
        id_product_variant INT,
        price_adjustment NUMERIC(10,2),
        is_default BOOLEAN,
        created_on TIMESTAMP,
        attribute_signature TEXT,
        stock_available INT,
        image_url TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_sku_sorted;

    EXECUTE format($q$
        INSERT INTO tmp_sku_sorted (
            rn, id_product_variant, price_adjustment, is_default, created_on,
            attribute_signature, stock_available, image_url
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s),
            id_product_variant, price_adjustment, is_default, created_on,
            attribute_signature, stock_available, image_url
        FROM tmp_sku
    $q$, v_order);

    SELECT COUNT(*) INTO v_total_count FROM tmp_sku_sorted;

    OPEN p_result FOR
        SELECT
            id_product_variant,
            price_adjustment,
            is_default,
            created_on,
            attribute_signature,
            stock_available,
            image_url
        FROM tmp_sku_sorted
        WHERE rn > ((GREATEST(p_page_index, 1) - 1) * GREATEST(p_page_size, 1))
          AND rn <= (GREATEST(p_page_index, 1) * GREATEST(p_page_size, 1));

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
