/* =============================================================================
   Procedure : place_order
   Source    : PlaceOrder (SQL Server)

   PURPOSE
     Place a COD order from the cart or a single Buy Now sku.
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
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please log in to place an order.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_receiver_name, '')) = ''
       OR length(TRIM(p_receiver_name)) < 2 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter the receiver name.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_phone, '')) !~ '^[0-9]{10}$' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter a 10-digit mobile number.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_address_line, '')) = ''
       OR length(TRIM(p_address_line)) < 5 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter the delivery address.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_city, '')) = '' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter the city.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_pincode, '')) !~ '^[0-9]{6}$' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter a 6-digit pincode.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    p_payment_method := upper(TRIM(COALESCE(p_payment_method, '')));
    IF p_payment_method = 'UPI' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'UPI will be available soon in this area. Please choose Cash on Delivery.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF p_payment_method <> 'COD' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please choose Cash on Delivery.'::TEXT AS responsemsg;
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
        productid         INT NOT NULL,
        productvariantid INT NOT NULL,
        name               TEXT NOT NULL,
        label              TEXT NULL,
        sku                TEXT NULL,
        price              NUMERIC(10, 2) NOT NULL,
        quantity           INT NOT NULL,
        linetotal         NUMERIC(10, 2) NOT NULL,
        stock_qty          INT NOT NULL
    ) ON COMMIT DROP;

    IF COALESCE(p_product_variant_id, 0) > 0 THEN
        INSERT INTO tmp_order_lines
        SELECT
            p.id_product,
            pv.id_productvariant,
            p.name,
            CASE
                WHEN COALESCE(pv.variantlabel, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variantlabel
            END,
            COALESCE(pv.sku, ''),
            pv.sellingprice,
            p_quantity,
            pv.sellingprice * p_quantity,
            COALESCE(st.qty, 0)
        FROM productvariants AS pv
        INNER JOIN products AS p ON p.id_product = pv.fk_product
        LEFT JOIN (
            SELECT fk_productvariant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_productvariant
        ) AS st ON st.fk_productvariant = pv.id_productvariant
        WHERE pv.id_productvariant = p_product_variant_id
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
          AND COALESCE(p.cancelled, FALSE) = FALSE
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE;
    ELSE
        INSERT INTO tmp_order_lines
        SELECT
            ci.fk_product,
            ci.fk_productvariant,
            p.name,
            CASE
                WHEN COALESCE(pv.variantlabel, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variantlabel
            END,
            COALESCE(pv.sku, ''),
            COALESCE(pv.sellingprice, ci.price),
            ci.quantity,
            COALESCE(pv.sellingprice, ci.price) * ci.quantity,
            COALESCE(st.qty, 0)
        FROM cartitems AS ci
        INNER JOIN cart AS c ON c.id_cart = ci.fk_cart
        INNER JOIN products AS p ON p.id_product = ci.fk_product
        INNER JOIN productvariants AS pv ON pv.id_productvariant = ci.fk_productvariant
        LEFT JOIN (
            SELECT fk_productvariant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_productvariant
        ) AS st ON st.fk_productvariant = pv.id_productvariant
        WHERE c.fk_user = p_user_id
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND pv.isactive = TRUE
          AND COALESCE(pv.sellonline, FALSE) = TRUE
          AND COALESCE(p.cancelled, FALSE) = FALSE
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM tmp_order_lines) THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'There is nothing to order.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM tmp_order_lines
        WHERE stock_qty < quantity
           OR productvariantid IS NULL
           OR productvariantid < 1
    ) THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'One or more items are out of stock. Update the cart and try again.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT SUM(linetotal) INTO v_total FROM tmp_order_lines;

    INSERT INTO orders (
        fk_user,
        orderdate,
        totalamount,
        orderstatus,
        shippingaddress,
        paymentmethod,
        cancelled,
        receivername,
        phone,
        addressline,
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
    RETURNING id_order INTO v_order_id;

    UPDATE orders
    SET ordernumber = 'SC' || lpad(v_order_id::TEXT, 6, '0')
    WHERE id_order = v_order_id;

    INSERT INTO orderitems (
        fk_order,
        fk_product,
        fk_productvariant,
        productname,
        variantlabel,
        sku,
        unitprice,
        quantity,
        linetotal
    )
    SELECT
        v_order_id,
        productid,
        productvariantid,
        name,
        label,
        sku,
        price,
        quantity,
        linetotal
    FROM tmp_order_lines;

    FOR r IN
        SELECT productvariantid, quantity
        FROM tmp_order_lines
    LOOP
        v_need := r.quantity;

        WHILE v_need > 0 LOOP
            v_stock_id := NULL;
            v_have := 0;

            SELECT s.id_stock, s.quantity
            INTO v_stock_id, v_have
            FROM stock AS s
            WHERE s.fk_productvariant = r.productvariantid
              AND COALESCE(s.cancelled, FALSE) = FALSE
              AND s.quantity > 0
            ORDER BY s.createdon ASC, s.id_stock ASC
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
        -- Undo in-procedure writes (plpgsql does not accept ROLLBACK TO SAVEPOINT).
        DELETE FROM orderitems WHERE fk_order = v_order_id;
        DELETE FROM orders WHERE id_order = v_order_id;
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Stock changed while placing the order. Please try again.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    INSERT INTO payments (
        fk_order,
        paymentdate,
        paymentamount,
        paymentstatus,
        paymentmethod,
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
        fk_order,
        shippingaddress,
        shippingstatus,
        cancelled
    )
    VALUES (
        v_order_id,
        v_full_address,
        'Pending',
        FALSE
    );

    IF COALESCE(p_product_variant_id, 0) < 1 THEN
        DELETE FROM cartitems AS ci
        USING cart AS c
        WHERE c.id_cart = ci.fk_cart
          AND c.fk_user = p_user_id;
    END IF;

    OPEN p_result FOR
    SELECT v_order_id AS responsecode, 1 AS statuscode,
           'Order placed. Pay cash on delivery.'::TEXT AS responsemsg;
END;
$$;
