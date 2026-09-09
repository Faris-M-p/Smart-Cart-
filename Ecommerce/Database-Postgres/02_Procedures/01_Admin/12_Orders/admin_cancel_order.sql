/**********************************************************************
Stored Procedure : admin_cancel_order
Source           : AdminCancelOrder (SQL Server)
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Soft-cancel a placed/pending/confirmed order and restore stock.
**********************************************************************/
CREATE OR REPLACE PROCEDURE admin_cancel_order(
    IN p_order_id INT,
    IN p_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_status TEXT;
    v_is_cancelled BOOLEAN;
    v_cancel_reason TEXT;
    v_variant_id INT;
    v_qty INT;
    v_stock_id INT;
    v_rows INT;
    r_item RECORD;
BEGIN
    v_cancel_reason := NULLIF(TRIM(COALESCE(p_reason, '')), '');
    IF v_cancel_reason IS NULL THEN
        v_cancel_reason := 'Cancelled by admin';
    END IF;

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
            SELECT -1 AS response_code, FALSE AS status_code,
                   'This order is already cancelled.' AS response_msg;
        RETURN;
    END IF;

    IF v_order_status NOT IN ('Placed', 'Pending', 'Confirmed') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code,
                   'This order can no longer be cancelled.' AS response_msg;
        RETURN;
    END IF;

    BEGIN
        FOR r_item IN
            SELECT oi.fk_productvariant, oi.quantity
            FROM orderitems oi
            WHERE oi.fk_order = p_order_id
              AND oi.fk_productvariant IS NOT NULL
              AND oi.fk_productvariant > 0
        LOOP
            v_variant_id := r_item.fk_productvariant;
            v_qty := r_item.quantity;
            v_stock_id := NULL;

            SELECT s.id_stock
            INTO v_stock_id
            FROM stock s
            WHERE s.fk_productvariant = v_variant_id
              AND COALESCE(s.cancelled, FALSE) = FALSE
            ORDER BY s.createdon DESC, s.id_stock DESC
            LIMIT 1
            FOR UPDATE;

            IF v_stock_id IS NOT NULL THEN
                UPDATE stock
                SET quantity = quantity + v_qty
                WHERE id_stock = v_stock_id;
            END IF;
        END LOOP;

        UPDATE payments
        SET cancelled = TRUE,
            cancelledon = NOW(),
            cancelledreason = v_cancel_reason,
            paymentstatus = 'Cancelled'
        WHERE fk_order = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        UPDATE shipping
        SET cancelled = TRUE,
            cancelledon = NOW(),
            cancelledreason = v_cancel_reason,
            shippingstatus = 'Cancelled'
        WHERE fk_order = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        UPDATE orders
        SET cancelled = TRUE,
            cancelledon = NOW(),
            cancelledreason = v_cancel_reason,
            orderstatus = 'Cancelled'
        WHERE id_order = p_order_id
          AND COALESCE(cancelled, FALSE) = FALSE;

        GET DIAGNOSTICS v_rows = ROW_COUNT;

        IF v_rows = 0 THEN
            RAISE EXCEPTION 'Order was not found.' USING ERRCODE = 'P0001';
        END IF;

        OPEN p_result FOR
            SELECT p_order_id AS response_code, TRUE AS status_code, 'Order cancelled.' AS response_msg;
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
