/* =============================================================================
   Procedure : get_shop_filters
   Source    : GetShopFilters (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_shop_filters(
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2',
    INOUT p_result3 refcursor DEFAULT 'p_result3'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        c.id_category AS "Id",
        c.name AS "Name"
    FROM category AS c
    WHERE COALESCE(c.cancelled, FALSE) = FALSE
      AND c.is_active = TRUE
    ORDER BY c.name;

    OPEN p_result2 FOR
    SELECT
        sc.id_subcategory AS "Id",
        sc.name AS "Name",
        sc.fk_category AS "CategoryId"
    FROM subcategory AS sc
    WHERE COALESCE(sc.cancelled, FALSE) = FALSE
      AND sc.is_active = TRUE
    ORDER BY sc.name;

    OPEN p_result3 FOR
    SELECT
        b.id_brand AS "Id",
        b.brand_name AS "Name"
    FROM brand AS b
    WHERE COALESCE(b.cancelled, FALSE) = FALSE
      AND b.is_active = TRUE
    ORDER BY b.brand_name;
END;
$$;
