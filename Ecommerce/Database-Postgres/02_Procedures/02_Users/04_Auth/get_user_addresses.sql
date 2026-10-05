/* =============================================================================
   Procedure : get_user_addresses
   Source    : GetUserAddresses (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_user_addresses(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        u.addressid     AS addressid,
        u.userid        AS userid,
        u.addresstype   AS addresstype,
        u.receivername  AS receivername,
        u.phone         AS phone,
        u.addressline   AS addressline,
        u.city          AS city,
        u.pincode       AS pincode,
        u.latitude      AS latitude,
        u.longitude     AS longitude,
        u.isdefault     AS isdefault,
        u.createdat     AS createdat
    FROM useraddresses u
    WHERE u.userid = p_user_id
      AND COALESCE(u.cancelled, FALSE) = FALSE
    ORDER BY u.isdefault DESC, u.addressid DESC;
END;
$$;
