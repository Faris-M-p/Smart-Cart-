/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 09/12/2025
Purpose     : Insert / Update / Delete Product Image (product_media)
Source      : ProProductImageUpdate (SQL Server)
------------------------------------------------------------------------
p_user_action = 1 → Add
p_user_action = 2 → Update
p_user_action = 3 → Delete (hard delete; product_media has no cancelled)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_product_image_update(
    IN p_user_action INT,
    IN p_id_product_image INT DEFAULT 0,
    IN p_fk_product INT DEFAULT 0,
    IN p_image TEXT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    IN p_enter_by INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
    v_id INT := COALESCE(p_id_product_image, 0);
    v_display_order INT;
BEGIN
    -------------------------------------------------------------------
    -- 1. ADD NEW IMAGE
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF (COALESCE(p_fk_product, 0) = 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Product ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF (COALESCE(p_image, '') = '') THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Image cannot be empty.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM products WHERE id_product = p_fk_product AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Product ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        SELECT COALESCE(MAX(display_order), -1) + 1
        INTO v_display_order
        FROM product_media
        WHERE fk_product = p_fk_product;

        INSERT INTO product_media (
            fk_product, media_type, media_url, display_order, is_primary, created_at, updated_at
        )
        VALUES (
            p_fk_product, 'Image', p_image, v_display_order, FALSE, v_user_date, NULL
        )
        RETURNING id_product_media INTO v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Product image added successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING IMAGE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM product_media WHERE id_product_media = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'Invalid or deleted Product Image ID.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE product_media
        SET media_url = p_image,
            updated_at = v_user_date
        WHERE id_product_media = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Product image updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- 3. DELETE IMAGE (hard delete — no soft-cancel columns)
    -------------------------------------------------------------------
    IF (p_user_action = 3) THEN
        IF NOT EXISTS (SELECT 1 FROM product_media WHERE id_product_media = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Product Image ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        DELETE FROM product_media WHERE id_product_media = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Product image deleted successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT -1 AS response_code, 'Invalid UserAction.' AS response_msg, FALSE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
