/**********************************************************************
Stored Procedure : get_order_item_reviews
Tables           : orders, orderitems, shipping, ratings

PURPOSE
  Review status of every item in ONE of the signed-in customer's orders
  (My Orders -> Order Details). The server is the authority on eligibility
  and on the 7-day review window; the browser only displays it.

CURSOR
  p_result : one row per order item
      iseligible          order is Delivered and not cancelled
      deliveredon         shipping.shippingdate of the Delivered shipping
                          row (falls back to orders.orderdate)
      reviewexpireson     deliveredon + 7 days
      reviewwindowexpired window has closed
      daysleft            whole days still left (0 when closed)
      reviewid / rating / review / reviewedon   the active review, if any
      canaddreview / caneditreview / candeletereview
      reviewstatus        NotEligible | CanReview | Reviewed | Locked | Expired

NOTES
  - p_user_id comes from the authenticated session, never from the client.
    Another customer's order simply returns no rows.
  - Window = 7 days from delivery (see also submit/update/delete procs).
**********************************************************************/

CREATE OR REPLACE PROCEDURE get_order_item_reviews(
    p_user_id      INT,
    p_order_id     INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_window CONSTANT INTERVAL := INTERVAL '7 days';
    v_now    TIMESTAMP := LOCALTIMESTAMP;
BEGIN
    OPEN p_result FOR
    WITH base AS (
        SELECT
            oi.id_orderitem,
            o.id_order,
            oi.fk_product,
            oi.productname,
            oi.variantlabel,
            oi.quantity,
            (COALESCE(o.cancelled, FALSE) = FALSE AND o.orderstatus = 'Delivered') AS isdelivered,
            COALESCE(
                (
                    SELECT MAX(s.shippingdate)
                    FROM shipping AS s
                    WHERE s.fk_order = o.id_order
                      AND COALESCE(s.cancelled, FALSE) = FALSE
                      AND s.shippingstatus = 'Delivered'
                ),
                o.orderdate
            ) AS deliveredon,
            r.id_rating,
            r.ratingvalue,
            r.review,
            r.createdat
        FROM orders AS o
        INNER JOIN orderitems AS oi ON oi.fk_order = o.id_order
        LEFT JOIN LATERAL (
            SELECT x.id_rating, x.ratingvalue, x.review, x.createdat
            FROM ratings AS x
            WHERE x.fk_orderitem = oi.id_orderitem
              AND x.fk_user = p_user_id
              AND x.cancelled IS NOT TRUE
            ORDER BY x.id_rating DESC
            LIMIT 1
        ) AS r ON TRUE
        WHERE o.id_order = p_order_id
          AND o.fk_user = p_user_id
    ),
    calc AS (
        SELECT
            b.*,
            CASE
                WHEN b.isdelivered AND b.deliveredon IS NOT NULL THEN b.deliveredon + v_window
            END AS expireson
        FROM base AS b
    )
    SELECT
        c.id_orderitem                                  AS orderitemid,
        c.id_order                                      AS orderid,
        c.fk_product                                    AS productid,
        c.productname                                   AS productname,
        COALESCE(c.variantlabel, '')                    AS variantlabel,
        c.quantity                                      AS quantity,
        c.isdelivered                                   AS iseligible,
        CASE WHEN c.isdelivered THEN c.deliveredon END  AS deliveredon,
        c.expireson                                     AS reviewexpireson,
        (c.expireson IS NOT NULL AND c.expireson <= v_now)           AS reviewwindowexpired,
        COALESCE(
            GREATEST(0, CEIL(EXTRACT(EPOCH FROM (c.expireson - v_now)) / 86400.0))::INT,
            0
        )                                               AS daysleft,
        COALESCE(c.id_rating, 0)                        AS reviewid,
        COALESCE(ROUND(c.ratingvalue)::INT, 0)          AS rating,
        COALESCE(c.review, '')                          AS review,
        c.createdat                                     AS reviewedon,
        (c.expireson IS NOT NULL AND c.expireson > v_now AND c.id_rating IS NULL)     AS canaddreview,
        (c.expireson IS NOT NULL AND c.expireson > v_now AND c.id_rating IS NOT NULL) AS caneditreview,
        (c.expireson IS NOT NULL AND c.expireson > v_now AND c.id_rating IS NOT NULL) AS candeletereview,
        CASE
            WHEN c.expireson IS NULL                          THEN 'NotEligible'
            WHEN c.id_rating IS NULL AND c.expireson > v_now  THEN 'CanReview'
            WHEN c.id_rating IS NULL                          THEN 'Expired'
            WHEN c.expireson > v_now                          THEN 'Reviewed'
            ELSE 'Locked'
        END                                             AS reviewstatus
    FROM calc AS c
    ORDER BY c.id_orderitem;
END;
$$;
