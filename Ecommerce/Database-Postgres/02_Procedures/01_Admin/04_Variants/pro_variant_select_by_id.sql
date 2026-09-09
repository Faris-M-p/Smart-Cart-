/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 15/01/2026
Purpose     : Select Variant by ID
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_variant_select_by_id(
    IN p_id_variant INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM variants WHERE id_variant = p_id_variant) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Variant ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM variants WHERE id_variant = p_id_variant AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Variant is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT
            id_variant,
            name AS variant_name,
            description,
            displayorder,
            NULL::TIMESTAMP AS createdon,
            cancelled,
            cancelledon,
            NULL::TEXT AS cancelledreason
        FROM variants
        WHERE id_variant = p_id_variant;
END;
$$;
