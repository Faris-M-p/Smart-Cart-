/* =============================================================================
   Procedure : place_order
   Source    : PlaceOrder (SQL Server)

   PURPOSE
     Place a COD order from the cart or a single Buy Now SKU.
     UPI is reserved for a later payment step.
   ============================================================================= */

CREATE OR REPLACE PROCEDURE place_order(
    p_user_id            INT,
    p_product_variant_id INT DEFAULT 0,
    p_quantity           INT DEFAULT 1,
    p_receiver_name      TEXT DEFAULT NULL,
    p_phone              TEXT DEFAULT NULL,
    p_address_line       TEXT DEFAULT NULL,
    p_city               TEXT DEFAULT NULL,
    p_pincode            TEXT DEFAULT NULL,
    p_payment_method     TEXT DEFAULT NULL,
    INOUT p_result       refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_full_address TEXT;
    v_total        NUMERIC(10, 2);
    v_order_id     INT;
    r              RECORD;
    v_need         INT;
    v_stock_id     INT;
    v_have         INT;
    v_stock_fail   BOOLEAN := FALSE;
BEGIN
    IF COALESCE(p_user_id, 0) < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please log in to place an order.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_receiver_name, '')) = ''
       OR length(TRIM(p_receiver_name)) < 2 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please enter the receiver name.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_phone, '')) !~ '^[0-9]{10}$' THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please enter a 10-digit mobile number.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_address_line, '')) = ''
       OR length(TRIM(p_address_line)) < 5 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please enter the delivery address.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_city, '')) = '' THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please enter the city.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_pincode, '')) !~ '^[0-9]{6}$' THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please enter a 6-digit pincode.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    p_payment_method := upper(TRIM(COALESCE(p_payment_method, '')));
    IF p_payment_method = 'UPI' THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'UPI will be available soon in this area. Please choose Cash on Delivery.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF p_payment_method <> 'COD' THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please choose Cash on Delivery.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF COALESCE(p_quantity, 0) < 1 THEN
        p_quantity := 1;
    END IF;

    v_full_address :=
        TRIM(p_receiver_name) || ', ' ||
        TRIM(p_phone) || ', ' ||
        TRIM(p_address_line) || ', ' ||
        TRIM(p_city) || ' - ' ||
        TRIM(p_pincode);

    DROP TABLE IF EXISTS tmp_order_lines;
    CREATE TEMP TABLE tmp_order_lines (
        product_id         INT NOT NULL,
        product_variant_id INT NOT NULL,
        name               TEXT NOT NULL,
        label              TEXT NULL,
        sku                TEXT NULL,
        price              NUMERIC(10, 2) NOT NULL,
        quantity           INT NOT NULL,
        line_total         NUMERIC(10, 2) NOT NULL,
        stock_qty          INT NOT NULL
    ) ON COMMIT DROP;

    IF COALESCE(p_product_variant_id, 0) > 0 THEN
        INSERT INTO tmp_order_lines
        SELECT
            p.id_product,
            pv.id_product_variant,
            p.name,
            CASE
                WHEN COALESCE(pv.variant_label, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variant_label
            END,
            COALESCE(pv.sku, ''),
            pv.selling_price,
            p_quantity,
            pv.selling_price * p_quantity,
            COALESCE(st.qty, 0)
        FROM product_variants AS pv
        INNER JOIN products AS p ON p.id_product = pv.fk_product
        LEFT JOIN (
            SELECT fk_product_variant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_product_variant
        ) AS st ON st.fk_product_variant = pv.id_product_variant
        WHERE pv.id_product_variant = p_product_variant_id
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
          AND COALESCE(p.cancelled, FALSE) = FALSE
          AND p.is_active = TRUE
          AND COALESCE(p.sell_online, FALSE) = TRUE;
    ELSE
        INSERT INTO tmp_order_lines
        SELECT
            ci.product_id,
            ci.product_variant_id,
            p.name,
            CASE
                WHEN COALESCE(pv.variant_label, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variant_label
            END,
            COALESCE(pv.sku, ''),
            COALESCE(pv.selling_price, ci.price),
            ci.quantity,
            COALESCE(pv.selling_price, ci.price) * ci.quantity,
            COALESCE(st.qty, 0)
        FROM cart_items AS ci
        INNER JOIN cart AS c ON c.cart_id = ci.cart_id
        INNER JOIN products AS p ON p.id_product = ci.product_id
        INNER JOIN product_variants AS pv ON pv.id_product_variant = ci.product_variant_id
        LEFT JOIN (
            SELECT fk_product_variant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_product_variant
        ) AS st ON st.fk_product_variant = pv.id_product_variant
        WHERE c.user_id = p_user_id
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.is_active = TRUE
          AND COALESCE(pv.sell_online, FALSE) = TRUE
          AND COALESCE(p.cancelled, FALSE) = FALSE
          AND p.is_active = TRUE
          AND COALESCE(p.sell_online, FALSE) = TRUE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM tmp_order_lines) THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'There is nothing to order.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM tmp_order_lines
        WHERE stock_qty < quantity
           OR product_variant_id IS NULL
           OR product_variant_id < 1
    ) THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'One or more items are out of stock. Update the cart and try again.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT SUM(line_total) INTO v_total FROM tmp_order_lines;

    SAVEPOINT sp_place_order;

    INSERT INTO orders (
        user_id,
        order_date,
        total_amount,
        order_status,
        shipping_address,
        payment_method,
        cancelled,
        receiver_name,
        phone,
        address_line,
        city,
        pincode
    )
    VALUES (
        p_user_id,
        NOW(),
        v_total,
        'Placed',
        v_full_address,
        'COD',
        FALSE,
        TRIM(p_receiver_name),
        TRIM(p_phone),
        TRIM(p_address_line),
        TRIM(p_city),
        TRIM(p_pincode)
    )
    RETURNING order_id INTO v_order_id;

    UPDATE orders
    SET order_number = 'SC' || lpad(v_order_id::TEXT, 6, '0')
    WHERE order_id = v_order_id;

    INSERT INTO order_items (
        fk_order,
        fk_product,
        fk_product_variant,
        product_name,
        variant_label,
        sku,
        unit_price,
        quantity,
        line_total
    )
    SELECT
        v_order_id,
        product_id,
        product_variant_id,
        name,
        label,
        sku,
        price,
        quantity,
        line_total
    FROM tmp_order_lines;

    FOR r IN
        SELECT product_variant_id, quantity
        FROM tmp_order_lines
    LOOP
        v_need := r.quantity;

        WHILE v_need > 0 LOOP
            v_stock_id := NULL;
            v_have := 0;

            SELECT s.id_stock, s.quantity
            INTO v_stock_id, v_have
            FROM stock AS s
            WHERE s.fk_product_variant = r.product_variant_id
              AND COALESCE(s.cancelled, FALSE) = FALSE
              AND s.quantity > 0
            ORDER BY s.created_on ASC, s.id_stock ASC
            LIMIT 1
            FOR UPDATE;

            IF v_stock_id IS NULL THEN
                v_stock_fail := TRUE;
                EXIT;
            END IF;

            IF v_have >= v_need THEN
                UPDATE stock
                SET quantity = quantity - v_need
                WHERE id_stock = v_stock_id;
                v_need := 0;
            ELSE
                UPDATE stock
                SET quantity = 0
                WHERE id_stock = v_stock_id;
                v_need := v_need - v_have;
            END IF;
        END LOOP;

        IF v_stock_fail THEN
            EXIT;
        END IF;
    END LOOP;

    IF v_stock_fail THEN
        ROLLBACK TO SAVEPOINT sp_place_order;
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Stock changed while placing the order. Please try again.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    INSERT INTO payments (
        order_id,
        payment_date,
        payment_amount,
        payment_status,
        payment_method,
        cancelled
    )
    VALUES (
        v_order_id,
        NOW(),
        v_total,
        'Pending',
        'COD',
        FALSE
    );

    INSERT INTO shipping (
        order_id,
        shipping_address,
        shipping_status,
        cancelled
    )
    VALUES (
        v_order_id,
        v_full_address,
        'Pending',
        FALSE
    );

    IF COALESCE(p_product_variant_id, 0) < 1 THEN
        DELETE FROM cart_items AS ci
        USING cart AS c
        WHERE c.cart_id = ci.cart_id
          AND c.user_id = p_user_id;
    END IF;

    RELEASE SAVEPOINT sp_place_order;

    OPEN p_result FOR
    SELECT v_order_id AS "ResponseCode", 1 AS "StatusCode",
           'Order placed. Pay cash on delivery.'::TEXT AS "ResponseMsg";
END;
$$;
