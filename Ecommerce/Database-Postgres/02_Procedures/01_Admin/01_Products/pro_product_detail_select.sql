/**********************************************************************
Stored Procedure : pro_product_detail_select
Created By       : Muhammed Faris
Created On       : 13/12/2025
Source           : ProProductDetailSelect (SQL Server)

PURPOSE
  Fetch complete product detail including:
    • Product info
    • Product images (product_media)
    • Variants (SKU)
    • Variant Attributes
    • Variant Stock summary
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_detail_select(
    IN p_id_product INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_result2 REFCURSOR DEFAULT 'p_result2',
    INOUT p_result3 REFCURSOR DEFAULT 'p_result3',
    INOUT p_result4 REFCURSOR DEFAULT 'p_result4',
    INOUT p_result5 REFCURSOR DEFAULT 'p_result5'
)
LANGUAGE plpgsql
AS $$
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM products WHERE id_product = p_id_product AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid or deleted Product ID.' AS response_msg, FALSE AS status_code;
        OPEN p_result2 FOR SELECT NULL WHERE FALSE;
        OPEN p_result3 FOR SELECT NULL WHERE FALSE;
        OPEN p_result4 FOR SELECT NULL WHERE FALSE;
        OPEN p_result5 FOR SELECT NULL WHERE FALSE;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- RESULT 1: PRODUCT BASIC INFO
    -------------------------------------------------------------------
    OPEN p_result FOR
        SELECT
            p.id_product,
            p.name,
            p.description,
            COALESCE((
                SELECT MIN(pv.selling_price)
                FROM product_variants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0) AS price,
            COALESCE((
                SELECT MIN(pv.mrp)
                FROM product_variants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0) AS mrp,
            sc.fk_category,
            p.fk_subcategory,
            p.fk_brand,
            NULL::NUMERIC(3,1) AS rating,
            NULL::TEXT AS gender,
            CASE WHEN p.is_active THEN 1 ELSE 0 END AS fk_status,
            p.created_at AS created_on,
            p.modified_at AS updated_on
        FROM products p
        INNER JOIN subcategory sc ON sc.id_subcategory = p.fk_subcategory
        WHERE p.id_product = p_id_product;

    -------------------------------------------------------------------
    -- RESULT 2: PRODUCT IMAGES (product_media; no cancelled column)
    -------------------------------------------------------------------
    OPEN p_result2 FOR
        SELECT
            pm.id_product_media AS id_product_image,
            pm.fk_product,
            pm.media_url AS image,
            pm.created_at AS created_on,
            FALSE AS cancelled
        FROM product_media pm
        WHERE pm.fk_product = p_id_product
        ORDER BY pm.is_primary DESC, pm.display_order ASC, pm.id_product_media ASC;

    -------------------------------------------------------------------
    -- RESULT 3: PRODUCT VARIANTS (SKU)
    -------------------------------------------------------------------
    OPEN p_result3 FOR
        SELECT
            pv.id_product_variant,
            pv.fk_product,
            pv.selling_price AS price_adjustment,
            pv.is_default,
            pv.created_at AS created_on,
            pv.cancelled,
            pv.sku,
            pv.variant_label,
            pv.mrp,
            pv.selling_price
        FROM product_variants pv
        WHERE pv.fk_product = p_id_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
        ORDER BY pv.id_product_variant ASC;

    -------------------------------------------------------------------
    -- RESULT 4: VARIANT ATTRIBUTES
    -------------------------------------------------------------------
    OPEN p_result4 FOR
        SELECT
            pva.fk_product_variant,
            v.name AS variant_name,
            vv.name AS value_name
        FROM product_variant_attributes pva
        INNER JOIN variants v ON v.id_variant = pva.fk_variant
        INNER JOIN variant_values vv ON vv.id_variant_value = pva.fk_variant_value
        WHERE pva.fk_product_variant IN (
            SELECT id_product_variant
            FROM product_variants
            WHERE fk_product = p_id_product AND COALESCE(cancelled, FALSE) = FALSE
        )
        ORDER BY pva.fk_product_variant, COALESCE(v.display_order, 0);

    -------------------------------------------------------------------
    -- RESULT 5: STOCK SUMMARY PER VARIANT
    -------------------------------------------------------------------
    OPEN p_result5 FOR
        SELECT
            pv.id_product_variant,
            COALESCE(SUM(s.quantity), 0) AS available_stock
        FROM product_variants pv
        LEFT JOIN stock s
            ON s.fk_product_variant = pv.id_product_variant
           AND COALESCE(s.cancelled, FALSE) = FALSE
        WHERE pv.fk_product = p_id_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
        GROUP BY pv.id_product_variant;
END;
$$;
