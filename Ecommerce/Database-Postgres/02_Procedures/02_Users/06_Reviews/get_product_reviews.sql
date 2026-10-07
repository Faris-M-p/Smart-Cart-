/**********************************************************************
Stored Procedure : get_product_reviews
Table            : ratings (reviews are written from My Orders -> Order Details)

PURPOSE
  Read-only ratings & reviews for one product (Product Details page).

CURSORS
  p_result  : summary      (averagerating, totalratings, totalreviews)
                           - no rows when the product is not visible in the shop
  p_result2 : distribution (stars 5..1, ratingcount, percentage)
  p_result3 : review page  (current customer's own review first, then newest)

NOTES
  - p_user_id comes from the authenticated session (0 = guest). It is only
    used to pin the viewer's own review and to link it back to its order.
  - isverifiedpurchase = the review is attached to an order item
    (ratings.fk_orderitem). Older sample reviews have no order item, so they
    are shown as normal product reviews WITHOUT the Verified Purchase badge.
  - Reviewer names are shortened to "First L." for privacy.
**********************************************************************/

CREATE OR REPLACE PROCEDURE get_product_reviews(
    p_product_id    INT,
    p_user_id       INT,
    p_page_index    INT,
    p_page_size     INT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2',
    INOUT p_result3 refcursor DEFAULT 'p_result3'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_id    INT := COALESCE(p_user_id, 0);
    v_page_index INT := GREATEST(COALESCE(p_page_index, 1), 1);
    v_page_size  INT := LEAST(GREATEST(COALESCE(p_page_size, 10), 1), 50);
    v_visible    BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM products AS p
        WHERE p.id_product = p_product_id
          AND COALESCE(p.cancelled, FALSE) = FALSE
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE
    )
    INTO v_visible;

    -- 1) Summary (computed from real, non-cancelled ratings)
    OPEN p_result FOR
    SELECT
        p.id_product AS productid,
        COALESCE(ROUND(AVG(r.ratingvalue), 1), 0)::NUMERIC(3, 1) AS averagerating,
        COUNT(r.id_rating)::INT AS totalratings,
        (COUNT(r.id_rating) FILTER (WHERE NULLIF(BTRIM(r.review), '') IS NOT NULL))::INT AS totalreviews
    FROM products AS p
    LEFT JOIN ratings AS r
           ON r.fk_product = p.id_product
          AND COALESCE(r.cancelled, FALSE) = FALSE
    WHERE p.id_product = p_product_id
      AND v_visible
    GROUP BY p.id_product;

    -- 2) 5..1 star distribution
    OPEN p_result2 FOR
    WITH counts AS (
        SELECT
            s.stars,
            COUNT(r.id_rating)::INT AS ratingcount
        FROM generate_series(5, 1, -1) AS s(stars)
        LEFT JOIN ratings AS r
               ON r.fk_product = p_product_id
              AND COALESCE(r.cancelled, FALSE) = FALSE
              AND ROUND(r.ratingvalue)::INT = s.stars
        GROUP BY s.stars
    )
    SELECT
        c.stars AS stars,
        c.ratingcount AS ratingcount,
        CASE
            WHEN SUM(c.ratingcount) OVER () = 0 THEN 0
            ELSE ROUND(100.0 * c.ratingcount / SUM(c.ratingcount) OVER ())::INT
        END AS percentage
    FROM counts AS c
    WHERE v_visible
    ORDER BY c.stars DESC;

    -- 3) One page of reviews (own review pinned first, then newest)
    OPEN p_result3 FOR
    SELECT
        r.id_rating AS reviewid,
        COALESCE(
            NULLIF(
                CASE
                    WHEN NULLIF(BTRIM(u.fullname), '') IS NULL THEN NULL
                    WHEN POSITION(' ' IN BTRIM(u.fullname)) = 0 THEN BTRIM(u.fullname)
                    ELSE SPLIT_PART(BTRIM(u.fullname), ' ', 1) || ' '
                         || UPPER(LEFT(REGEXP_REPLACE(BTRIM(u.fullname), '^.*\s', ''), 1)) || '.'
                END,
                ''
            ),
            NULLIF(BTRIM(u.username), ''),
            'Customer'
        ) AS reviewername,
        ROUND(r.ratingvalue)::INT AS rating,
        COALESCE(r.review, '') AS review,
        r.createdat AS createdat,
        (r.fk_orderitem IS NOT NULL) AS isverifiedpurchase,
        (v_user_id > 0 AND r.fk_user = v_user_id) AS ismine,
        CASE
            WHEN v_user_id > 0 AND r.fk_user = v_user_id THEN COALESCE(r.fk_order, 0)
            ELSE 0
        END AS myorderid
    FROM ratings AS r
    INNER JOIN users AS u ON u.id_user = r.fk_user
    WHERE r.fk_product = p_product_id
      AND v_visible
      AND COALESCE(r.cancelled, FALSE) = FALSE
    ORDER BY (v_user_id > 0 AND r.fk_user = v_user_id) DESC,
             r.createdat DESC NULLS LAST,
             r.id_rating DESC
    LIMIT v_page_size
    OFFSET (v_page_index - 1) * v_page_size;
END;
$$;
