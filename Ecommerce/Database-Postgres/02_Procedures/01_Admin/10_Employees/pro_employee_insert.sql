/**********************************************************************
Stored Procedure : pro_employee_insert
Created By       : Muhammed Faris
Created On       : 06/09/2026

INPUT
  p_password_hash must already be hashed by the application.
  Do not pass a plain-text password.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_employee_insert(
    IN p_full_name TEXT,
    IN p_user_name TEXT,
    IN p_password_hash TEXT,
    IN p_fk_user_role INT,
    IN p_is_active BOOLEAN DEFAULT TRUE,
    IN p_email TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_id_admin_user INT;
BEGIN
    IF (TRIM(COALESCE(p_full_name, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Please enter employee name.' AS response_msg;
        RETURN;
    END IF;

    IF (TRIM(COALESCE(p_user_name, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Please enter username.' AS response_msg;
        RETURN;
    END IF;

    IF (TRIM(COALESCE(p_password_hash, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Please enter password.' AS response_msg;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM userroles
        WHERE id_userrole = p_fk_user_role AND cancelled = FALSE AND isactive = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Please select a valid active user role.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM adminusers
        WHERE LOWER(username) = LOWER(TRIM(p_user_name))
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Username already exists.' AS response_msg;
        RETURN;
    END IF;

    INSERT INTO adminusers (
        fk_userrole, username, passwordhash, fullname, email,
        isactive, createdat, cancelled
    )
    VALUES (
        p_fk_user_role,
        TRIM(p_user_name),
        p_password_hash,
        TRIM(p_full_name),
        COALESCE(p_email, LOWER(TRIM(p_user_name)) || '@smartcart.local'),
        COALESCE(p_is_active, TRUE),
        NOW(),
        FALSE
    )
    RETURNING id_adminuser INTO v_id_admin_user;

    OPEN p_result FOR
        SELECT v_id_admin_user AS response_code, TRUE AS status_code, 'Employee created successfully.' AS response_msg;
END;
$$;
