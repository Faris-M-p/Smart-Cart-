/* =============================================================================
   Procedure : get_cart
   Source    : GetCart (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_cart(
    p_user_id       INT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_cart_id INT;
BEGIN
    SELECT c.cart_id
    INTO v_cart_id
    FROM cart AS c
    WHERE c.user_id = p_user_id
    LIMIT 1;

    OPEN p_result FOR
    SELECT
        ci.cart_item_id AS "CartItemId",
        ci.product_id AS "ProductId",
        COALESCE(ci.product_variant_id, 0) AS "ProductVariantId",
        p.name AS "Name",
        p.slug AS "Slug",
        CASE
            WHEN COALESCE(pv.variant_label, '') = '' THEN COALESCE(pv.sku, '')
            ELSE pv.variant_label
        END AS "Label",
        COALESCE(pv.sku, '') AS "SKU",
        COALESCE(pv.selling_price, ci.price) AS "Price",
        COALESCE(pv.mrp, ci.price) AS "MRP",
        ci.quantity AS "Quantity",
        COALESCE(pv.selling_price, ci.price) * ci.quantity AS "LineTotal",
        COALESCE(st.qty, 0) AS "StockQuantity",
        CASE
            WHEN COALESCE(p.cancelled, FALSE) = FALSE
             AND p.is_active = TRUE
             AND pv.id_product_variant IS NOT NULL
             AND COALESCE(pv.cancelled, FALSE) = FALSE
             AND pv.is_active = TRUE
             AND COALESCE(st.qty, 0) > 0
            THEN TRUE ELSE FALSE
        END AS "InStock",
        COALESCE((
            SELECT sm.media_url
            FROM sku_media AS sm
            WHERE sm.fk_product_sku = pv.id_product_variant
              AND sm.media_url IS NOT NULL
              AND sm.media_url <> ''
            ORDER BY sm.is_primary DESC, sm.display_order ASC
            LIMIT 1
        ), (
            SELECT pm.media_url
            FROM product_media AS pm
            WHERE pm.fk_product = p.id_product
              AND pm.media_type = 'Image'
              AND pm.media_url IS NOT NULL
              AND pm.media_url <> ''
            ORDER BY pm.is_primary DESC, pm.display_order ASC
            LIMIT 1
        )) AS "ImageUrl"
    FROM cart_items AS ci
    INNER JOIN products AS p ON p.id_product = ci.product_id
    LEFT JOIN product_variants AS pv
        ON pv.id_product_variant = ci.product_variant_id
    LEFT JOIN (
        SELECT fk_product_variant, SUM(quantity) AS qty
        FROM stock
        WHERE COALESCE(cancelled, FALSE) = FALSE
        GROUP BY fk_product_variant
    ) AS st ON st.fk_product_variant = pv.id_product_variant
    WHERE ci.cart_id = v_cart_id
    ORDER BY ci.cart_item_id DESC;

    OPEN p_result2 FOR
    SELECT
        COALESCE(SUM(ci.quantity), 0) AS "TotalQuantity",
        COALESCE(SUM(COALESCE(pv.selling_price, ci.price) * ci.quantity), 0) AS "Subtotal",
        COALESCE(COUNT(1), 0) AS "ItemCount"
    FROM cart_items AS ci
    LEFT JOIN product_variants AS pv
        ON pv.id_product_variant = ci.product_variant_id
    WHERE ci.cart_id = v_cart_id;
END;
$$;
