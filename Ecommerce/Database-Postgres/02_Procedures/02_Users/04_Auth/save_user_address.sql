/* =============================================================================
   Procedure : save_user_address
   Source    : SaveUserAddress (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE save_user_address(
    p_user_id       INT,
    p_address_id    INT DEFAULT 0,
    p_address_type  TEXT DEFAULT 'Home',
    p_receiver_name TEXT DEFAULT '',
    p_phone         TEXT DEFAULT '',
    p_address_line  TEXT DEFAULT '',
    p_city          TEXT DEFAULT '',
    p_pincode       TEXT DEFAULT '',
    p_latitude      NUMERIC DEFAULT NULL,
    p_longitude     NUMERIC DEFAULT NULL,
    p_is_default    BOOLEAN DEFAULT FALSE,
    INOUT p_result  refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_addressid     INT := COALESCE(p_address_id, 0);
    v_addresstype   TEXT := TRIM(COALESCE(p_address_type, 'Home'));
    v_receivername  TEXT := TRIM(COALESCE(p_receiver_name, ''));
    v_phone         TEXT := TRIM(COALESCE(p_phone, ''));
    v_addressline   TEXT := TRIM(COALESCE(p_address_line, ''));
    v_city          TEXT := TRIM(COALESCE(p_city, ''));
    v_pincode       TEXT := TRIM(COALESCE(p_pincode, ''));
    v_isdefault     BOOLEAN := COALESCE(p_is_default, FALSE);
    v_newid         INT;
BEGIN
    IF v_addresstype = '' THEN v_addresstype := 'Home'; END IF;

    IF v_receivername = '' THEN
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'Receiver name is required.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_phone = '' THEN
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'Phone number is required.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_addressline = '' THEN
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'Address line is required.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_city = '' THEN
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'City is required.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_pincode = '' THEN
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'Pincode is required.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    -- Enforce strictly 1 address per type per user (Home, Work, Office, Other)
    IF v_addressid <= 0 THEN
        SELECT addressid INTO v_addressid
        FROM useraddresses
        WHERE userid = p_user_id
          AND LOWER(addresstype) = LOWER(v_addresstype)
          AND COALESCE(cancelled, FALSE) = FALSE
        LIMIT 1;
        v_addressid := COALESCE(v_addressid, 0);
    END IF;

    IF v_isdefault THEN
        UPDATE useraddresses
        SET isdefault = FALSE
        WHERE userid = p_user_id AND COALESCE(cancelled, FALSE) = FALSE;
    END IF;

    IF v_addressid > 0 AND EXISTS (
        SELECT 1 FROM useraddresses
        WHERE addressid = v_addressid AND userid = p_user_id AND COALESCE(cancelled, FALSE) = FALSE
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
        WHERE addressid = v_addressid AND userid = p_user_id;

        OPEN p_result FOR SELECT v_addressid AS responsecode, TRUE AS statuscode, 'Address updated successfully.'::TEXT AS responsemsg;
    ELSE
        IF NOT EXISTS (
            SELECT 1 FROM useraddresses
            WHERE userid = p_user_id AND COALESCE(cancelled, FALSE) = FALSE
        ) THEN
            v_isdefault := TRUE;
        END IF;

        INSERT INTO useraddresses (
            userid, addresstype, receivername, phone, addressline, city, pincode, latitude, longitude, isdefault, createdat, cancelled
        )
        VALUES (
            p_user_id, v_addresstype, v_receivername, v_phone, v_addressline, v_city, v_pincode, p_latitude, p_longitude, v_isdefault, NOW(), FALSE
        )
        RETURNING addressid INTO v_newid;

        OPEN p_result FOR SELECT v_newid AS responsecode, TRUE AS statuscode, 'Address saved successfully.'::TEXT AS responsemsg;
    END IF;
END;
$$;
