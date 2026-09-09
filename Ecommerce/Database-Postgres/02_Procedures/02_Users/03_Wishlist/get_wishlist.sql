/* =============================================================================
   Procedure : get_wishlist
   Source    : GetWishlist (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_wishlist(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        wi.id_wishlistitem AS wishlistitemid,
        wi.fk_product AS productid,
        p.name AS name,
        p.slug AS slug,
        COALESCE(c.name, '') AS categoryname,
        COALESCE(b.brandname, '') AS brandname,
        COALESCE(pr.price, 0) AS price,
        COALESCE(pr.mrp, 0) AS mrp,
        CASE WHEN COALESCE(pr.stock_qty, 0) > 0 THEN TRUE ELSE FALSE END AS instock,
        COALESCE((
            SELECT pm.mediaurl
            FROM productmedia AS pm
            WHERE pm.fk_product = p.id_product
              AND pm.mediatype = 'Image'
              AND pm.mediaurl IS NOT NULL
              AND pm.mediaurl <> ''
            ORDER BY pm.isprimary DESC, pm.displayorder ASC
            LIMIT 1
        ), '') AS imageurl
    FROM wishlistitems AS wi
    INNER JOIN wishlist AS w ON w.id_wishlist = wi.fk_wishlist
    INNER JOIN products AS p ON p.id_product = wi.fk_product
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN category AS c ON c.id_category = sc.fk_category
    LEFT JOIN brand AS b ON b.id_brand = p.fk_brand
    LEFT JOIN (
        SELECT
            pv.fk_product,
            MIN(pv.sellingprice) AS price,
            MIN(pv.mrp) AS mrp,
            SUM(COALESCE(st.qty, 0)) AS stock_qty
        FROM productvariants AS pv
        LEFT JOIN (
            SELECT fk_productvariant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_productvariant
        ) AS st ON st.fk_productvariant = pv.id_productvariant
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
        GROUP BY pv.fk_product
    ) AS pr ON pr.fk_product = p.id_product
    WHERE w.fk_user = p_user_id
      AND COALESCE(w.cancelled, FALSE) = FALSE
      AND COALESCE(wi.cancelled, FALSE) = FALSE
      AND COALESCE(p.cancelled, FALSE) = FALSE
      AND p.isactive = TRUE
    ORDER BY wi.id_wishlistitem DESC;
END;
$$;
