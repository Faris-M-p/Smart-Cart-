/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select Supplier By ID for Admin Edit
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_supplier_select_by_id(
    IN p_id_supplier BIGINT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM supplier WHERE id_supplier = p_id_supplier) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Supplier ID.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM supplier WHERE id_supplier = p_id_supplier AND cancelled = TRUE) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Supplier is deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT
            s.id_supplier,
            s.name AS supplier_name,
            NULL::TEXT AS contact_person,
            s.phone,
            s.email,
            NULL::TEXT AS gst_number,
            s.address,
            s.created_at AS created_on,
            s.cancelled,
            s.cancelled_on,
            s.cancelled_reason
        FROM supplier s
        WHERE s.id_supplier = p_id_supplier;
END;
$$;
