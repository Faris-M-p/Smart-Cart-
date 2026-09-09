/* =============================================================================
   Procedure : get_cart
   Source    : GetCart (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_cart(
    p_user_id       INT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_cart_id INT;
BEGIN
    SELECT c.id_cart
    INTO v_cart_id
    FROM cart AS c
    WHERE c.fk_user = p_user_id
    LIMIT 1;

    OPEN p_result FOR
    SELECT
        ci.id_cartitem AS cartitemid,
        ci.fk_product AS productid,
        COALESCE(ci.fk_productvariant, 0) AS productvariantid,
        p.name AS name,
        p.slug AS slug,
        CASE
            WHEN COALESCE(pv.variantlabel, '') = '' THEN COALESCE(pv.sku, '')
            ELSE pv.variantlabel
        END AS label,
        COALESCE(pv.sku, '') AS sku,
        COALESCE(pv.sellingprice, ci.price) AS price,
        COALESCE(pv.mrp, ci.price) AS mrp,
        ci.quantity AS quantity,
        COALESCE(pv.sellingprice, ci.price) * ci.quantity AS linetotal,
        COALESCE(st.qty, 0) AS stockquantity,
        CASE
            WHEN COALESCE(p.cancelled, FALSE) = FALSE
             AND p.isactive = TRUE
             AND pv.id_productvariant IS NOT NULL
             AND COALESCE(pv.cancelled, FALSE) = FALSE
             AND pv.isactive = TRUE
             AND COALESCE(st.qty, 0) > 0
            THEN TRUE ELSE FALSE
        END AS instock,
        COALESCE((
            SELECT sm.mediaurl
            FROM skumedia AS sm
            WHERE sm.fk_productsku = pv.id_productvariant
              AND sm.mediaurl IS NOT NULL
              AND sm.mediaurl <> ''
            ORDER BY sm.isprimary DESC, sm.displayorder ASC
            LIMIT 1
        ), (
            SELECT pm.mediaurl
            FROM productmedia AS pm
            WHERE pm.fk_product = p.id_product
              AND pm.mediatype = 'Image'
              AND pm.mediaurl IS NOT NULL
              AND pm.mediaurl <> ''
            ORDER BY pm.isprimary DESC, pm.displayorder ASC
            LIMIT 1
        )) AS imageurl
    FROM cartitems AS ci
    INNER JOIN products AS p ON p.id_product = ci.fk_product
    LEFT JOIN productvariants AS pv
        ON pv.id_productvariant = ci.fk_productvariant
    LEFT JOIN (
        SELECT fk_productvariant, SUM(quantity) AS qty
        FROM stock
        WHERE COALESCE(cancelled, FALSE) = FALSE
        GROUP BY fk_productvariant
    ) AS st ON st.fk_productvariant = pv.id_productvariant
    WHERE ci.fk_cart = v_cart_id
    ORDER BY ci.id_cartitem DESC;

    OPEN p_result2 FOR
    SELECT
        COALESCE(SUM(ci.quantity), 0) AS totalquantity,
        COALESCE(SUM(COALESCE(pv.sellingprice, ci.price) * ci.quantity), 0) AS subtotal,
        COALESCE(COUNT(1), 0) AS itemcount
    FROM cartitems AS ci
    LEFT JOIN productvariants AS pv
        ON pv.id_productvariant = ci.fk_productvariant
    WHERE ci.fk_cart = v_cart_id;
END;
$$;
