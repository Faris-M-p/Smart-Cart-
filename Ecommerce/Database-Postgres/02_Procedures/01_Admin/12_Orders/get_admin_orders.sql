/**********************************************************************
Stored Procedure : get_admin_orders
Source           : GetAdminOrders (SQL Server)
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Admin order list for all customers. Search, status, date, paging.
**********************************************************************/
CREATE OR REPLACE PROCEDURE get_admin_orders(
    IN p_search_text TEXT DEFAULT '',
    IN p_order_status TEXT DEFAULT '',
    IN p_from_date DATE DEFAULT NULL,
    IN p_to_date DATE DEFAULT NULL,
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10,
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_meta REFCURSOR DEFAULT 'p_meta'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_count INT;
    v_search TEXT;
    v_status TEXT;
BEGIN
    IF COALESCE(p_page_index, 0) < 1 THEN
        p_page_index := 1;
    END IF;
    IF COALESCE(p_page_size, 0) < 1 THEN
        p_page_size := 10;
    END IF;
    IF p_page_size > 100 THEN
        p_page_size := 100;
    END IF;

    v_search := TRIM(COALESCE(p_search_text, ''));
    v_status := TRIM(COALESCE(p_order_status, ''));

    CREATE TEMP TABLE IF NOT EXISTS tmp_admin_orders (
        rn BIGINT,
        orderid INT,
        ordernumber TEXT,
        orderdate TIMESTAMP,
        totalamount NUMERIC(10,2),
        orderstatus TEXT,
        paymentmethod TEXT,
        paymentstatus TEXT,
        receivername TEXT,
        phone TEXT,
        city TEXT,
        customer_name TEXT,
        customer_email TEXT,
        cancelled BOOLEAN,
        item_count INT,
        first_product_name TEXT,
        first_image_url TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_admin_orders;

    INSERT INTO tmp_admin_orders (
        rn, orderid, ordernumber, orderdate, totalamount, orderstatus,
        paymentmethod, paymentstatus, receivername, phone, city,
        customer_name, customer_email, cancelled, item_count,
        first_product_name, first_image_url
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY o.orderdate DESC, o.id_order DESC) AS rn,
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
        COALESCE(o.receivername, '') AS receivername,
        COALESCE(o.phone, '') AS phone,
        COALESCE(o.city, '') AS city,
        COALESCE(NULLIF(TRIM(u.fullname), ''), COALESCE(u.username, '')) AS customer_name,
        COALESCE(u.email, '') AS customer_email,
        COALESCE(o.cancelled, FALSE) AS cancelled,
        COALESCE((
            SELECT COUNT(*)::INT
            FROM orderitems oi
            WHERE oi.fk_order = o.id_order
        ), 0) AS item_count,
        COALESCE(fi.first_product_name, '') AS first_product_name,
        COALESCE(fi.first_image_url, '') AS first_image_url
    FROM orders o
    LEFT JOIN users u ON u.id_user = o.fk_user
    LEFT JOIN LATERAL (
        SELECT
            oi.productname AS first_product_name,
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
            )) AS first_image_url
        FROM orderitems oi
        WHERE oi.fk_order = o.id_order
        ORDER BY oi.id_orderitem
        LIMIT 1
    ) fi ON TRUE
    WHERE (v_search = ''
           OR COALESCE(o.ordernumber, 'SC' || LPAD(o.id_order::TEXT, 6, '0')) ILIKE '%' || v_search || '%'
           OR COALESCE(o.receivername, '') ILIKE '%' || v_search || '%'
           OR COALESCE(o.phone, '') ILIKE '%' || v_search || '%'
           OR COALESCE(o.city, '') ILIKE '%' || v_search || '%'
           OR COALESCE(NULLIF(TRIM(u.fullname), ''), COALESCE(u.username, '')) ILIKE '%' || v_search || '%'
           OR COALESCE(u.email, '') ILIKE '%' || v_search || '%')
      AND (v_status = ''
           OR CASE WHEN COALESCE(o.cancelled, FALSE) THEN 'Cancelled' ELSE o.orderstatus END = v_status)
      AND (p_from_date IS NULL OR o.orderdate::DATE >= p_from_date)
      AND (p_to_date IS NULL OR o.orderdate::DATE <= p_to_date);

    SELECT COUNT(*) INTO v_total_count FROM tmp_admin_orders;

    OPEN p_result FOR
        SELECT
            orderid,
            ordernumber,
            orderdate,
            totalamount,
            orderstatus,
            paymentmethod,
            paymentstatus,
            receivername,
            phone,
            city,
            customer_name,
            cancelled,
            item_count,
            first_product_name,
            first_image_url,
            (NOT cancelled AND orderstatus IN ('Placed', 'Pending')) AS can_confirm,
            (NOT cancelled AND orderstatus = 'Confirmed') AS can_update_status,
            (NOT cancelled AND orderstatus IN ('Confirmed', 'Shipped')) AS can_deliver,
            (NOT cancelled AND orderstatus IN ('Placed', 'Pending', 'Confirmed')) AS can_cancel
        FROM tmp_admin_orders
        WHERE rn BETWEEN ((p_page_index - 1) * p_page_size + 1)
                      AND (p_page_index * p_page_size)
        ORDER BY rn;

    OPEN p_meta FOR
        SELECT v_total_count AS total_count,
               p_page_size AS page_size,
               p_page_index AS page_index;
END;
$$;
