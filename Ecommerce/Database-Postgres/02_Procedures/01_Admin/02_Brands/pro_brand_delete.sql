/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Delete Brand with Validation and Soft Deletion
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_brand_delete(
    IN p_brand_id INT,
    IN p_cancelled_reason TEXT DEFAULT '',
    IN p_enter_by INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
BEGIN
    -------------------------------------------------------------------
    -- 1. CHECK IF BRAND EXISTS
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM brand WHERE id_brand = p_brand_id) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Brand ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 2. ALREADY CANCELLED CHECK
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM brand WHERE id_brand = p_brand_id AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'This brand is already deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 3. CHECK PRODUCT DEPENDENCY
    -------------------------------------------------------------------
    IF EXISTS (
        SELECT 1 FROM products
        WHERE fk_brand = p_brand_id AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Cannot delete this brand because active products exist.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 4. SOFT DELETE BRAND
    -------------------------------------------------------------------
    UPDATE brand
    SET cancelled = TRUE,
        cancelled_on = v_user_date,
        cancelled_reason = p_cancelled_reason
    WHERE id_brand = p_brand_id;

    OPEN p_result FOR
        SELECT p_brand_id AS response_code,
               'Brand deleted successfully.' AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
