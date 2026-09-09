/**********************************************************************
Stored Procedure : admin_confirm_order
Source           : AdminConfirmOrder (SQL Server)
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Accept a placed/pending order so it can be packed and delivered.
**********************************************************************/
CREATE OR REPLACE PROCEDURE admin_confirm_order(
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
    SELECT o.orderstatus, COALESCE(o.cancelled, FALSE)
    INTO v_order_status, v_is_cancelled
    FROM orders o
    WHERE o.id_order = p_order_id;

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

    IF v_order_status NOT IN ('Placed', 'Pending') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'Only placed orders can be confirmed.' AS response_msg;
        RETURN;
    END IF;

    UPDATE orders
    SET orderstatus = 'Confirmed'
    WHERE id_order = p_order_id
      AND COALESCE(cancelled, FALSE) = FALSE
      AND orderstatus IN ('Placed', 'Pending');

    GET DIAGNOSTICS v_rows = ROW_COUNT;

    IF v_rows = 0 THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'Order could not be confirmed.' AS response_msg;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT p_order_id AS response_code, TRUE AS status_code, 'Order confirmed.' AS response_msg;
END;
$$;
