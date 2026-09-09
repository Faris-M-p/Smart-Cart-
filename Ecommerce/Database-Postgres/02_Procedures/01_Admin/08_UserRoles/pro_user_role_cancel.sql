/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Soft-delete a User Role when no employees are assigned
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_user_role_cancel(
    IN p_id_user_role INT,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
BEGIN
    IF NOT EXISTS (SELECT 1 FROM userroles WHERE id_userrole = p_id_user_role) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'User role not found.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM userroles
        WHERE id_userrole = p_id_user_role AND cancelled = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'This user role is already deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM userroles
        WHERE id_userrole = p_id_user_role AND issystemrole = TRUE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'System roles cannot be deleted.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM adminusers
        WHERE fk_userrole = p_id_user_role
          AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'Cannot delete this role because employees are assigned to it.' AS response_msg,
                   FALSE AS status_code;
        RETURN;
    END IF;

    UPDATE userroles
    SET cancelled = TRUE,
        cancelledon = v_now,
        cancelledreason = p_cancelled_reason,
        updatedat = v_now
    WHERE id_userrole = p_id_user_role;

    UPDATE userrolepermissions
    SET cancelled = TRUE,
        cancelledon = v_now,
        cancelledreason = 'Role cancelled'
    WHERE fk_userrole = p_id_user_role
      AND cancelled = FALSE;

    OPEN p_result FOR
        SELECT p_id_user_role AS response_code,
               'User role deleted successfully.' AS response_msg,
               TRUE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
