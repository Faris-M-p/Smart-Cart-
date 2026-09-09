/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Delete category with Validation and Soft Deletion
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_category_delete(
    IN p_category_id INT,
    IN p_cancelled_reason TEXT DEFAULT '',
    IN p_enter_by INT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
BEGIN
    IF NOT EXISTS (SELECT 1 FROM category WHERE id_category = p_category_id) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Category ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM category WHERE id_category = p_category_id AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'This category is already deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM subcategory
        WHERE fk_category = p_category_id AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Cannot delete this category because active subcategories exist.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;

    UPDATE category
    SET cancelled = TRUE,
        cancelledon = v_user_date,
        cancelledreason = p_cancelled_reason
    WHERE id_category = p_category_id;

    OPEN p_result FOR
        SELECT p_category_id AS response_code,
               'Category deleted successfully.' AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
