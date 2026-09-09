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
        SELECT 1 FROM variantvalues WHERE id_variantvalue = p_id_variant_value
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Variant Value ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM variantvalues
        WHERE id_variantvalue = p_id_variant_value AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Variant Value is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT
            vv.id_variantvalue,
            vv.fk_variant,
            vv.name AS value_name,
            vv.description,
            NULL::TEXT AS value_icon,
            vv.displayorder,
            NULL::TIMESTAMP AS createdon,
            vv.cancelled,
            vv.cancelledon,
            NULL::TEXT AS cancelledreason,
            v.name AS variant_name
        FROM variantvalues vv
        LEFT JOIN variants v ON v.id_variant = vv.fk_variant
        WHERE vv.id_variantvalue = p_id_variant_value;
END;
$$;
