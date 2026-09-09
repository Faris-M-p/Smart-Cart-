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
    DELETE FROM cart_items AS ci
    USING cart AS c
    WHERE c.cart_id = ci.cart_id
      AND c.user_id = p_user_id;

    OPEN p_result FOR
    SELECT 0 AS "ResponseCode", 1 AS "StatusCode",
           'Cart cleared.'::TEXT AS "ResponseMsg";
END;
$$;
