CREATE OR REPLACE FUNCTION delete_user_address(
    p_userid    INT,
    p_addressid INT
)
RETURNS TABLE (
    ResponseCode BIGINT,
    StatusCode   BOOLEAN,
    ResponseMsg  TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM useraddresses
        WHERE addressid = p_addressid AND userid = p_userid AND COALESCE(cancelled, FALSE) = FALSE
    ) THEN
        UPDATE useraddresses
        SET cancelled = TRUE,
            cancelledon = NOW()
        WHERE addressid = p_addressid AND userid = p_userid;

        RETURN QUERY SELECT p_addressid::BIGINT, TRUE, 'Address deleted successfully.'::TEXT;
    ELSE
        RETURN QUERY SELECT -1::BIGINT, FALSE, 'Address not found.'::TEXT;
    END IF;
END;
$$;
