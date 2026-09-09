/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 15/01/2026
Purpose     : Select Variant Value by ID
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_variant_value_select_by_id(
    IN p_id_variant_value INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM variant_values WHERE id_variant_value = p_id_variant_value
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Variant Value ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM variant_values
        WHERE id_variant_value = p_id_variant_value AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Variant Value is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT
            vv.id_variant_value,
            vv.fk_variant,
            vv.name AS value_name,
            vv.description,
            NULL::TEXT AS value_icon,
            vv.display_order,
            NULL::TIMESTAMP AS created_on,
            vv.cancelled,
            vv.cancelled_on,
            NULL::TEXT AS cancelled_reason,
            v.name AS variant_name
        FROM variant_values vv
        LEFT JOIN variants v ON v.id_variant = vv.fk_variant
        WHERE vv.id_variant_value = p_id_variant_value;
END;
$$;
