/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select Product By ID for Admin Edit
Source      : ProProductSelectById (SQL Server)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_product_select_by_id(
    IN p_id_product INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM products WHERE id_product = p_id_product) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Product ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM products WHERE id_product = p_id_product AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Product is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- RESULT: PRODUCT INFO
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
            p.modifiedat AS updated_on,
            (
                SELECT pm.mediaurl
                FROM productmedia pm
                WHERE pm.fk_product = p.id_product
                  AND pm.mediatype = 'Image'
                ORDER BY pm.isprimary DESC, pm.displayorder ASC, pm.id_productmedia ASC
                LIMIT 1
            ) AS image_data,
            FALSE AS is_base64
        FROM products p
        INNER JOIN subcategory sc ON sc.id_subcategory = p.fk_subcategory
        WHERE p.id_product = p_id_product;
END;
$$;
