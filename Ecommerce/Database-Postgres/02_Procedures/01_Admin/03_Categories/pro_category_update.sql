/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Insert / Update category Master with Validation
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_category_update(
    IN p_user_action INT,                  -- 1 = Add, 2 = Edit
    IN p_category_id INT DEFAULT 0,
    IN p_category_name TEXT DEFAULT NULL,
    IN p_description TEXT DEFAULT '',
    IN p_enter_by INT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_category_id INT := COALESCE(p_category_id, 0);
    v_is_duplicate INT := 0;
BEGIN
    IF (TRIM(COALESCE(p_category_name, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Please enter category name.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    SELECT COUNT(*) INTO v_is_duplicate
    FROM category
    WHERE name = p_category_name
      AND id_category <> v_category_id
      AND cancelled = FALSE;

    IF (v_is_duplicate > 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Category name "' || p_category_name || '" already exists.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 1) THEN
        INSERT INTO category (name, description, isactive, cancelled, cancelledon, cancelledreason)
        VALUES (p_category_name, p_description, TRUE, FALSE, NULL, NULL)
        RETURNING id_category INTO v_category_id;

        OPEN p_result FOR
            SELECT v_category_id AS response_code,
                   'Category created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM category WHERE id_category = v_category_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Category ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (SELECT 1 FROM category WHERE id_category = v_category_id AND cancelled = TRUE) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'This category is deleted and cannot be edited.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE category
        SET name = p_category_name,
            description = p_description
        WHERE id_category = v_category_id;

        OPEN p_result FOR
            SELECT v_category_id AS response_code,
                   'Category updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
