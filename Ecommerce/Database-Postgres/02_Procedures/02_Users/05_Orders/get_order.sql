/* =============================================================================
   Procedure : get_order
   Source    : GetOrder (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_order(
    p_user_id       INT,
    p_order_id      INT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2'
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
        COALESCE(o.receiver_name, '') AS "ReceiverName",
        COALESCE(o.phone, '') AS "Phone",
        COALESCE(o.address_line, '') AS "AddressLine",
        COALESCE(o.city, '') AS "City",
        COALESCE(o.pincode, '') AS "Pincode",
        COALESCE(o.shipping_address, '') AS "ShippingAddress",
        COALESCE((
            SELECT p.payment_status
            FROM payments AS p
            WHERE p.order_id = o.order_id
            ORDER BY p.payment_id DESC
            LIMIT 1
        ), 'Pending') AS "PaymentStatus",
        COALESCE(o.cancelled, FALSE) AS "Cancelled",
        o.cancelled_on AS "CancelledOn",
        COALESCE(o.cancelled_reason, '') AS "CancelledReason",
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
        END AS "CanCancel"
    FROM orders AS o
    WHERE o.order_id = p_order_id
      AND o.user_id = p_user_id;

    OPEN p_result2 FOR
    SELECT
        oi.id_order_item AS "OrderItemId",
        oi.fk_product AS "ProductId",
        COALESCE(oi.fk_product_variant, 0) AS "ProductVariantId",
        oi.product_name AS "Name",
        COALESCE(pr.slug, '') AS "Slug",
        COALESCE(oi.variant_label, '') AS "Label",
        COALESCE(oi.sku, '') AS "SKU",
        oi.unit_price AS "Price",
        oi.unit_price AS "MRP",
        oi.quantity AS "Quantity",
        oi.line_total AS "LineTotal",
        TRUE AS "InStock",
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
        )) AS "ImageUrl"
    FROM order_items AS oi
    INNER JOIN orders AS o ON o.order_id = oi.fk_order
    LEFT JOIN products AS pr ON pr.id_product = oi.fk_product
    WHERE oi.fk_order = p_order_id
      AND o.user_id = p_user_id
    ORDER BY oi.id_order_item;
END;
$$;
