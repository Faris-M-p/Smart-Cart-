/**********************************************************************
Stored Procedure : pro_admin_auth_select_by_user_name
Created By       : Muhammed Faris
Created On       : 06/09/2026
Purpose          : Admin login lookup — employee + role by username
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_admin_auth_select_by_user_name(
    IN p_user_name TEXT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT
            e.id_adminuser,
            e.username,
            e.passwordhash,
            e.fullname,
            e.isactive,
            e.cancelled,
            e.fk_userrole,
            r.rolename,
            r.isactive AS role_is_active,
            r.cancelled AS role_cancelled
        FROM adminusers e
        LEFT JOIN userroles r ON r.id_userrole = e.fk_userrole
        WHERE e.username = p_user_name;
END;
$$;
