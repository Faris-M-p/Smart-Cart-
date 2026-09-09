/* =============================================================================
   Procedure : register_user
   Source    : RegisterUser (SQL Server)

   INPUT
     p_password_hash must already be hashed by the application.
   ============================================================================= */

CREATE OR REPLACE PROCEDURE register_user(
    p_full_name     TEXT,
    p_email         TEXT,
    p_password_hash TEXT,
    INOUT p_result  refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_id INT;
BEGIN
    p_full_name := TRIM(COALESCE(p_full_name, ''));
    p_email := TRIM(COALESCE(p_email, ''));

    IF p_full_name = '' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter your name.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF p_email = '' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter your email.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF TRIM(COALESCE(p_password_hash, '')) = '' THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please enter a password.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM users
        WHERE LOWER(email) = LOWER(p_email)
          AND COALESCE(cancelled, FALSE) = FALSE
    ) THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'An account with this email already exists.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    INSERT INTO users (
        username,
        fullname,
        passwordhash,
        email,
        isadmin,
        createdat,
        cancelled
    )
    VALUES (
        p_email,
        p_full_name,
        p_password_hash,
        p_email,
        FALSE,
        NOW(),
        FALSE
    )
    RETURNING id_user INTO v_user_id;

    OPEN p_result FOR
    SELECT v_user_id AS responsecode, 1 AS statuscode,
           'Account created.'::TEXT AS responsemsg;
END;
$$;
