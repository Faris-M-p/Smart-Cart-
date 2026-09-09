/* =============================================================================
   Procedure : get_product_details
   Source    : GetProductDetails (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_product_details(
    p_slug          TEXT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2',
    INOUT p_result3 refcursor DEFAULT 'p_result3',
    INOUT p_result4 refcursor DEFAULT 'p_result4',
    INOUT p_result5 refcursor DEFAULT 'p_result5'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_id INT;
    v_slug_as_int INT;
BEGIN
    v_slug_as_int := CASE WHEN p_slug ~ '^\d+$' THEN p_slug::INT ELSE NULL END;

    IF v_slug_as_int IS NOT NULL AND v_slug_as_int > 0 THEN
        SELECT p.id_product
        INTO v_product_id
        FROM products AS p
        WHERE COALESCE(p.cancelled, FALSE) = FALSE
          AND p.is_active = TRUE
          AND COALESCE(p.sell_online, FALSE) = TRUE
          AND (p.id_product = v_slug_as_int OR p.slug = p_slug);
    ELSE
        SELECT p.id_product
        INTO v_product_id
        FROM products AS p
        WHERE COALESCE(p.cancelled, FALSE) = FALSE
          AND p.is_active = TRUE
          AND COALESCE(p.sell_online, FALSE) = TRUE
          AND p.slug = p_slug;
    END IF;

    OPEN p_result FOR
    SELECT
        p.id_product AS "ProductId",
        p.name AS "Name",
        p.slug AS "Slug",
        COALESCE(p.description, '') AS "Description",
        c.id_category AS "CategoryId",
        c.name AS "CategoryName",
        p.fk_subcategory AS "SubCategoryId",
        sc.name AS "SubCategoryName",
        COALESCE(p.fk_brand, 0) AS "BrandId",
        COALESCE(b.brand_name, '') AS "BrandName"
    FROM products AS p
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN category AS c ON c.id_category = sc.fk_category
    LEFT JOIN brand AS b ON b.id_brand = p.fk_brand
    WHERE p.id_product = v_product_id;

    OPEN p_result2 FOR
    SELECT
        pm.media_url AS "MediaUrl"
    FROM product_media AS pm
    WHERE pm.fk_product = v_product_id
      AND pm.media_type = 'Image'
      AND pm.media_url IS NOT NULL
      AND pm.media_url <> ''
    ORDER BY pm.is_primary DESC, pm.display_order ASC, pm.id_product_media ASC;

    OPEN p_result3 FOR
    SELECT
        pv.id_product_variant AS "ProductVariantId",
        pv.sku AS "SKU",
        CASE WHEN COALESCE(pv.variant_label, '') = '' THEN pv.sku ELSE pv.variant_label END AS "Label",
        pv.selling_price AS "Price",
        pv.mrp AS "MRP",
        CASE WHEN COALESCE(st.qty, 0) > 0 THEN TRUE ELSE FALSE END AS "InStock",
        pv.is_default AS "IsDefault"
    FROM product_variants AS pv
    LEFT JOIN (
        SELECT fk_product_variant, SUM(quantity) AS qty
        FROM stock
        WHERE COALESCE(cancelled, FALSE) = FALSE
        GROUP BY fk_product_variant
    ) AS st ON st.fk_product_variant = pv.id_product_variant
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.is_active = TRUE
      AND COALESCE(pv.sell_online, FALSE) = TRUE
    ORDER BY pv.is_default DESC, pv.selling_price ASC, pv.id_product_variant ASC;

    OPEN p_result4 FOR
    SELECT
        pva.fk_product_variant AS "ProductVariantId",
        v.id_variant AS "VariantId",
        v.name AS "VariantName",
        vv.id_variant_value AS "VariantValueId",
        vv.name AS "VariantValueName"
    FROM product_variant_attributes AS pva
    INNER JOIN product_variants AS pv ON pv.id_product_variant = pva.fk_product_variant
    INNER JOIN variants AS v ON v.id_variant = pva.fk_variant
    INNER JOIN variant_values AS vv ON vv.id_variant_value = pva.fk_variant_value
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.is_active = TRUE
      AND COALESCE(pv.sell_online, FALSE) = TRUE
    ORDER BY v.display_order, v.name, vv.display_order, vv.name;

    OPEN p_result5 FOR
    SELECT
        sm.fk_product_sku AS "ProductVariantId",
        sm.media_url AS "ImageUrl"
    FROM sku_media AS sm
    INNER JOIN product_variants AS pv ON pv.id_product_variant = sm.fk_product_sku
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.is_active = TRUE
      AND COALESCE(pv.sell_online, FALSE) = TRUE
      AND sm.media_type = 'Image'
      AND sm.media_url IS NOT NULL
      AND sm.media_url <> ''
    ORDER BY sm.is_primary DESC, sm.display_order ASC;
END;
$$;
