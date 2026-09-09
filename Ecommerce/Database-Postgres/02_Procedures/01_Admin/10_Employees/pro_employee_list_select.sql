/**********************************************************************
Stored Procedure : pro_employee_list_select
Created By       : Muhammed Faris
Created On       : 06/09/2026

PURPOSE
  Admin Employee listing from adminusers with User Role name.
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
                e.id_adminuser,
                e.fullname,
                e.username,
                e.fk_userrole,
                r.rolename,
                e.isactive,
                e.createdat
            FROM adminusers e
            INNER JOIN userroles r ON r.id_userrole = e.fk_userrole
            WHERE e.cancelled = FALSE
              AND (
                    v_search = ''
                    OR LOWER(e.fullname) LIKE '%' || v_search || '%'
                    OR LOWER(e.username) LIKE '%' || v_search || '%'
                    OR LOWER(r.rolename) LIKE '%' || v_search || '%'
                  )
        )
        SELECT
            COUNT(1) OVER() AS total_count,
            id_adminuser AS employee_id,
            fullname AS employee_name,
            username,
            fk_userrole,
            rolename AS user_role_name,
            isactive,
            createdat
        FROM filtered
        ORDER BY id_adminuser DESC
        OFFSET (v_page_index - 1) * v_page_size
        LIMIT v_page_size;
END;
$$;
