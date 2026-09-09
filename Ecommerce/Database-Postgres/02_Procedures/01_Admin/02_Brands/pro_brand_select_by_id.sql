/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select brand By ID for Admin Edit
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_brand_select_by_id(
    IN p_brand_id INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM brand WHERE id_brand = p_brand_id) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Brand ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM brand WHERE id_brand = p_brand_id AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Brand is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- RESULT: brand INFO
    -------------------------------------------------------------------
    OPEN p_result FOR
        SELECT
            b.id_brand AS brand_id,
            b.brandname,
            b.cancelled,
            b.cancelledon,
            b.cancelledreason
        FROM brand b
        WHERE b.id_brand = p_brand_id;
END;
$$;
