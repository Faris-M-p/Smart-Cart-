/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Selected permission IDs for a User Role
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_user_role_permission_select(
    IN p_id_user_role INT,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT urp.fk_permission AS id_permission
        FROM user_role_permissions urp
        INNER JOIN permissions p ON p.id_permission = urp.fk_permission
        WHERE urp.fk_user_role = p_id_user_role
          AND urp.cancelled = FALSE
          AND p.cancelled = FALSE
        ORDER BY p.display_order ASC;
END;
$$;
