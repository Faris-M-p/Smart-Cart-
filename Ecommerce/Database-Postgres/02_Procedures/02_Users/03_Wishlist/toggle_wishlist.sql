/* =============================================================================
   Procedure : toggle_wishlist
   Source    : ToggleWishlist (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE toggle_wishlist(
    p_user_id      INT,
    p_product_id   INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_wishlist_id      INT;
    v_existing_item_id INT;
    v_active_count     INT;
    v_new_item_id      INT;
BEGIN
    IF COALESCE(p_user_id, 0) < 1 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Please log in to use the wishlist.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM products
        WHERE id_product = p_product_id
          AND COALESCE(cancelled, FALSE) = FALSE
          AND is_active = TRUE
    ) THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'This product is not available.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT wishlist_id
    INTO v_wishlist_id
    FROM wishlist
    WHERE user_id = p_user_id
      AND COALESCE(cancelled, FALSE) = FALSE
    LIMIT 1;

    IF v_wishlist_id IS NULL THEN
        INSERT INTO wishlist (user_id, created_at, cancelled)
        VALUES (p_user_id, NOW(), FALSE)
        RETURNING wishlist_id INTO v_wishlist_id;
    END IF;

    SELECT wishlist_item_id
    INTO v_existing_item_id
    FROM wishlist_items
    WHERE wishlist_id = v_wishlist_id
      AND product_id = p_product_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    IF v_existing_item_id IS NOT NULL THEN
        UPDATE wishlist_items
        SET cancelled = TRUE,
            cancelled_on = NOW(),
            cancelled_reason = 'Removed from wishlist'
        WHERE wishlist_item_id = v_existing_item_id;

        OPEN p_result FOR
        SELECT 0 AS "ResponseCode", 1 AS "StatusCode",
               'Removed from wishlist.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    SELECT COUNT(1)
    INTO v_active_count
    FROM wishlist_items
    WHERE wishlist_id = v_wishlist_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    IF v_active_count >= 10 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Your wishlist can hold 10 products. Remove one before adding another.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    INSERT INTO wishlist_items (wishlist_id, product_id, created_at, cancelled)
    VALUES (v_wishlist_id, p_product_id, NOW(), FALSE)
    RETURNING wishlist_item_id INTO v_new_item_id;

    OPEN p_result FOR
    SELECT v_new_item_id AS "ResponseCode", 1 AS "StatusCode",
           'Added to wishlist.'::TEXT AS "ResponseMsg";
END;
$$;
