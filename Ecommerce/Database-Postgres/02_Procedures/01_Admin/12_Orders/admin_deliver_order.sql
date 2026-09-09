/**********************************************************************
Stored Procedure : admin_deliver_order
Source           : AdminDeliverOrder (SQL Server)
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Mark a confirmed/shipped order delivered and collect COD payment.
**********************************************************************/
CREATE OR REPLACE PROCEDURE admin_deliver_order(
    IN p_order_id INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_status TEXT;
    v_is_cancelled BOOLEAN;
    v_rows INT;
BEGIN
    SELECT o.order_status, COALESCE(o.cancelled, FALSE)
    INTO v_order_status, v_is_cancelled
    FROM orders o
    WHERE o.order_id = p_order_id;

    IF v_order_status IS NULL THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Order was not found.' AS response_msg;
        RETURN;
    END IF;

    IF v_is_cancelled OR v_order_status = 'Cancelled' THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'This order is cancelled.' AS response_msg;
        RETURN;
    END IF;

    IF v_order_status NOT IN ('Confirmed', 'Shipped') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'Confirm the order before marking it delivered.' AS response_msg;
        RETURN;
    END IF;

    BEGIN
        UPDATE orders
        SET order_status = 'Delivered'
        WHERE order_id = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE
          AND order_status IN ('Confirmed', 'Shipped');

        GET DIAGNOSTICS v_rows = ROW_COUNT;

        IF v_rows = 0 THEN
            RAISE EXCEPTION 'Order could not be delivered.' USING ERRCODE = 'P0001';
        END IF;

        UPDATE payments
        SET payment_status = 'Collected',
            payment_date = NOW()
        WHERE order_id = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        UPDATE shipping
        SET shipping_status = 'Delivered',
            shipping_date = NOW()
        WHERE order_id = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        OPEN p_result FOR
            SELECT p_order_id AS response_code, TRUE AS status_code,
                   'Order delivered. COD collected.' AS response_msg;
    EXCEPTION
        WHEN SQLSTATE 'P0001' THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, FALSE AS status_code, SQLERRM AS response_msg;
        WHEN OTHERS THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, FALSE AS status_code, SQLERRM AS response_msg;
    END;
END;
$$;
