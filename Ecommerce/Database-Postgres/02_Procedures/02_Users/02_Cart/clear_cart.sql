/* =============================================================================
   Procedure : clear_cart
   Source    : ClearCart (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE clear_cart(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM cartitems AS ci
    USING cart AS c
    WHERE c.id_cart = ci.fk_cart
      AND c.fk_user = p_user_id;

    OPEN p_result FOR
    SELECT 0 AS responsecode, 1 AS statuscode,
           'Cart cleared.'::TEXT AS responsemsg;
END;
$$;
