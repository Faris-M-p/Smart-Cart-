/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : List active (non-cancelled) User Roles
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_user_role_list_select(
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT
            r.id_user_role,
            r.role_name,
            r.description,
            r.is_system_role,
            r.is_active
        FROM user_roles r
        WHERE r.cancelled = FALSE
        ORDER BY r.is_system_role DESC, r.role_name ASC;
END;
$$;
