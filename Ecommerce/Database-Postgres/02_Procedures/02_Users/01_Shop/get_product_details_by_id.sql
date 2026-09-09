/* =============================================================================
   Procedure : get_product_details_by_id
   Source    : GetProductDetailsById (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_product_details_by_id(
    p_stock_id     INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        p.id_product AS productid,
        p.name AS name,
        p.description AS description,
        sc.fk_category AS categoryid,
        p.fk_subcategory AS subcategoryid,
        p.fk_brand AS brandid,
        NULL::TEXT AS gender,
        NULL::INT AS statusid,
        s.id_stock AS stockid,
        pv.sellingprice AS price,
        pv.mrp AS mrp,
        s.quantity AS quantity,
        NULL::INT AS rating
    FROM products AS p
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN productvariants AS pv ON pv.fk_product = p.id_product
    INNER JOIN stock AS s ON s.fk_productvariant = pv.id_productvariant
    WHERE s.id_stock = p_stock_id;
END;
$$;
