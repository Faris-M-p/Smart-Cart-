/**********************************************************************
Stored Procedure : get_admin_order
Source           : GetAdminOrder (SQL Server)
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Admin order header and line items (any customer).
**********************************************************************/
CREATE OR REPLACE PROCEDURE get_admin_order(
    IN p_order_id INT,
    INOUT p_header REFCURSOR DEFAULT 'p_header',
    INOUT p_items REFCURSOR DEFAULT 'p_items'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_header FOR
        SELECT
            o.order_id,
            COALESCE(o.order_number, 'SC' || LPAD(o.order_id::TEXT, 6, '0')) AS order_number,
            o.order_date,
            o.total_amount,
            CASE WHEN COALESCE(o.cancelled, FALSE) THEN 'Cancelled' ELSE o.order_status END AS order_status,
            o.payment_method,
            COALESCE((
                SELECT p.payment_status
                FROM payments p
                WHERE p.order_id = o.order_id
                ORDER BY p.payment_id DESC
                LIMIT 1
            ), 'Pending') AS payment_status,
            COALESCE((
                SELECT s.shipping_status
                FROM shipping s
                WHERE s.order_id = o.order_id
                ORDER BY s.shipping_id DESC
                LIMIT 1
            ), 'Pending') AS shipping_status,
            COALESCE(o.receiver_name, '') AS receiver_name,
            COALESCE(o.phone, '') AS phone,
            COALESCE(o.address_line, '') AS address_line,
            COALESCE(o.city, '') AS city,
            COALESCE(o.pincode, '') AS pincode,
            COALESCE(o.shipping_address, '') AS shipping_address,
            COALESCE(NULLIF(TRIM(u.full_name), ''), COALESCE(u.user_name, '')) AS customer_name,
            COALESCE(u.email, '') AS customer_email,
            COALESCE(o.cancelled, FALSE) AS cancelled,
            o.cancelled_on,
            COALESCE(o.cancelled_reason, '') AS cancelled_reason,
            (NOT COALESCE(o.cancelled, FALSE) AND o.order_status IN ('Placed', 'Pending')) AS can_confirm,
            (NOT COALESCE(o.cancelled, FALSE) AND o.order_status = 'Confirmed') AS can_update_status,
            (NOT COALESCE(o.cancelled, FALSE) AND o.order_status IN ('Confirmed', 'Shipped')) AS can_deliver,
            (NOT COALESCE(o.cancelled, FALSE) AND o.order_status IN ('Placed', 'Pending', 'Confirmed')) AS can_cancel
        FROM orders o
        LEFT JOIN users u ON u.user_id = o.user_id
        WHERE o.order_id = p_order_id;

    OPEN p_items FOR
        SELECT
            oi.id_order_item AS order_item_id,
            oi.fk_product AS product_id,
            COALESCE(oi.fk_product_variant, 0) AS product_variant_id,
            oi.product_name AS name,
            COALESCE(pr.slug, '') AS slug,
            COALESCE(oi.variant_label, '') AS label,
            COALESCE(oi.sku, '') AS sku,
            oi.unit_price AS price,
            oi.unit_price AS mrp,
            oi.quantity,
            oi.line_total,
            TRUE AS in_stock,
            COALESCE((
                SELECT sm.media_url
                FROM sku_media sm
                WHERE sm.fk_product_sku = oi.fk_product_variant
                  AND sm.media_url IS NOT NULL
                  AND sm.media_url <> ''
                ORDER BY sm.is_primary DESC, sm.display_order ASC
                LIMIT 1
            ), (
                SELECT pm.media_url
                FROM product_media pm
                WHERE pm.fk_product = oi.fk_product
                  AND pm.media_type = 'Image'
                  AND pm.media_url IS NOT NULL
                  AND pm.media_url <> ''
                ORDER BY pm.is_primary DESC, pm.display_order ASC
                LIMIT 1
            )) AS image_url
        FROM order_items oi
        LEFT JOIN products pr ON pr.id_product = oi.fk_product
        WHERE oi.fk_order = p_order_id
        ORDER BY oi.id_order_item;
END;
$$;
