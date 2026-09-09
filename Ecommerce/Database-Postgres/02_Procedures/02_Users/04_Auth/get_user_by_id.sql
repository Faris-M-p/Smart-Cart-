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
        u.user_id AS "UserId",
        COALESCE(u.full_name, u.user_name) AS "FullName",
        u.email AS "Email",
        COALESCE(u.cancelled, FALSE) AS "Cancelled"
    FROM users AS u
    WHERE u.user_id = p_user_id
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
