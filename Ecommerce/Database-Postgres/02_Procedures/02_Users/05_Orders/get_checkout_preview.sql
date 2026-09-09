/* =============================================================================
   Procedure : get_checkout_preview
   Source    : GetCheckoutPreview (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_checkout_preview(
    p_user_id            INT,
    p_product_variant_id INT DEFAULT 0,
    p_quantity           INT DEFAULT 1,
    INOUT p_result       refcursor DEFAULT 'p_result',
    INOUT p_result2      refcursor DEFAULT 'p_result2',
    INOUT p_result3      refcursor DEFAULT 'p_result3'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF COALESCE(p_quantity, 0) < 1 THEN
        p_quantity := 1;
    END IF;

    DROP TABLE IF EXISTS tmp_checkout_lines;
    CREATE TEMP TABLE tmp_checkout_lines (
        product_id         INT NOT NULL,
        product_variant_id INT NOT NULL,
        name               TEXT NOT NULL,
        slug               TEXT NULL,
        label              TEXT NULL,
        sku                TEXT NULL,
        price              NUMERIC(10, 2) NOT NULL,
        mrp                NUMERIC(10, 2) NOT NULL,
        quantity           INT NOT NULL,
        line_total         NUMERIC(10, 2) NOT NULL,
        stock_quantity     INT NOT NULL,
        in_stock           BOOLEAN NOT NULL,
        image_url          TEXT NULL
    ) ON COMMIT DROP;

    IF COALESCE(p_product_variant_id, 0) > 0 THEN
        INSERT INTO tmp_checkout_lines
        SELECT
            p.id_product,
            pv.id_product_variant,
            p.name,
            p.slug,
            CASE
                WHEN COALESCE(pv.variant_label, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variant_label
            END,
            COALESCE(pv.sku, ''),
            pv.selling_price,
            COALESCE(pv.mrp, pv.selling_price),
            p_quantity,
            pv.selling_price * p_quantity,
            COALESCE(st.qty, 0),
            CASE
                WHEN COALESCE(p.cancelled, FALSE) = FALSE
                 AND p.is_active = TRUE
                 AND COALESCE(p.sell_online, FALSE) = TRUE
                 AND COALESCE(pv.cancelled, FALSE) = FALSE
                 AND pv.is_active = TRUE
                 AND COALESCE(pv.sell_online, FALSE) = TRUE
                 AND COALESCE(st.qty, 0) >= p_quantity
                THEN TRUE ELSE FALSE
            END,
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
            ))
        FROM product_variants AS pv
        INNER JOIN products AS p ON p.id_product = pv.fk_product
        LEFT JOIN (
            SELECT fk_product_variant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_product_variant
        ) AS st ON st.fk_product_variant = pv.id_product_variant
        WHERE pv.id_product_variant = p_product_variant_id;
    ELSE
        INSERT INTO tmp_checkout_lines
        SELECT
            ci.product_id,
            COALESCE(ci.product_variant_id, 0),
            p.name,
            p.slug,
            CASE
                WHEN COALESCE(pv.variant_label, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variant_label
            END,
            COALESCE(pv.sku, ''),
            COALESCE(pv.selling_price, ci.price),
            COALESCE(pv.mrp, ci.price),
            ci.quantity,
            COALESCE(pv.selling_price, ci.price) * ci.quantity,
            COALESCE(st.qty, 0),
            CASE
                WHEN COALESCE(p.cancelled, FALSE) = FALSE
                 AND p.is_active = TRUE
                 AND COALESCE(p.sell_online, FALSE) = TRUE
                 AND pv.id_product_variant IS NOT NULL
                 AND COALESCE(pv.cancelled, FALSE) = FALSE
                 AND pv.is_active = TRUE
                 AND COALESCE(pv.sell_online, FALSE) = TRUE
                 AND COALESCE(st.qty, 0) >= ci.quantity
                THEN TRUE ELSE FALSE
            END,
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
            ))
        FROM cart_items AS ci
        INNER JOIN cart AS c ON c.cart_id = ci.cart_id
        INNER JOIN products AS p ON p.id_product = ci.product_id
        LEFT JOIN product_variants AS pv
            ON pv.id_product_variant = ci.product_variant_id
        LEFT JOIN (
            SELECT fk_product_variant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_product_variant
        ) AS st ON st.fk_product_variant = pv.id_product_variant
        WHERE c.user_id = p_user_id;
    END IF;

    OPEN p_result FOR
    SELECT
        product_id AS "ProductId",
        product_variant_id AS "ProductVariantId",
        name AS "Name",
        slug AS "Slug",
        label AS "Label",
        sku AS "SKU",
        price AS "Price",
        mrp AS "MRP",
        quantity AS "Quantity",
        line_total AS "LineTotal",
        stock_quantity AS "StockQuantity",
        in_stock AS "InStock",
        image_url AS "ImageUrl"
    FROM tmp_checkout_lines
    ORDER BY name;

    OPEN p_result2 FOR
    SELECT
        COALESCE(SUM(quantity), 0) AS "TotalQuantity",
        COALESCE(SUM(line_total), 0) AS "Subtotal",
        COALESCE(COUNT(1), 0) AS "ItemCount",
        CASE
            WHEN COUNT(1) > 0 AND MIN(CASE WHEN in_stock THEN 1 ELSE 0 END) = 1
            THEN TRUE ELSE FALSE
        END AS "CanPlace"
    FROM tmp_checkout_lines;

    OPEN p_result3 FOR
    SELECT
        COALESCE(u.full_name, u.user_name) AS "FullName",
        COALESCE(u.phone_number, '') AS "Phone",
        u.email AS "Email"
    FROM users AS u
    WHERE u.user_id = p_user_id
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
