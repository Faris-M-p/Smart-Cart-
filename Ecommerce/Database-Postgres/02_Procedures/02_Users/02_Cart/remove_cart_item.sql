/* =============================================================================
   Procedure : remove_cart_item
   Source    : RemoveCartItem (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE remove_cart_item(
    p_user_id      INT,
    p_cart_item_id INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rowcount INT;
BEGIN
    DELETE FROM cartitems AS ci
    USING cart AS c
    WHERE c.id_cart = ci.fk_cart
      AND ci.id_cartitem = p_cart_item_id
      AND c.fk_user = p_user_id;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Cart item was not found.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT 0 AS responsecode, 1 AS statuscode,
           'Item removed from cart.'::TEXT AS responsemsg;
END;
$$;
