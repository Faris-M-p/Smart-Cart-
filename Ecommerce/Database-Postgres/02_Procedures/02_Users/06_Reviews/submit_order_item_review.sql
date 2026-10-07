/**********************************************************************
Stored Procedure : submit_order_item_review
Tables           : ratings, orders, orderitems, shipping

PURPOSE
  A customer reviews ONE purchased order item (Flipkart style).
  Rule: one customer + one order item = at most one active review,
  regardless of quantity. The same product bought in another order is
  a different order item and can be reviewed again.

VALIDATION CHAIN
  signed-in user -> order item exists and belongs to the order and the
  product AND the order belongs to the user -> order delivered (not
  cancelled) -> inside the 7-day window -> no active review yet -> rating
  1..5 -> insert.

RESPONSE (p_result)
  responsecode > 0  : new review id (success)
  responsecode = -1 : validation / not signed in
  responsecode = -2 : duplicate (this order item is already reviewed)
  responsecode = -3 : order is not delivered yet / cancelled
  responsecode = -5 : access denied (order item is not the caller's, or
                      the ids do not match each other)
  responsecode = -6 : review window (7 days from delivery) is over

NOTES
  - p_user_id is taken from the authenticated session, never from the client.
  - Duplicates are blocked three ways: advisory lock per order item,
    the explicit check below, and the partial unique index
    ux_ratings_orderitem_active.
**********************************************************************/

CREATE OR REPLACE PROCEDURE submit_order_item_review(
    p_user_id       INT,
    p_order_id      INT,
    p_order_item_id INT,
    p_product_id    INT,
    p_rating        INT,
    p_review        TEXT,
    INOUT p_result  refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_window       CONSTANT INTERVAL := INTERVAL '7 days';
    v_now          TIMESTAMP := LOCALTIMESTAMP;
    v_review       TEXT := NULLIF(BTRIM(COALESCE(p_review, '')), '');
    v_owner_id     INT;
    v_order_id     INT;
    v_product_id   INT;
    v_status       TEXT;
    v_cancelled    BOOLEAN;
    v_delivered_on TIMESTAMP;
    v_new_id       INT;
BEGIN
    IF COALESCE(p_user_id, 0) < 1
       OR NOT EXISTS (
            SELECT 1 FROM users
            WHERE id_user = p_user_id
              AND COALESCE(cancelled, FALSE) = FALSE
       ) THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, FALSE AS statuscode,
               'Please log in to review this product.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF p_rating IS NULL OR p_rating < 1 OR p_rating > 5 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, FALSE AS statuscode,
               'Please select a rating from 1 to 5 stars.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_review IS NOT NULL AND CHAR_LENGTH(v_review) > 1000 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, FALSE AS statuscode,
               'Review cannot exceed 1000 characters.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    -- Order item -> order -> customer, and order item -> product
    SELECT o.fk_user,
           oi.fk_order,
           oi.fk_product,
           o.orderstatus,
           COALESCE(o.cancelled, FALSE),
           COALESCE(
               (
                   SELECT MAX(s.shippingdate)
                   FROM shipping AS s
                   WHERE s.fk_order = o.id_order
                     AND COALESCE(s.cancelled, FALSE) = FALSE
                     AND s.shippingstatus = 'Delivered'
               ),
               o.orderdate
           )
    INTO v_owner_id, v_order_id, v_product_id, v_status, v_cancelled, v_delivered_on
    FROM orderitems AS oi
    INNER JOIN orders AS o ON o.id_order = oi.fk_order
    WHERE oi.id_orderitem = p_order_item_id;

    IF v_owner_id IS DISTINCT FROM p_user_id
       OR v_order_id IS DISTINCT FROM p_order_id
       OR v_product_id IS DISTINCT FROM p_product_id THEN
        OPEN p_result FOR
        SELECT -5 AS responsecode, FALSE AS statuscode,
               'You can only review items from your own orders.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_cancelled OR v_status <> 'Delivered' THEN
        OPEN p_result FOR
        SELECT -3 AS responsecode, FALSE AS statuscode,
               'You can review this item once your order has been delivered.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_delivered_on IS NULL OR v_delivered_on + v_window <= v_now THEN
        OPEN p_result FOR
        SELECT -6 AS responsecode, FALSE AS statuscode,
               'The review period for this item has ended.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    -- Serialise concurrent submits for the same order item
    PERFORM pg_advisory_xact_lock(
        hashtextextended('review:orderitem:' || p_order_item_id::TEXT, 0)
    );

    IF EXISTS (
        SELECT 1
        FROM ratings AS r
        WHERE r.fk_orderitem = p_order_item_id
          AND r.cancelled IS NOT TRUE
    ) THEN
        OPEN p_result FOR
        SELECT -2 AS responsecode, FALSE AS statuscode,
               'You have already reviewed this item. You can edit your review instead.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    BEGIN
        INSERT INTO ratings (
            fk_product, fk_user, fk_order, fk_orderitem,
            ratingvalue, review, createdat, cancelled
        )
        VALUES (
            v_product_id, p_user_id, v_order_id, p_order_item_id,
            p_rating, v_review, NOW(), FALSE
        )
        RETURNING id_rating INTO v_new_id;
    EXCEPTION
        WHEN unique_violation THEN
            OPEN p_result FOR
            SELECT -2 AS responsecode, FALSE AS statuscode,
                   'You have already reviewed this item. You can edit your review instead.'::TEXT AS responsemsg;
            RETURN;
    END;

    OPEN p_result FOR
    SELECT v_new_id AS responsecode, TRUE AS statuscode,
           'Thank you! Your review has been submitted.'::TEXT AS responsemsg;
END;
$$;
