/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Soft-delete Product
Source      : ProProductDelete (SQL Server)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_product_delete(
    IN p_id_product INT,
    IN p_cancelled_reason TEXT DEFAULT '',
    IN p_enter_by INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
BEGIN
    IF NOT EXISTS (SELECT 1 FROM products WHERE id_product = p_id_product) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Product ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM products WHERE id_product = p_id_product AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Product already deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -- products has cancelled / cancelled_on only (no cancelled_reason / cancelled_by)
    UPDATE products
    SET cancelled = TRUE,
        cancelled_on = v_user_date,
        is_active = FALSE
    WHERE id_product = p_id_product;

    OPEN p_result FOR
        SELECT p_id_product AS response_code,
               'Product deleted successfully.' AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
