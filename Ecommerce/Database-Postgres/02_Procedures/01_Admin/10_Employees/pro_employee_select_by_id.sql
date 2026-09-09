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
            e.id_adminuser AS employee_id,
            e.fullname AS employee_name,
            e.username,
            e.fk_userrole,
            r.rolename AS user_role_name,
            e.isactive,
            e.createdat
        FROM adminusers e
        INNER JOIN userroles r ON r.id_userrole = e.fk_userrole
        WHERE e.id_adminuser = p_id_admin_user
          AND e.cancelled = FALSE;
END;
$$;
