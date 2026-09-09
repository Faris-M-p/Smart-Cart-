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
            o.id_order AS orderid,
            COALESCE(o.ordernumber, 'SC' || LPAD(o.id_order::TEXT, 6, '0')) AS ordernumber,
            o.orderdate,
            o.totalamount,
            CASE WHEN COALESCE(o.cancelled, FALSE) THEN 'Cancelled' ELSE o.orderstatus END AS orderstatus,
            o.paymentmethod,
            COALESCE((
                SELECT p.paymentstatus
                FROM payments p
                WHERE p.fk_order = o.id_order
                ORDER BY p.id_payment DESC
                LIMIT 1
            ), 'Pending') AS paymentstatus,
            COALESCE((
                SELECT s.shippingstatus
                FROM shipping s
                WHERE s.fk_order = o.id_order
                ORDER BY s.id_shipping DESC
                LIMIT 1
            ), 'Pending') AS shippingstatus,
            COALESCE(o.receivername, '') AS receivername,
            COALESCE(o.phone, '') AS phone,
            COALESCE(o.addressline, '') AS addressline,
            COALESCE(o.city, '') AS city,
            COALESCE(o.pincode, '') AS pincode,
            COALESCE(o.shippingaddress, '') AS shippingaddress,
            COALESCE(NULLIF(TRIM(u.fullname), ''), COALESCE(u.username, '')) AS customer_name,
            COALESCE(u.email, '') AS customer_email,
            COALESCE(o.cancelled, FALSE) AS cancelled,
            o.cancelledon,
            COALESCE(o.cancelledreason, '') AS cancelledreason,
            (NOT COALESCE(o.cancelled, FALSE) AND o.orderstatus IN ('Placed', 'Pending')) AS can_confirm,
            (NOT COALESCE(o.cancelled, FALSE) AND o.orderstatus = 'Confirmed') AS can_update_status,
            (NOT COALESCE(o.cancelled, FALSE) AND o.orderstatus IN ('Confirmed', 'Shipped')) AS can_deliver,
            (NOT COALESCE(o.cancelled, FALSE) AND o.orderstatus IN ('Placed', 'Pending', 'Confirmed')) AS can_cancel
        FROM orders o
        LEFT JOIN users u ON u.id_user = o.fk_user
        WHERE o.id_order = p_order_id;

    OPEN p_items FOR
        SELECT
            oi.id_orderitem AS order_item_id,
            oi.fk_product AS productid,
            COALESCE(oi.fk_productvariant, 0) AS productvariantid,
            oi.productname AS name,
            COALESCE(pr.slug, '') AS slug,
            COALESCE(oi.variantlabel, '') AS label,
            COALESCE(oi.sku, '') AS sku,
            oi.unitprice AS price,
            oi.unitprice AS mrp,
            oi.quantity,
            oi.linetotal,
            TRUE AS in_stock,
            COALESCE((
                SELECT sm.mediaurl
                FROM skumedia sm
                WHERE sm.fk_productsku = oi.fk_productvariant
                  AND sm.mediaurl IS NOT NULL
                  AND sm.mediaurl <> ''
                ORDER BY sm.isprimary DESC, sm.displayorder ASC
                LIMIT 1
            ), (
                SELECT pm.mediaurl
                FROM productmedia pm
                WHERE pm.fk_product = oi.fk_product
                  AND pm.mediatype = 'Image'
                  AND pm.mediaurl IS NOT NULL
                  AND pm.mediaurl <> ''
                ORDER BY pm.isprimary DESC, pm.displayorder ASC
                LIMIT 1
            )) AS imageurl
        FROM orderitems oi
        LEFT JOIN products pr ON pr.id_product = oi.fk_product
        WHERE oi.fk_order = p_order_id
        ORDER BY oi.id_orderitem;
END;
$$;
