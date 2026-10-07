/**********************************************************************
Stored Procedure : update_order_item_review
Tables           : ratings, orders, orderitems, shipping

PURPOSE
  A customer edits their OWN order-item review (rating and text) — only
  while the 7-day review window (from delivery) is still open.

RESPONSE (p_result)
  responsecode > 0  : review id (success)
  responsecode = -1 : validation / not signed in
  responsecode = -4 : review not found
  responsecode = -5 : review / order belongs to another customer
  responsecode = -6 : review is locked (window over, or an older sample
                      review that is not linked to an order item)

NOTES
  - p_user_id is taken from the authenticated session, never from the client.
  - The original review date is kept (the table has no updated-on column).
**********************************************************************/

CREATE OR REPLACE PROCEDURE update_order_item_review(
    p_user_id      INT,
    p_review_id    INT,
    p_rating       INT,
    p_review       TEXT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_window       CONSTANT INTERVAL := INTERVAL '7 days';
    v_now          TIMESTAMP := LOCALTIMESTAMP;
    v_review       TEXT := NULLIF(BTRIM(COALESCE(p_review, '')), '');
    v_found        BOOLEAN := FALSE;
    v_owner_id     INT;
    v_item_id      INT;
    v_order_owner  INT;
    v_status       TEXT;
    v_cancelled    BOOLEAN;
    v_delivered_on TIMESTAMP;
BEGIN
    IF COALESCE(p_user_id, 0) < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, FALSE AS statuscode,
               'Please log in to edit your review.'::TEXT AS responsemsg;
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

    SELECT TRUE,
           r.fk_user,
           r.fk_orderitem,
           o.fk_user,
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
    INTO v_found, v_owner_id, v_item_id, v_order_owner, v_status, v_cancelled, v_delivered_on
    FROM ratings AS r
    LEFT JOIN orderitems AS oi ON oi.id_orderitem = r.fk_orderitem
    LEFT JOIN orders AS o ON o.id_order = oi.fk_order
    WHERE r.id_rating = p_review_id
      AND r.cancelled IS NOT TRUE
    FOR UPDATE OF r;

    IF NOT COALESCE(v_found, FALSE) THEN
        OPEN p_result FOR
        SELECT -4 AS responsecode, FALSE AS statuscode,
               'Review not found.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_owner_id IS DISTINCT FROM p_user_id THEN
        OPEN p_result FOR
        SELECT -5 AS responsecode, FALSE AS statuscode,
               'You can only change your own review.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_item_id IS NULL THEN
        OPEN p_result FOR
        SELECT -6 AS responsecode, FALSE AS statuscode,
               'This review is locked and can no longer be changed.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_order_owner IS DISTINCT FROM p_user_id THEN
        OPEN p_result FOR
        SELECT -5 AS responsecode, FALSE AS statuscode,
               'You can only change your own review.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF v_cancelled OR v_status <> 'Delivered'
       OR v_delivered_on IS NULL OR v_delivered_on + v_window <= v_now THEN
        OPEN p_result FOR
        SELECT -6 AS responsecode, FALSE AS statuscode,
               'The review period for this item has ended. Your review is locked.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    UPDATE ratings
    SET ratingvalue = p_rating,
        review = v_review
    WHERE id_rating = p_review_id
      AND fk_user = p_user_id
      AND cancelled IS NOT TRUE;

    OPEN p_result FOR
    SELECT p_review_id AS responsecode, TRUE AS statuscode,
           'Your review has been updated.'::TEXT AS responsemsg;
END;
$$;
