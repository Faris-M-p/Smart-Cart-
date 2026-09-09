/**********************************************************************
Created By  :  Muhammed Faris
Purpose     :  Insert / Update / Delete Variant Master
-----------------------------------------------------------------------
p_user_action = 1 → INSERT
p_user_action = 2 → UPDATE
p_user_action = 3 → DELETE (Soft Delete)
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_variant_update(
    IN p_user_action INT,                          -- 1=Insert, 2=Update, 3=Delete
    IN p_id_variant INT DEFAULT 0,
    IN p_variant_name TEXT DEFAULT NULL,
    IN p_description TEXT DEFAULT NULL,
    IN p_display_order INT DEFAULT 1,
    IN p_enter_by INT,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id_variant INT := COALESCE(p_id_variant, 0);
BEGIN
    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF (COALESCE(p_variant_name, '') = '') THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'VariantName is required.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1 FROM variants
            WHERE name = p_variant_name AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Variant already exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        INSERT INTO variants (name, description, display_order, is_active, cancelled, cancelled_on)
        VALUES (p_variant_name, p_description, p_display_order, TRUE, FALSE, NULL)
        RETURNING id_variant INTO v_id_variant;

        OPEN p_result FOR
            SELECT v_id_variant AS response_code,
                   'Variant created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (
            SELECT 1 FROM variants
            WHERE id_variant = v_id_variant AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Variant ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE variants
        SET name = p_variant_name,
            description = p_description,
            display_order = p_display_order
        WHERE id_variant = v_id_variant;

        OPEN p_result FOR
            SELECT v_id_variant AS response_code,
                   'Variant updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DELETE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (p_user_action = 3) THEN
        IF NOT EXISTS (SELECT 1 FROM variants WHERE id_variant = v_id_variant) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Variant ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE variants
        SET cancelled = TRUE,
            cancelled_on = v_now
        WHERE id_variant = v_id_variant;

        OPEN p_result FOR
            SELECT v_id_variant AS response_code,
                   'Variant deleted successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
