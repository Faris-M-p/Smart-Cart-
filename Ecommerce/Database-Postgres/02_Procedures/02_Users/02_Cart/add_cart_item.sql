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
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please log in to add items to cart.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF COALESCE(p_quantity, 0) < 1 THEN
        p_quantity := 1;
    END IF;

    SELECT
        pv.fk_product,
        pv.sellingprice,
        COALESCE(pv.maxorderqty, 10)
    INTO
        v_product_id,
        v_price,
        v_max_order_qty
    FROM productvariants AS pv
    INNER JOIN products AS p ON p.id_product = pv.fk_product
    WHERE pv.id_productvariant = p_product_variant_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.isactive = TRUE
      AND COALESCE(pv.sellonline, FALSE) = TRUE
      AND COALESCE(p.cancelled, FALSE) = FALSE
      AND p.isactive = TRUE
      AND COALESCE(p.sellonline, FALSE) = TRUE;

    IF v_product_id IS NULL THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'This product option is not available.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT COALESCE(SUM(s.quantity), 0)
    INTO v_stock_qty
    FROM stock AS s
    WHERE s.fk_productvariant = p_product_variant_id
      AND COALESCE(s.cancelled, FALSE) = FALSE;

    IF v_stock_qty < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'This variant is out of stock.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT c.id_cart
    INTO v_cart_id
    FROM cart AS c
    WHERE c.fk_user = p_user_id
    LIMIT 1;

    IF v_cart_id IS NULL THEN
        INSERT INTO cart (fk_user, createdat)
        VALUES (p_user_id, NOW())
        RETURNING id_cart INTO v_cart_id;
    END IF;

    SELECT
        ci.id_cartitem,
        ci.quantity
    INTO
        v_existing_item_id,
        v_existing_qty
    FROM cartitems AS ci
    WHERE ci.fk_cart = v_cart_id
      AND ci.fk_productvariant = p_product_variant_id;

    v_new_qty := COALESCE(v_existing_qty, 0) + p_quantity;

    IF v_existing_item_id IS NULL THEN
        SELECT COUNT(1)
        INTO v_distinct_count
        FROM cartitems
        WHERE fk_cart = v_cart_id;

        IF v_distinct_count >= 20 THEN
            OPEN p_result FOR
            SELECT -1 AS responsecode, 0 AS statuscode,
                   'Your cart can hold 20 products. Remove one before adding another.'::TEXT AS responsemsg;
            RETURN;
        END IF;
    END IF;

    IF v_new_qty > v_stock_qty THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               ('Only ' || v_stock_qty::TEXT || ' units are in stock.')::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_max_order_qty > 0 AND v_new_qty > v_max_order_qty THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               ('You can add up to ' || v_max_order_qty::TEXT || ' of this item.')::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_existing_item_id IS NULL THEN
        INSERT INTO cartitems (fk_cart, fk_product, fk_productvariant, quantity, price, createdat)
        VALUES (v_cart_id, v_product_id, p_product_variant_id, v_new_qty, v_price, NOW())
        RETURNING id_cartitem INTO v_existing_item_id;
    ELSE
        UPDATE cartitems
        SET quantity = v_new_qty,
            price = v_price
        WHERE id_cartitem = v_existing_item_id;
    END IF;

    OPEN p_result FOR
    SELECT v_existing_item_id AS responsecode, 1 AS statuscode,
           'Added to cart.'::TEXT AS responsemsg;
END;
$$;
