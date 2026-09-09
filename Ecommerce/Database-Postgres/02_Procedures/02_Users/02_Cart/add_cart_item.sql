/* =============================================================================
   Procedure : add_cart_item
   Source    : AddCartItem (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE add_cart_item(
    p_user_id            INT,
    p_product_variant_id INT,
    p_quantity           INT DEFAULT 1,
    INOUT p_result       refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_id       INT;
    v_price            NUMERIC(10, 2);
    v_stock_qty        INT;
    v_max_order_qty    INT;
    v_cart_id          INT;
    v_existing_item_id INT;
    v_existing_qty     INT;
    v_distinct_count   INT;
    v_new_qty          INT;
BEGIN
    IF COALESCE(p_user_id, 0) < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please log in to add items to cart.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF COALESCE(p_quantity, 0) < 1 THEN
        p_quantity := 1;
    END IF;

    SELECT
        pv.fk_product,
        pv.selling_price,
        COALESCE(pv.max_order_qty, 10)
    INTO
        v_product_id,
        v_price,
        v_max_order_qty
    FROM product_variants AS pv
    INNER JOIN products AS p ON p.id_product = pv.fk_product
    WHERE pv.id_product_variant = p_product_variant_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.is_active = TRUE
      AND COALESCE(pv.sell_online, FALSE) = TRUE
      AND COALESCE(p.cancelled, FALSE) = FALSE
      AND p.is_active = TRUE
      AND COALESCE(p.sell_online, FALSE) = TRUE;

    IF v_product_id IS NULL THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This product option is not available.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT COALESCE(SUM(s.quantity), 0)
    INTO v_stock_qty
    FROM stock AS s
    WHERE s.fk_product_variant = p_product_variant_id
      AND COALESCE(s.cancelled, FALSE) = FALSE;

    IF v_stock_qty < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This variant is out of stock.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT c.cart_id
    INTO v_cart_id
    FROM cart AS c
    WHERE c.user_id = p_user_id
    LIMIT 1;

    IF v_cart_id IS NULL THEN
        INSERT INTO cart (user_id, created_at)
        VALUES (p_user_id, NOW())
        RETURNING cart_id INTO v_cart_id;
    END IF;

    SELECT
        ci.cart_item_id,
        ci.quantity
    INTO
        v_existing_item_id,
        v_existing_qty
    FROM cart_items AS ci
    WHERE ci.cart_id = v_cart_id
      AND ci.product_variant_id = p_product_variant_id;

    v_new_qty := COALESCE(v_existing_qty, 0) + p_quantity;

    IF v_existing_item_id IS NULL THEN
        SELECT COUNT(1)
        INTO v_distinct_count
        FROM cart_items
        WHERE cart_id = v_cart_id;

        IF v_distinct_count >= 20 THEN
            OPEN p_result FOR
            SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
                   'Your cart can hold 20 products. Remove one before adding another.'::TEXT AS "ResponseMsg";
            RETURN;
        END IF;
    END IF;

    IF v_new_qty > v_stock_qty THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               ('Only ' || v_stock_qty::TEXT || ' units are in stock.')::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF v_max_order_qty > 0 AND v_new_qty > v_max_order_qty THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               ('You can add up to ' || v_max_order_qty::TEXT || ' of this item.')::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF v_existing_item_id IS NULL THEN
        INSERT INTO cart_items (cart_id, product_id, product_variant_id, quantity, price, created_at)
        VALUES (v_cart_id, v_product_id, p_product_variant_id, v_new_qty, v_price, NOW())
        RETURNING cart_item_id INTO v_existing_item_id;
    ELSE
        UPDATE cart_items
        SET quantity = v_new_qty,
            price = v_price
        WHERE cart_item_id = v_existing_item_id;
    END IF;

    OPEN p_result FOR
    SELECT v_existing_item_id AS "ResponseCode", 1 AS "StatusCode",
           'Added to cart.'::TEXT AS "ResponseMsg";
END;
$$;
