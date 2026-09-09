/* =============================================================================
   Procedure : cancel_order
   Source    : CancelOrder (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE cancel_order(
    p_user_id      INT,
    p_order_id     INT,
    p_reason       TEXT DEFAULT NULL,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_status    TEXT;
    v_is_cancelled    BOOLEAN;
    v_shipping_status TEXT;
    v_cancel_reason   TEXT;
    v_rowcount        INT;
    r                 RECORD;
    v_stock_id        INT;
BEGIN
    v_cancel_reason := NULLIF(TRIM(COALESCE(p_reason, '')), '');

    IF v_cancel_reason IS NULL THEN
        v_cancel_reason := 'Cancelled by customer';
    END IF;

    SELECT
        o.order_status,
        COALESCE(o.cancelled, FALSE)
    INTO
        v_order_status,
        v_is_cancelled
    FROM orders AS o
    WHERE o.order_id = p_order_id
      AND o.user_id = p_user_id;

    IF v_order_status IS NULL THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Order was not found.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF v_is_cancelled = TRUE THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This order is already cancelled.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF v_order_status NOT IN ('Placed', 'Pending') THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This order can no longer be cancelled.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT s.shipping_status
    INTO v_shipping_status
    FROM shipping AS s
    WHERE s.order_id = p_order_id
      AND COALESCE(s.cancelled, FALSE) = FALSE
    ORDER BY s.shipping_id DESC
    LIMIT 1;

    IF v_shipping_status IN ('Shipped', 'Out for delivery', 'Delivered', 'In Transit') THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This order can no longer be cancelled.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SAVEPOINT sp_cancel_order;

    FOR r IN
        SELECT oi.fk_product_variant AS variant_id, oi.quantity AS qty
        FROM order_items AS oi
        WHERE oi.fk_order = p_order_id
          AND oi.fk_product_variant IS NOT NULL
          AND oi.fk_product_variant > 0
    LOOP
        v_stock_id := NULL;

        SELECT s.id_stock
        INTO v_stock_id
        FROM stock AS s
        WHERE s.fk_product_variant = r.variant_id
          AND COALESCE(s.cancelled, FALSE) = FALSE
        ORDER BY s.created_on DESC, s.id_stock DESC
        LIMIT 1
        FOR UPDATE;

        IF v_stock_id IS NOT NULL THEN
            UPDATE stock
            SET quantity = quantity + r.qty
            WHERE id_stock = v_stock_id;
        END IF;
    END LOOP;

    UPDATE payments
    SET cancelled = TRUE,
        cancelled_on = NOW(),
        cancelled_reason = v_cancel_reason,
        payment_status = 'Cancelled'
    WHERE order_id = p_order_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    UPDATE shipping
    SET cancelled = TRUE,
        cancelled_on = NOW(),
        cancelled_reason = v_cancel_reason,
        shipping_status = 'Cancelled'
    WHERE order_id = p_order_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    UPDATE orders
    SET cancelled = TRUE,
        cancelled_on = NOW(),
        cancelled_reason = v_cancel_reason,
        order_status = 'Cancelled'
    WHERE order_id = p_order_id
      AND user_id = p_user_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        ROLLBACK TO SAVEPOINT sp_cancel_order;
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Order was not found.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    RELEASE SAVEPOINT sp_cancel_order;

    OPEN p_result FOR
    SELECT p_order_id AS "ResponseCode", 1 AS "StatusCode",
           'Order cancelled.'::TEXT AS "ResponseMsg";
END;
$$;
