/* =============================================================================
   Procedure : get_user_by_id
   Source    : GetUserById (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_user_by_id(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        u.id_user AS userid,
        COALESCE(u.fullname, u.username) AS fullname,
        u.email AS email,
        COALESCE(u.cancelled, FALSE) AS cancelled
    FROM users AS u
    WHERE u.id_user = p_user_id
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
