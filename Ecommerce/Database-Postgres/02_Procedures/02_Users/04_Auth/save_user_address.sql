CREATE OR REPLACE FUNCTION save_user_address(
    p_userid        INT,
    p_addressid     INT DEFAULT 0,
    p_addresstype   TEXT DEFAULT 'Home',
    p_receivername  TEXT DEFAULT '',
    p_phone         TEXT DEFAULT '',
    p_addressline   TEXT DEFAULT '',
    p_city          TEXT DEFAULT '',
    p_pincode       TEXT DEFAULT '',
    p_latitude      NUMERIC DEFAULT NULL,
    p_longitude     NUMERIC DEFAULT NULL,
    p_isdefault     BOOLEAN DEFAULT FALSE
)
RETURNS TABLE (
    ResponseCode BIGINT,
    StatusCode   BOOLEAN,
    ResponseMsg  TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_addressid     INT := COALESCE(p_addressid, 0);
    v_addresstype   TEXT := TRIM(COALESCE(p_addresstype, 'Home'));
    v_receivername  TEXT := TRIM(COALESCE(p_receivername, ''));
    v_phone         TEXT := TRIM(COALESCE(p_phone, ''));
    v_addressline   TEXT := TRIM(COALESCE(p_addressline, ''));
    v_city          TEXT := TRIM(COALESCE(p_city, ''));
    v_pincode       TEXT := TRIM(COALESCE(p_pincode, ''));
    v_isdefault     BOOLEAN := COALESCE(p_isdefault, FALSE);
    v_newid         INT;
BEGIN
    IF v_addresstype = '' THEN v_addresstype := 'Home'; END IF;

    IF v_receivername = '' THEN
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'Receiver name is required.'::TEXT;
        RETURN;
    END IF;

    IF v_phone = '' THEN
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'Phone number is required.'::TEXT;
        RETURN;
    END IF;

    IF v_addressline = '' THEN
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'Address line is required.'::TEXT;
        RETURN;
    END IF;

    IF v_city = '' THEN
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'City is required.'::TEXT;
        RETURN;
    END IF;

    IF v_pincode = '' THEN
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'Pincode is required.'::TEXT;
        RETURN;
    END IF;

    -- Enforce strictly 1 address per type per user (Home, Work, Office, Other)
    IF v_addressid <= 0 THEN
        SELECT addressid INTO v_addressid
        FROM useraddresses
        WHERE userid = p_userid
          AND LOWER(addresstype) = LOWER(v_addresstype)
          AND COALESCE(cancelled, FALSE) = FALSE
        LIMIT 1;
        v_addressid := COALESCE(v_addressid, 0);
    END IF;

    IF v_isdefault THEN
        UPDATE useraddresses
        SET isdefault = FALSE
        WHERE userid = p_userid AND COALESCE(cancelled, FALSE) = FALSE;
    END IF;

    IF v_addressid > 0 AND EXISTS (
        SELECT 1 FROM useraddresses
        WHERE addressid = v_addressid AND userid = p_userid AND COALESCE(cancelled, FALSE) = FALSE
    ) THEN
        UPDATE useraddresses
        SET addresstype  = v_addresstype,
            receivername = v_receivername,
            phone        = v_phone,
            addressline  = v_addressline,
            city         = v_city,
            pincode      = v_pincode,
            latitude     = p_latitude,
            longitude    = p_longitude,
            isdefault    = v_isdefault
        WHERE addressid = v_addressid AND userid = p_userid;

        RETURN QUERY SELECT v_addressid::BIGINT, TRUE, 'Address updated successfully.'::TEXT;
    ELSE
        IF NOT EXISTS (
            SELECT 1 FROM useraddresses
            WHERE userid = p_userid AND COALESCE(cancelled, FALSE) = FALSE
        ) THEN
            v_isdefault := TRUE;
        END IF;

        INSERT INTO useraddresses (
            userid, addresstype, receivername, phone, addressline, city, pincode, latitude, longitude, isdefault, createdat, cancelled
        )
        VALUES (
            p_userid, v_addresstype, v_receivername, v_phone, v_addressline, v_city, v_pincode, p_latitude, p_longitude, v_isdefault, NOW(), FALSE
        )
        RETURNING addressid INTO v_newid;

        RETURN QUERY SELECT v_newid::BIGINT, TRUE, 'Address saved successfully.'::TEXT;
    END IF;
END;
$$;
