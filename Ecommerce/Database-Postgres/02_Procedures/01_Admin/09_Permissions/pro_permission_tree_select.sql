/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Flat module + permission list for the Admin permission tree
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_permission_tree_select(
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
        SELECT
            m.id_module,
            m.modulename,
            m.displayname,
            m.displayorder AS module_display_order,
            p.id_permission,
            p.permissionname,
            p.permissioncode,
            p.displayorder AS permission_display_order
        FROM modules m
        INNER JOIN permissions p ON p.fk_module = m.id_module
        WHERE m.cancelled = FALSE
          AND m.isactive = TRUE
          AND p.cancelled = FALSE
          AND p.isactive = TRUE
        ORDER BY m.displayorder ASC, p.displayorder ASC, p.permissionname ASC;
END;
$$;
