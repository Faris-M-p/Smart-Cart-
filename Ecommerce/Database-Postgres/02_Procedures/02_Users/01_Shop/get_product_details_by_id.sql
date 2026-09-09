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
        p.id_product AS "ProductId",
        p.name AS "Name",
        p.description AS "Description",
        sc.fk_category AS "CategoryId",
        p.fk_subcategory AS "SubCategoryId",
        p.fk_brand AS "BrandId",
        NULL::TEXT AS "Gender",
        NULL::INT AS "StatusId",
        s.id_stock AS "StockId",
        pv.selling_price AS "Price",
        pv.mrp AS "MRP",
        s.quantity AS "Quantity",
        NULL::INT AS "Rating"
    FROM products AS p
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN product_variants AS pv ON pv.fk_product = p.id_product
    INNER JOIN stock AS s ON s.fk_product_variant = pv.id_product_variant
    WHERE s.id_stock = p_stock_id;
END;
$$;
