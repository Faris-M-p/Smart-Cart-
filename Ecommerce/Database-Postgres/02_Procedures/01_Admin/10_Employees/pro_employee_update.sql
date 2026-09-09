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
    IN p_fk_user_role INT,
    IN p_is_active BOOLEAN DEFAULT TRUE,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM admin_users WHERE id_admin_user = p_id_admin_user) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Employee not found.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM admin_users
        WHERE id_admin_user = p_id_admin_user AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'This employee is deleted and cannot be edited.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM admin_users
        WHERE id_admin_user = p_id_admin_user AND LOWER(user_name) = 'admin'
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'The system administrator cannot be modified.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM admin_users
        WHERE id_admin_user <> p_id_admin_user
          AND LOWER(user_name) = LOWER(TRIM(p_user_name))
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Username already exists.' AS response_msg;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM user_roles
        WHERE id_user_role = p_fk_user_role AND cancelled = FALSE AND is_active = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'Please select a valid active user role.' AS response_msg;
        RETURN;
    END IF;

    UPDATE admin_users
    SET full_name = TRIM(p_full_name),
        user_name = TRIM(p_user_name),
        fk_user_role = p_fk_user_role,
        is_active = COALESCE(p_is_active, TRUE),
        password_hash = CASE
            WHEN TRIM(COALESCE(p_password_hash, '')) = '' THEN password_hash
            ELSE p_password_hash
        END,
        updated_at = NOW()
    WHERE id_admin_user = p_id_admin_user;

    OPEN p_result FOR
        SELECT p_id_admin_user AS response_code, TRUE AS status_code, 'Employee updated successfully.' AS response_msg;
END;
$$;
