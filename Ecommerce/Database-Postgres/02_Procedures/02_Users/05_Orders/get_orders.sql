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
        o.id_order AS orderid,
        COALESCE(o.ordernumber, 'SC' || lpad(o.id_order::TEXT, 6, '0')) AS ordernumber,
        o.orderdate AS orderdate,
        o.totalamount AS totalamount,
        CASE WHEN COALESCE(o.cancelled, FALSE) = TRUE THEN 'Cancelled' ELSE o.orderstatus END AS orderstatus,
        o.paymentmethod AS paymentmethod,
        COALESCE((
            SELECT p.paymentstatus
            FROM payments AS p
            WHERE p.fk_order = o.id_order
            ORDER BY p.id_payment DESC
            LIMIT 1
        ), 'Pending') AS paymentstatus,
        COALESCE(o.cancelled, FALSE) AS cancelled,
        CASE
            WHEN COALESCE(o.cancelled, FALSE) = FALSE
             AND o.orderstatus IN ('Placed', 'Pending')
             AND NOT EXISTS (
                SELECT 1
                FROM shipping AS s
                WHERE s.fk_order = o.id_order
                  AND COALESCE(s.cancelled, FALSE) = FALSE
                  AND s.shippingstatus IN ('Shipped', 'Out for delivery', 'Delivered', 'In Transit')
             )
            THEN TRUE
            ELSE FALSE
        END AS cancancel,
        COALESCE(counts.item_count, 0) AS itemcount,
        COALESCE(first_item.first_product_name, '') AS firstproductname,
        COALESCE(first_item.first_image_url, '') AS firstimageurl
    FROM orders AS o
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS item_count
        FROM orderitems AS oi
        WHERE oi.fk_order = o.id_order
    ) AS counts ON TRUE
    LEFT JOIN LATERAL (
        SELECT
            oi.productname AS first_product_name,
            COALESCE((
                SELECT sm.mediaurl
                FROM skumedia AS sm
                WHERE sm.fk_productsku = oi.fk_productvariant
                  AND sm.mediaurl IS NOT NULL
                  AND sm.mediaurl <> ''
                ORDER BY sm.isprimary DESC, sm.displayorder ASC
                LIMIT 1
            ), (
                SELECT pm.mediaurl
                FROM productmedia AS pm
                WHERE pm.fk_product = oi.fk_product
                  AND pm.mediatype = 'Image'
                  AND pm.mediaurl IS NOT NULL
                  AND pm.mediaurl <> ''
                ORDER BY pm.isprimary DESC, pm.displayorder ASC
                LIMIT 1
            )) AS first_image_url
        FROM orderitems AS oi
        WHERE oi.fk_order = o.id_order
        ORDER BY oi.id_orderitem
        LIMIT 1
    ) AS first_item ON TRUE
    WHERE o.fk_user = p_user_id
    ORDER BY o.orderdate DESC, o.id_order DESC;
END;
$$;
