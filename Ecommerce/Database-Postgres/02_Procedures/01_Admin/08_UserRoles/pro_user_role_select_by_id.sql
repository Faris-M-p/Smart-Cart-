/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Select one User Role and its assigned permission IDs
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_user_role_select_by_id(
    IN p_id_user_role INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_permissions REFCURSOR DEFAULT 'p_permissions'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM userroles
        WHERE id_userrole = p_id_user_role
          AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'User role not found.' AS response_msg, FALSE AS status_code;
        OPEN p_permissions FOR
            SELECT NULL::INT AS id_permission WHERE FALSE;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT
            r.id_userrole,
            r.rolename,
            r.description,
            r.issystemrole,
            r.isactive
        FROM userroles r
        WHERE r.id_userrole = p_id_user_role
          AND r.cancelled = FALSE;

    OPEN p_permissions FOR
        SELECT urp.fk_permission AS id_permission
        FROM userrolepermissions urp
        WHERE urp.fk_userrole = p_id_user_role
          AND urp.cancelled = FALSE;
END;
$$;
