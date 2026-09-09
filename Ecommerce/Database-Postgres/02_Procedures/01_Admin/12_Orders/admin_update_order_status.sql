/**********************************************************************
Stored Procedure : admin_update_order_status
Source           : AdminUpdateOrderStatus (SQL Server)
Created By       : Muhammed Faris
Created On       : 09/09/2026

PURPOSE
  Set order to Shipped after Confirm and before Deliver.
**********************************************************************/
CREATE OR REPLACE PROCEDURE admin_update_order_status(
    IN p_order_id INT,
    IN p_order_status TEXT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_current_status TEXT;
    v_is_cancelled BOOLEAN;
    v_next_status TEXT := TRIM(COALESCE(p_order_status, ''));
    v_rows INT;
BEGIN
    SELECT o.order_status, COALESCE(o.cancelled, FALSE)
    INTO v_current_status, v_is_cancelled
    FROM orders o
    WHERE o.order_id = p_order_id;

    IF v_current_status IS NULL THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Order was not found.' AS response_msg;
        RETURN;
    END IF;

    IF v_is_cancelled OR v_current_status = 'Cancelled' THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'This order is cancelled.' AS response_msg;
        RETURN;
    END IF;

    IF v_next_status <> 'Shipped' THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'Only Shipped can be set from here. Use Confirm or Deliver for the other steps.' AS response_msg;
        RETURN;
    END IF;

    IF v_current_status <> 'Confirmed' THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'Confirm the order before setting it as Shipped.' AS response_msg;
        RETURN;
    END IF;

    BEGIN
        UPDATE orders
        SET order_status = 'Shipped'
        WHERE order_id = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE
          AND order_status = 'Confirmed';

        GET DIAGNOSTICS v_rows = ROW_COUNT;

        IF v_rows = 0 THEN
            RAISE EXCEPTION 'Order status could not be updated.' USING ERRCODE = 'P0001';
        END IF;

        UPDATE shipping
        SET shipping_status = 'Shipped',
            shipping_date = NOW()
        WHERE order_id = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        OPEN p_result FOR
            SELECT p_order_id AS response_code, TRUE AS status_code,
                   'Order status updated to Shipped.' AS response_msg;
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
