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
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Please log in to use the wishlist.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM products
        WHERE id_product = p_product_id
          AND COALESCE(cancelled, FALSE) = FALSE
          AND isactive = TRUE
    ) THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'This product is not available.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT id_wishlist
    INTO v_wishlist_id
    FROM wishlist
    WHERE fk_user = p_user_id
      AND COALESCE(cancelled, FALSE) = FALSE
    LIMIT 1;

    IF v_wishlist_id IS NULL THEN
        INSERT INTO wishlist (fk_user, createdat, cancelled)
        VALUES (p_user_id, NOW(), FALSE)
        RETURNING id_wishlist INTO v_wishlist_id;
    END IF;

    SELECT id_wishlistitem
    INTO v_existing_item_id
    FROM wishlistitems
    WHERE fk_wishlist = v_wishlist_id
      AND fk_product = p_product_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    IF v_existing_item_id IS NOT NULL THEN
        UPDATE wishlistitems
        SET cancelled = TRUE,
            cancelledon = NOW(),
            cancelledreason = 'Removed from wishlist'
        WHERE id_wishlistitem = v_existing_item_id;

        OPEN p_result FOR
        SELECT 0 AS responsecode, 1 AS statuscode,
               'Removed from wishlist.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    SELECT COUNT(1)
    INTO v_active_count
    FROM wishlistitems
    WHERE fk_wishlist = v_wishlist_id
      AND COALESCE(cancelled, FALSE) = FALSE;

    IF v_active_count >= 10 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Your wishlist can hold 10 products. Remove one before adding another.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    INSERT INTO wishlistitems (fk_wishlist, fk_product, createdat, cancelled)
    VALUES (v_wishlist_id, p_product_id, NOW(), FALSE)
    RETURNING id_wishlistitem INTO v_new_item_id;

    OPEN p_result FOR
    SELECT v_new_item_id AS responsecode, 1 AS statuscode,
           'Added to wishlist.'::TEXT AS responsemsg;
END;
$$;
