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
        o.id_order AS orderid,
        COALESCE(o.ordernumber, 'SC' || lpad(o.id_order::TEXT, 6, '0')) AS ordernumber,
        o.orderdate AS orderdate,
        o.totalamount AS totalamount,
        CASE WHEN COALESCE(o.cancelled, FALSE) = TRUE THEN 'Cancelled' ELSE o.orderstatus END AS orderstatus,
        o.paymentmethod AS paymentmethod,
        COALESCE(o.receivername, '') AS receivername,
        COALESCE(o.phone, '') AS phone,
        COALESCE(o.addressline, '') AS addressline,
        COALESCE(o.city, '') AS city,
        COALESCE(o.pincode, '') AS pincode,
        COALESCE(o.shippingaddress, '') AS shippingaddress,
        COALESCE((
            SELECT p.paymentstatus
            FROM payments AS p
            WHERE p.fk_order = o.id_order
            ORDER BY p.id_payment DESC
            LIMIT 1
        ), 'Pending') AS paymentstatus,
        COALESCE(o.cancelled, FALSE) AS cancelled,
        o.cancelledon AS cancelledon,
        COALESCE(o.cancelledreason, '') AS cancelledreason,
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
        END AS cancancel
    FROM orders AS o
    WHERE o.id_order = p_order_id
      AND o.fk_user = p_user_id;

    OPEN p_result2 FOR
    SELECT
        oi.id_orderitem AS orderitemid,
        oi.fk_product AS productid,
        COALESCE(oi.fk_productvariant, 0) AS productvariantid,
        oi.productname AS name,
        COALESCE(pr.slug, '') AS slug,
        COALESCE(oi.variantlabel, '') AS label,
        COALESCE(oi.sku, '') AS sku,
        oi.unitprice AS price,
        oi.unitprice AS mrp,
        oi.quantity AS quantity,
        oi.linetotal AS linetotal,
        TRUE AS instock,
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
        )) AS imageurl
    FROM orderitems AS oi
    INNER JOIN orders AS o ON o.id_order = oi.fk_order
    LEFT JOIN products AS pr ON pr.id_product = oi.fk_product
    WHERE oi.fk_order = p_order_id
      AND o.fk_user = p_user_id
    ORDER BY oi.id_orderitem;
END;
$$;
