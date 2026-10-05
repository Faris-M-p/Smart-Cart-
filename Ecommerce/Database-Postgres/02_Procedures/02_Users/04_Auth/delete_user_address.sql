/* =============================================================================
   Procedure : delete_user_address
   Source    : DeleteUserAddress (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE delete_user_address(
    p_user_id      INT,
    p_address_id   INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM useraddresses
        WHERE addressid = p_address_id AND userid = p_user_id AND COALESCE(cancelled, FALSE) = FALSE
    ) THEN
        UPDATE useraddresses
        SET cancelled = TRUE,
            cancelledon = NOW()
        WHERE addressid = p_address_id AND userid = p_user_id;

        OPEN p_result FOR SELECT p_address_id AS responsecode, TRUE AS statuscode, 'Address deleted successfully.'::TEXT AS responsemsg;
    ELSE
        OPEN p_result FOR SELECT -1 AS responsecode, FALSE AS statuscode, 'Address not found.'::TEXT AS responsemsg;
    END IF;
END;
$$;
