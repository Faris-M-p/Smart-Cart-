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
        order_id INT,
        order_number TEXT,
        order_date TIMESTAMP,
        total_amount NUMERIC(10,2),
        order_status TEXT,
        payment_method TEXT,
        payment_status TEXT,
        receiver_name TEXT,
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
        rn, order_id, order_number, order_date, total_amount, order_status,
        payment_method, payment_status, receiver_name, phone, city,
        customer_name, customer_email, cancelled, item_count,
        first_product_name, first_image_url
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY o.order_date DESC, o.order_id DESC) AS rn,
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
        COALESCE(o.receiver_name, '') AS receiver_name,
        COALESCE(o.phone, '') AS phone,
        COALESCE(o.city, '') AS city,
        COALESCE(NULLIF(TRIM(u.full_name), ''), COALESCE(u.user_name, '')) AS customer_name,
        COALESCE(u.email, '') AS customer_email,
        COALESCE(o.cancelled, FALSE) AS cancelled,
        COALESCE((
            SELECT COUNT(*)::INT
            FROM order_items oi
            WHERE oi.fk_order = o.order_id
        ), 0) AS item_count,
        COALESCE(fi.first_product_name, '') AS first_product_name,
        COALESCE(fi.first_image_url, '') AS first_image_url
    FROM orders o
    LEFT JOIN users u ON u.user_id = o.user_id
    LEFT JOIN LATERAL (
        SELECT
            oi.product_name AS first_product_name,
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
            )) AS first_image_url
        FROM order_items oi
        WHERE oi.fk_order = o.order_id
        ORDER BY oi.id_order_item
        LIMIT 1
    ) fi ON TRUE
    WHERE (v_search = ''
           OR COALESCE(o.order_number, 'SC' || LPAD(o.order_id::TEXT, 6, '0')) ILIKE '%' || v_search || '%'
           OR COALESCE(o.receiver_name, '') ILIKE '%' || v_search || '%'
           OR COALESCE(o.phone, '') ILIKE '%' || v_search || '%'
           OR COALESCE(o.city, '') ILIKE '%' || v_search || '%'
           OR COALESCE(NULLIF(TRIM(u.full_name), ''), COALESCE(u.user_name, '')) ILIKE '%' || v_search || '%'
           OR COALESCE(u.email, '') ILIKE '%' || v_search || '%')
      AND (v_status = ''
           OR CASE WHEN COALESCE(o.cancelled, FALSE) THEN 'Cancelled' ELSE o.order_status END = v_status)
      AND (p_from_date IS NULL OR o.order_date::DATE >= p_from_date)
      AND (p_to_date IS NULL OR o.order_date::DATE <= p_to_date);

    SELECT COUNT(*) INTO v_total_count FROM tmp_admin_orders;

    OPEN p_result FOR
        SELECT
            order_id,
            order_number,
            order_date,
            total_amount,
            order_status,
            payment_method,
            payment_status,
            receiver_name,
            phone,
            city,
            customer_name,
            cancelled,
            item_count,
            first_product_name,
            first_image_url,
            (NOT cancelled AND order_status IN ('Placed', 'Pending')) AS can_confirm,
            (NOT cancelled AND order_status = 'Confirmed') AS can_update_status,
            (NOT cancelled AND order_status IN ('Confirmed', 'Shipped')) AS can_deliver,
            (NOT cancelled AND order_status IN ('Placed', 'Pending', 'Confirmed')) AS can_cancel
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
