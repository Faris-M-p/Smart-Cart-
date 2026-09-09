/* =============================================================================
   Procedure : get_user_by_email
   Source    : GetUserByEmail (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_user_by_email(
    p_email        TEXT,
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
        u.password_hash AS "PasswordHash",
        COALESCE(u.cancelled, FALSE) AS "Cancelled"
    FROM users AS u
    WHERE LOWER(u.email) = LOWER(TRIM(COALESCE(p_email, '')))
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
