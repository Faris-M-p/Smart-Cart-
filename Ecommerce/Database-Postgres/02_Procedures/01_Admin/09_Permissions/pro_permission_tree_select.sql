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
            m.module_name,
            m.display_name,
            m.display_order AS module_display_order,
            p.id_permission,
            p.permission_name,
            p.permission_code,
            p.display_order AS permission_display_order
        FROM modules m
        INNER JOIN permissions p ON p.fk_module = m.id_module
        WHERE m.cancelled = FALSE
          AND m.is_active = TRUE
          AND p.cancelled = FALSE
          AND p.is_active = TRUE
        ORDER BY m.display_order ASC, p.display_order ASC, p.permission_name ASC;
END;
$$;
