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
            r.id_userrole,
            r.rolename,
            r.description,
            r.issystemrole,
            r.isactive
        FROM userroles r
        WHERE r.cancelled = FALSE
        ORDER BY r.issystemrole DESC, r.rolename ASC;
END;
$$;
