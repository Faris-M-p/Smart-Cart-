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
        c.id_category AS id,
        c.name AS name
    FROM category AS c
    WHERE COALESCE(c.cancelled, FALSE) = FALSE
      AND c.isactive = TRUE
    ORDER BY c.name;

    OPEN p_result2 FOR
    SELECT
        sc.id_subcategory AS id,
        sc.name AS name,
        sc.fk_category AS categoryid
    FROM subcategory AS sc
    WHERE COALESCE(sc.cancelled, FALSE) = FALSE
      AND sc.isactive = TRUE
    ORDER BY sc.name;

    OPEN p_result3 FOR
    SELECT
        b.id_brand AS id,
        b.brandname AS name
    FROM brand AS b
    WHERE COALESCE(b.cancelled, FALSE) = FALSE
      AND b.isactive = TRUE
    ORDER BY b.brandname;
END;
$$;
