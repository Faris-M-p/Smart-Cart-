/* =============================================================================
   Procedure : pro_stock_available_select
   Source    : ProStockAvailableSelect (SQL Server)

   PURPOSE
     Returns available stock for:
       • A single ProductVariant (sku)
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
            -1 AS responsecode,
            'Provide either ID_Product or ID_ProductVariant.'::TEXT AS responsemsg,
            0 AS statuscode;
        RETURN;
    END IF;

    IF (p_id_product_variant > 0) THEN
        OPEN p_result FOR
        SELECT
            pv.id_productvariant AS id_productvariant,
            pv.fk_product AS fk_product,
            SUM(s.quantity) AS availablestock
        FROM productvariants AS pv
        LEFT JOIN stock AS s
            ON s.fk_productvariant = pv.id_productvariant
           AND s.cancelled = FALSE
        WHERE pv.id_productvariant = p_id_product_variant
          AND pv.cancelled = FALSE
        GROUP BY pv.id_productvariant, pv.fk_product;
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT
        p.id_product AS id_product,
        SUM(s.quantity) AS availablestock
    FROM products AS p
    INNER JOIN productvariants AS pv
        ON pv.fk_product = p.id_product
       AND pv.cancelled = FALSE
    LEFT JOIN stock AS s
        ON s.fk_productvariant = pv.id_productvariant
       AND s.cancelled = FALSE
    WHERE p.id_product = p_id_product
    GROUP BY p.id_product;
END;
$$;
