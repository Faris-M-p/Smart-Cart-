/**********************************************************************
Stored Procedure : pro_admin_auth_permission_select
Created By       : Muhammed Faris
Created On       : 06/09/2026
Purpose          : Active permission codes for an Employee User Role
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_admin_auth_permission_select(
    IN p_id_user_role INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT
            p.permission_code
        FROM user_role_permissions urp
        INNER JOIN permissions p ON p.id_permission = urp.fk_permission
        WHERE urp.fk_user_role = p_id_user_role
          AND urp.cancelled = FALSE
          AND p.cancelled = FALSE
          AND p.is_active = TRUE
        ORDER BY p.display_order ASC;
END;
$$;
