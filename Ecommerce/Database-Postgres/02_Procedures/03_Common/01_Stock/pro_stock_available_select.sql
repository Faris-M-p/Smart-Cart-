/* =============================================================================
   Procedure : pro_stock_available_select
   Source    : ProStockAvailableSelect (SQL Server)

   PURPOSE
     Returns available stock for:
       • A single ProductVariant (SKU)
       • OR a full Product (sum of all variants)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE pro_stock_available_select(
    p_id_product         INT DEFAULT 0,
    p_id_product_variant INT DEFAULT 0,
    INOUT p_result       refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF (p_id_product = 0 AND p_id_product_variant = 0) THEN
        OPEN p_result FOR
        SELECT
            -1 AS "ResponseCode",
            'Provide either ID_Product or ID_ProductVariant.'::TEXT AS "ResponseMsg",
            0 AS "StatusCode";
        RETURN;
    END IF;

    IF (p_id_product_variant > 0) THEN
        OPEN p_result FOR
        SELECT
            pv.id_product_variant AS "ID_ProductVariant",
            pv.fk_product AS "FK_Product",
            SUM(s.quantity) AS "AvailableStock"
        FROM product_variants AS pv
        LEFT JOIN stock AS s
            ON s.fk_product_variant = pv.id_product_variant
           AND s.cancelled = FALSE
        WHERE pv.id_product_variant = p_id_product_variant
          AND pv.cancelled = FALSE
        GROUP BY pv.id_product_variant, pv.fk_product;
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT
        p.id_product AS "ID_Product",
        SUM(s.quantity) AS "AvailableStock"
    FROM products AS p
    INNER JOIN product_variants AS pv
        ON pv.fk_product = p.id_product
       AND pv.cancelled = FALSE
    LEFT JOIN stock AS s
        ON s.fk_product_variant = pv.id_product_variant
       AND s.cancelled = FALSE
    WHERE p.id_product = p_id_product
    GROUP BY p.id_product;
END;
$$;
