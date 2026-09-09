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
        wi.wishlist_item_id AS "WishlistItemId",
        wi.product_id AS "ProductId",
        p.name AS "Name",
        p.slug AS "Slug",
        COALESCE(c.name, '') AS "CategoryName",
        COALESCE(b.brand_name, '') AS "BrandName",
        COALESCE(pr.price, 0) AS "Price",
        COALESCE(pr.mrp, 0) AS "MRP",
        CASE WHEN COALESCE(pr.stock_qty, 0) > 0 THEN TRUE ELSE FALSE END AS "InStock",
        COALESCE((
            SELECT pm.media_url
            FROM product_media AS pm
            WHERE pm.fk_product = p.id_product
              AND pm.media_type = 'Image'
              AND pm.media_url IS NOT NULL
              AND pm.media_url <> ''
            ORDER BY pm.is_primary DESC, pm.display_order ASC
            LIMIT 1
        ), '') AS "ImageUrl"
    FROM wishlist_items AS wi
    INNER JOIN wishlist AS w ON w.wishlist_id = wi.wishlist_id
    INNER JOIN products AS p ON p.id_product = wi.product_id
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN category AS c ON c.id_category = sc.fk_category
    LEFT JOIN brand AS b ON b.id_brand = p.fk_brand
    LEFT JOIN (
        SELECT
            pv.fk_product,
            MIN(pv.selling_price) AS price,
            MIN(pv.mrp) AS mrp,
            SUM(COALESCE(st.qty, 0)) AS stock_qty
        FROM product_variants AS pv
        LEFT JOIN (
            SELECT fk_product_variant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_product_variant
        ) AS st ON st.fk_product_variant = pv.id_product_variant
        WHERE COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
        GROUP BY pv.fk_product
    ) AS pr ON pr.fk_product = p.id_product
    WHERE w.user_id = p_user_id
      AND COALESCE(w.cancelled, FALSE) = FALSE
      AND COALESCE(wi.cancelled, FALSE) = FALSE
      AND COALESCE(p.cancelled, FALSE) = FALSE
      AND p.is_active = TRUE
    ORDER BY wi.wishlist_item_id DESC;
END;
$$;
