/* =============================================================================
   Procedure : get_orders
   Source    : GetOrders (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_orders(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        o.order_id AS "OrderId",
        COALESCE(o.order_number, 'SC' || lpad(o.order_id::TEXT, 6, '0')) AS "OrderNumber",
        o.order_date AS "OrderDate",
        o.total_amount AS "TotalAmount",
        CASE WHEN COALESCE(o.cancelled, FALSE) = TRUE THEN 'Cancelled' ELSE o.order_status END AS "OrderStatus",
        o.payment_method AS "PaymentMethod",
        COALESCE((
            SELECT p.payment_status
            FROM payments AS p
            WHERE p.order_id = o.order_id
            ORDER BY p.payment_id DESC
            LIMIT 1
        ), 'Pending') AS "PaymentStatus",
        COALESCE(o.cancelled, FALSE) AS "Cancelled",
        CASE
            WHEN COALESCE(o.cancelled, FALSE) = FALSE
             AND o.order_status IN ('Placed', 'Pending')
             AND NOT EXISTS (
                SELECT 1
                FROM shipping AS s
                WHERE s.order_id = o.order_id
                  AND COALESCE(s.cancelled, FALSE) = FALSE
                  AND s.shipping_status IN ('Shipped', 'Out for delivery', 'Delivered', 'In Transit')
             )
            THEN TRUE
            ELSE FALSE
        END AS "CanCancel",
        COALESCE(counts.item_count, 0) AS "ItemCount",
        COALESCE(first_item.first_product_name, '') AS "FirstProductName",
        COALESCE(first_item.first_image_url, '') AS "FirstImageUrl"
    FROM orders AS o
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS item_count
        FROM order_items AS oi
        WHERE oi.fk_order = o.order_id
    ) AS counts ON TRUE
    LEFT JOIN LATERAL (
        SELECT
            oi.product_name AS first_product_name,
            COALESCE((
                SELECT sm.media_url
                FROM sku_media AS sm
                WHERE sm.fk_product_sku = oi.fk_product_variant
                  AND sm.media_url IS NOT NULL
                  AND sm.media_url <> ''
                ORDER BY sm.is_primary DESC, sm.display_order ASC
                LIMIT 1
            ), (
                SELECT pm.media_url
                FROM product_media AS pm
                WHERE pm.fk_product = oi.fk_product
                  AND pm.media_type = 'Image'
                  AND pm.media_url IS NOT NULL
                  AND pm.media_url <> ''
                ORDER BY pm.is_primary DESC, pm.display_order ASC
                LIMIT 1
            )) AS first_image_url
        FROM order_items AS oi
        WHERE oi.fk_order = o.order_id
        ORDER BY oi.id_order_item
        LIMIT 1
    ) AS first_item ON TRUE
    WHERE o.user_id = p_user_id
    ORDER BY o.order_date DESC, o.order_id DESC;
END;
$$;
