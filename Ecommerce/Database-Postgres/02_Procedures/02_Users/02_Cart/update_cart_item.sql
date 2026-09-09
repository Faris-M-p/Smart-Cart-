/* =============================================================================
   Procedure : update_cart_item
   Source    : UpdateCartItem (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE update_cart_item(
    p_user_id      INT,
    p_cart_item_id INT,
    p_quantity     INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_variant_id INT;
    v_stock_qty          INT;
    v_max_order_qty      INT;
    v_price              NUMERIC(10, 2);
BEGIN
    IF COALESCE(p_quantity, 0) < 1 THEN
        DELETE FROM cartitems AS ci
        USING cart AS c
        WHERE c.id_cart = ci.fk_cart
          AND ci.id_cartitem = p_cart_item_id
          AND c.fk_user = p_user_id;

        OPEN p_result FOR
        SELECT 0 AS responsecode, 1 AS statuscode,
               'Item removed from cart.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT
        ci.fk_productvariant,
        pv.sellingprice,
        COALESCE(pv.maxorderqty, 10)
    INTO
        v_product_variant_id,
        v_price,
        v_max_order_qty
    FROM cartitems AS ci
    INNER JOIN cart AS c ON c.id_cart = ci.fk_cart
    INNER JOIN productvariants AS pv ON pv.id_productvariant = ci.fk_productvariant
    WHERE ci.id_cartitem = p_cart_item_id
      AND c.fk_user = p_user_id;

    IF v_product_variant_id IS NULL THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Cart item was not found.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT COALESCE(SUM(s.quantity), 0)
    INTO v_stock_qty
    FROM stock AS s
    WHERE s.fk_productvariant = v_product_variant_id
      AND COALESCE(s.cancelled, FALSE) = FALSE;

    IF p_quantity > v_stock_qty THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               ('Only ' || v_stock_qty::TEXT || ' units are in stock.')::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_max_order_qty > 0 AND p_quantity > v_max_order_qty THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               ('You can add up to ' || v_max_order_qty::TEXT || ' of this item.')::TEXT AS responsemsg;
        RETURN;
    END IF;

    UPDATE cartitems
    SET quantity = p_quantity,
        price = COALESCE(v_price, price)
    WHERE id_cartitem = p_cart_item_id;

    OPEN p_result FOR
    SELECT p_cart_item_id AS responsecode, 1 AS statuscode,
           'Cart updated.'::TEXT AS responsemsg;
END;
$$;
