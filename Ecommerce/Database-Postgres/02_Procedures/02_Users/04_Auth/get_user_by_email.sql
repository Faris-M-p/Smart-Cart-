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
        u.id_user AS userid,
        COALESCE(u.fullname, u.username) AS fullname,
        u.email AS email,
        u.passwordhash AS passwordhash,
        COALESCE(u.cancelled, FALSE) AS cancelled
    FROM users AS u
    WHERE LOWER(u.email) = LOWER(TRIM(COALESCE(p_email, '')))
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
