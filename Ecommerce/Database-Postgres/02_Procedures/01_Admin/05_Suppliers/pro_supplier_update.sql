/**********************************************************************
Stored Procedure : pro_supplier_update
Created By       : Muhammed Faris
Created On       : 12/12/2025

ACTIONS
  1 → Insert
  2 → Update
  3 → Delete (soft delete)
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_supplier_update(
    IN p_user_action INT,                -- 1=Insert, 2=Update, 3=Delete
    IN p_id_supplier BIGINT DEFAULT 0,
    IN p_supplier_name TEXT DEFAULT NULL,
    IN p_contact_person TEXT DEFAULT NULL,
    IN p_phone TEXT DEFAULT NULL,
    IN p_email TEXT DEFAULT NULL,
    IN p_gst_number TEXT DEFAULT NULL,
    IN p_address TEXT DEFAULT NULL,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id_supplier BIGINT := COALESCE(p_id_supplier, 0);
    v_is_duplicate INT := 0;
BEGIN
    IF TRIM(COALESCE(p_supplier_name, '')) = '' THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Supplier Name is required.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    SELECT COUNT(*) INTO v_is_duplicate
    FROM supplier
    WHERE name = p_supplier_name
      AND cancelled = FALSE
      AND id_supplier <> v_id_supplier;

    IF v_is_duplicate > 0 THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Duplicate supplier exists.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 1) THEN
        INSERT INTO supplier (
            name, phone, email, address,
            state, district, city,
            createdat, cancelled
        )
        VALUES (
            p_supplier_name, p_phone, p_email, p_address,
            '', '', '',
            v_now, FALSE
        )
        RETURNING id_supplier INTO v_id_supplier;

        OPEN p_result FOR
            SELECT v_id_supplier AS response_code,
                   'Supplier created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM supplier WHERE id_supplier = v_id_supplier) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Supplier ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE supplier
        SET name = p_supplier_name,
            phone = p_phone,
            email = p_email,
            address = p_address,
            updatedat = v_now
        WHERE id_supplier = v_id_supplier;

        OPEN p_result FOR
            SELECT v_id_supplier AS response_code,
                   'Supplier updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    IF (p_user_action = 3) THEN
        IF NOT EXISTS (SELECT 1 FROM supplier WHERE id_supplier = v_id_supplier) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Supplier ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE supplier
        SET cancelled = TRUE,
            cancelledon = v_now,
            cancelledreason = p_cancelled_reason
        WHERE id_supplier = v_id_supplier;

        OPEN p_result FOR
            SELECT v_id_supplier AS response_code,
                   'Supplier deleted successfully.' AS response_msg,
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
