CREATE OR REPLACE FUNCTION get_user_addresses(
    p_userid INT
)
RETURNS TABLE (
    AddressId       INT,
    UserId          INT,
    AddressType     TEXT,
    ReceiverName    TEXT,
    Phone           TEXT,
    AddressLine     TEXT,
    City            TEXT,
    Pincode         TEXT,
    Latitude        NUMERIC,
    Longitude       NUMERIC,
    IsDefault       BOOLEAN,
    CreatedAt       TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.addressid,
        u.userid,
        u.addresstype,
        u.receivername,
        u.phone,
        u.addressline,
        u.city,
        u.pincode,
        u.latitude,
        u.longitude,
        u.isdefault,
        u.createdat
    FROM useraddresses u
    WHERE u.userid = p_userid
      AND COALESCE(u.cancelled, FALSE) = FALSE
    ORDER BY u.isdefault DESC, u.addressid DESC;
END;
$$;
