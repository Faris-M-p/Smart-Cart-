/**********************************************************************
Stored Procedure : pro_employee_delete
Created By       : Muhammed Faris
Created On       : 06/09/2026

PURPOSE
  Soft-delete an employee (cancelled = TRUE). Does not remove the row.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_employee_delete(
    IN p_id_admin_user INT,
    IN p_cancelled_reason TEXT DEFAULT NULL,
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
            SELECT -1 AS response_code, FALSE AS status_code, 'This employee is already deleted.' AS response_msg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM admin_users
        WHERE id_admin_user = p_id_admin_user AND LOWER(user_name) = 'admin'
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, FALSE AS status_code, 'The system administrator cannot be deleted.' AS response_msg;
        RETURN;
    END IF;

    UPDATE admin_users
    SET cancelled = TRUE,
        cancelled_on = NOW(),
        cancelled_reason = p_cancelled_reason,
        is_active = FALSE,
        updated_at = NOW()
    WHERE id_admin_user = p_id_admin_user;

    OPEN p_result FOR
        SELECT p_id_admin_user AS response_code, TRUE AS status_code, 'Employee deleted successfully.' AS response_msg;
END;
$$;
