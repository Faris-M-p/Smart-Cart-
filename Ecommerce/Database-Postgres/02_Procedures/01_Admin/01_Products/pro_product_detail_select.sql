/**********************************************************************
Stored Procedure : pro_product_detail_select
Created By       : Muhammed Faris
Created On       : 13/12/2025
Source           : ProProductDetailSelect (SQL Server)

PURPOSE
  Fetch complete product detail including:
    • Product info
    • Product images (productmedia)
    • variants (sku)
    • Variant Attributes
    • Variant stock summary
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
                SELECT MIN(pv.sellingprice)
                FROM productvariants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0) AS price,
            COALESCE((
                SELECT MIN(pv.mrp)
                FROM productvariants pv
                WHERE pv.fk_product = p.id_product AND COALESCE(pv.cancelled, FALSE) = FALSE
            ), 0) AS mrp,
            sc.fk_category,
            p.fk_subcategory,
            p.fk_brand,
            NULL::NUMERIC(3,1) AS rating,
            NULL::TEXT AS gender,
            CASE WHEN p.isactive THEN 1 ELSE 0 END AS fk_status,
            p.createdat AS createdon,
            p.modifiedat AS updated_on
        FROM products p
        INNER JOIN subcategory sc ON sc.id_subcategory = p.fk_subcategory
        WHERE p.id_product = p_id_product;

    -------------------------------------------------------------------
    -- RESULT 2: PRODUCT IMAGES (productmedia; no cancelled column)
    -------------------------------------------------------------------
    OPEN p_result2 FOR
        SELECT
            pm.id_productmedia AS id_product_image,
            pm.fk_product,
            pm.mediaurl AS image,
            pm.createdat AS createdon,
            FALSE AS cancelled
        FROM productmedia pm
        WHERE pm.fk_product = p_id_product
        ORDER BY pm.isprimary DESC, pm.displayorder ASC, pm.id_productmedia ASC;

    -------------------------------------------------------------------
    -- RESULT 3: PRODUCT variants (sku)
    -------------------------------------------------------------------
    OPEN p_result3 FOR
        SELECT
            pv.id_productvariant,
            pv.fk_product,
            pv.sellingprice AS price_adjustment,
            pv.isdefault,
            pv.createdat AS createdon,
            pv.cancelled,
            pv.sku,
            pv.variantlabel,
            pv.mrp,
            pv.sellingprice
        FROM productvariants pv
        WHERE pv.fk_product = p_id_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
        ORDER BY pv.id_productvariant ASC;

    -------------------------------------------------------------------
    -- RESULT 4: VARIANT ATTRIBUTES
    -------------------------------------------------------------------
    OPEN p_result4 FOR
        SELECT
            pva.fk_productvariant,
            v.name AS variant_name,
            vv.name AS value_name
        FROM productvariantattributes pva
        INNER JOIN variants v ON v.id_variant = pva.fk_variant
        INNER JOIN variantvalues vv ON vv.id_variantvalue = pva.fk_variantvalue
        WHERE pva.fk_productvariant IN (
            SELECT id_productvariant
            FROM productvariants
            WHERE fk_product = p_id_product AND COALESCE(cancelled, FALSE) = FALSE
        )
        ORDER BY pva.fk_productvariant, COALESCE(v.displayorder, 0);

    -------------------------------------------------------------------
    -- RESULT 5: stock SUMMARY PER VARIANT
    -------------------------------------------------------------------
    OPEN p_result5 FOR
        SELECT
            pv.id_productvariant,
            COALESCE(SUM(s.quantity), 0) AS available_stock
        FROM productvariants pv
        LEFT JOIN stock s
            ON s.fk_productvariant = pv.id_productvariant
           AND COALESCE(s.cancelled, FALSE) = FALSE
        WHERE pv.fk_product = p_id_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
        GROUP BY pv.id_productvariant;
END;
$$;
