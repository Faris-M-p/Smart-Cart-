/**********************************************************************
Stored Procedure : pro_employee_select_by_id
Created By       : Muhammed Faris
Created On       : 06/09/2026
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_employee_select_by_id(
    IN p_id_admin_user INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT
            e.id_admin_user AS employee_id,
            e.full_name AS employee_name,
            e.user_name,
            e.fk_user_role,
            r.role_name AS user_role_name,
            e.is_active,
            e.created_at
        FROM admin_users e
        INNER JOIN user_roles r ON r.id_user_role = e.fk_user_role
        WHERE e.id_admin_user = p_id_admin_user
          AND e.cancelled = FALSE;
END;
$$;
