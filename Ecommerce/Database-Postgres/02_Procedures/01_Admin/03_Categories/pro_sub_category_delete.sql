/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Delete subcategory with Validation and Soft Deletion
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_sub_category_delete(
    IN p_sub_category_id INT,
    IN p_cancelled_reason TEXT DEFAULT '',
    IN p_enter_by INT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
BEGIN
    IF NOT EXISTS (SELECT 1 FROM subcategory WHERE id_subcategory = p_sub_category_id) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid SubCategory ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM subcategory
        WHERE id_subcategory = p_sub_category_id AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'This subcategory is already deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    /*
    -- Optional product dependency check (commented in SQL Server source)
    IF EXISTS (
        SELECT 1 FROM products
        WHERE fk_subcategory = p_sub_category_id AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Cannot delete this subcategory because products are linked to it.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;
    */

    UPDATE subcategory
    SET cancelled = TRUE,
        cancelledon = v_user_date,
        cancelledreason = p_cancelled_reason
    WHERE id_subcategory = p_sub_category_id;

    OPEN p_result FOR
        SELECT p_sub_category_id AS response_code,
               'SubCategory deleted successfully.' AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
