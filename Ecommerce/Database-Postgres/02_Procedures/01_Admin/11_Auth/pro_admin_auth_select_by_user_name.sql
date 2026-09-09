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
            e.id_admin_user,
            e.user_name,
            e.password_hash,
            e.full_name,
            e.is_active,
            e.cancelled,
            e.fk_user_role,
            r.role_name,
            r.is_active AS role_is_active,
            r.cancelled AS role_cancelled
        FROM admin_users e
        LEFT JOIN user_roles r ON r.id_user_role = e.fk_user_role
        WHERE e.user_name = p_user_name;
END;
$$;
