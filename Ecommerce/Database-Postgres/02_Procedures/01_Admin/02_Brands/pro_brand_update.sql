/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Insert / Update Brand Master with Validation
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_brand_update(
    IN p_user_action INT,                  -- 1 = Add, 2 = Edit
    IN p_brand_id INT DEFAULT 0,
    IN p_brand_name TEXT,
    IN p_enter_by INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_brand_id INT := COALESCE(p_brand_id, 0);
    v_is_duplicate INT := 0;
BEGIN
    -------------------------------------------------------------------
    -- REQUIRED FIELD VALIDATION
    -------------------------------------------------------------------
    IF (TRIM(COALESCE(p_brand_name, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Please enter brand name.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (Only against active brands)
    -------------------------------------------------------------------
    SELECT COUNT(*) INTO v_is_duplicate
    FROM brand
    WHERE brand_name = p_brand_name
      AND id_brand <> v_brand_id
      AND cancelled = FALSE;

    IF (v_is_duplicate > 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Brand name "' || p_brand_name || '" already exists.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 1. ADD NEW BRAND
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        INSERT INTO brand (brand_name, cancelled, cancelled_on, cancelled_reason)
        VALUES (p_brand_name, FALSE, NULL, NULL)
        RETURNING id_brand INTO v_brand_id;

        OPEN p_result FOR
            SELECT v_brand_id AS response_code,
                   'Brand created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING BRAND
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM brand WHERE id_brand = v_brand_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Brand ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (SELECT 1 FROM brand WHERE id_brand = v_brand_id AND cancelled = TRUE) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'This brand is deleted and cannot be edited.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE brand
        SET brand_name = p_brand_name
        WHERE id_brand = v_brand_id;

        OPEN p_result FOR
            SELECT v_brand_id AS response_code,
                   'Brand updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
