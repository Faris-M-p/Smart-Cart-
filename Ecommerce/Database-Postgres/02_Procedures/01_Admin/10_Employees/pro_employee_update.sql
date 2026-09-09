/**********************************************************************
Stored Procedure : pro_employee_update
Created By       : Muhammed Faris
Created On       : 06/09/2026

INPUT
  p_password_hash NULL/empty = keep existing hash.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_employee_update(
    IN p_id_admin_user INT,
    IN p_full_name TEXT,
    IN p_user_name TEXT,
    IN p_password_hash TEXT DEFAULT NULL,
    IN p_fk_user_role INT DEFAULT NULL,
    IN p_is_active BOOLEAN DEFAULT TRUE,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM adminusers WHERE id_adminuser = p_id_admin_user) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Employee not found.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM adminusers
        WHERE id_adminuser = p_id_admin_user AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'This employee is deleted and cannot be edited.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM adminusers
        WHERE id_adminuser = p_id_admin_user AND LOWER(username) = 'admin'
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'The system administrator cannot be modified.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM adminusers
        WHERE id_adminuser <> p_id_admin_user
          AND LOWER(username) = LOWER(TRIM(p_user_name))
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Username already exists.' AS response_msg;
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

    UPDATE adminusers
    SET fullname = TRIM(p_full_name),
        username = TRIM(p_user_name),
        fk_userrole = p_fk_user_role,
        isactive = COALESCE(p_is_active, TRUE),
        passwordhash = CASE
            WHEN TRIM(COALESCE(p_password_hash, '')) = '' THEN passwordhash
            ELSE p_password_hash
        END,
        updatedat = NOW()
    WHERE id_adminuser = p_id_admin_user;

    OPEN p_result FOR
        SELECT p_id_admin_user AS response_code, TRUE AS status_code, 'Employee updated successfully.' AS response_msg;
END;
$$;
