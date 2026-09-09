/**********************************************************************
Stored Procedure : pro_employee_list_select
Created By       : Muhammed Faris
Created On       : 06/09/2026

PURPOSE
  Admin Employee listing from admin_users with User Role name.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_employee_list_select(
    IN p_search_text TEXT DEFAULT '',
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_page_index INT := COALESCE(p_page_index, 1);
    v_page_size INT := COALESCE(p_page_size, 10);
    v_search TEXT;
BEGIN
    IF (v_page_index < 1) THEN
        v_page_index := 1;
    END IF;
    IF (v_page_size < 1) THEN
        v_page_size := 10;
    END IF;

    v_search := LOWER(TRIM(COALESCE(p_search_text, '')));

    OPEN p_result FOR
        WITH filtered AS (
            SELECT
                e.id_admin_user,
                e.full_name,
                e.user_name,
                e.fk_user_role,
                r.role_name,
                e.is_active,
                e.created_at
            FROM admin_users e
            INNER JOIN user_roles r ON r.id_user_role = e.fk_user_role
            WHERE e.cancelled = FALSE
              AND (
                    v_search = ''
                    OR LOWER(e.full_name) LIKE '%' || v_search || '%'
                    OR LOWER(e.user_name) LIKE '%' || v_search || '%'
                    OR LOWER(r.role_name) LIKE '%' || v_search || '%'
                  )
        )
        SELECT
            COUNT(1) OVER() AS total_count,
            id_admin_user AS employee_id,
            full_name AS employee_name,
            user_name,
            fk_user_role,
            role_name AS user_role_name,
            is_active,
            created_at
        FROM filtered
        ORDER BY id_admin_user DESC
        OFFSET (v_page_index - 1) * v_page_size
        LIMIT v_page_size;
END;
$$;
