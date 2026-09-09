/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Insert / Update / Delete Variant Value
------------------------------------------------------------------------
p_user_action = 1 → INSERT
p_user_action = 2 → UPDATE
p_user_action = 3 → DELETE (Soft Delete)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_variant_value_update(
    IN p_user_action INT,                          -- 1=Insert, 2=Update, 3=Delete
    IN p_id_variant_value INT DEFAULT 0,
    IN p_fk_variant INT DEFAULT 0,
    IN p_value_name TEXT DEFAULT NULL,
    IN p_description TEXT DEFAULT NULL,
    IN p_value_icon TEXT DEFAULT NULL,             -- retained for API parity; not stored in PG schema
    IN p_display_order INT DEFAULT 1,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id_variant_value INT := COALESCE(p_id_variant_value, 0);
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (p_user_action IN (1, 2)) THEN
        IF (COALESCE(p_fk_variant, 0) = 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Variant ID is required.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF (COALESCE(p_value_name, '') = '') THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'ValueName is required.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;
    END IF;

    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF EXISTS (
            SELECT 1 FROM variantvalues
            WHERE fk_variant = p_fk_variant
              AND name = p_value_name
              AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Duplicate Variant Value exists.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        INSERT INTO variantvalues (fk_variant, name, description, displayorder, cancelled, cancelledon)
        VALUES (p_fk_variant, p_value_name, p_description, p_display_order, FALSE, NULL)
        RETURNING id_variantvalue INTO v_id_variant_value;

        OPEN p_result FOR
            SELECT v_id_variant_value AS response_code,
                   'Variant Value added successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (
            SELECT 1 FROM variantvalues
            WHERE id_variantvalue = v_id_variant_value AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Variant Value ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE variantvalues
        SET name = p_value_name,
            description = p_description,
            displayorder = p_display_order
        WHERE id_variantvalue = v_id_variant_value;

        OPEN p_result FOR
            SELECT v_id_variant_value AS response_code,
                   'Variant Value updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DELETE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (p_user_action = 3) THEN
        IF NOT EXISTS (
            SELECT 1 FROM variantvalues WHERE id_variantvalue = v_id_variant_value
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Variant Value ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE variantvalues
        SET cancelled = TRUE,
            cancelledon = v_now
        WHERE id_variantvalue = v_id_variant_value;

        OPEN p_result FOR
            SELECT v_id_variant_value AS response_code,
                   'Variant Value deleted successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
