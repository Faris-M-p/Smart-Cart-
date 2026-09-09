/* =============================================================================
   Procedure : get_checkout_preview
   Source    : GetCheckoutPreview (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_checkout_preview(
    p_user_id            INT,
    p_product_variant_id INT DEFAULT 0,
    p_quantity           INT DEFAULT 1,
    INOUT p_result       refcursor DEFAULT 'p_result',
    INOUT p_result2      refcursor DEFAULT 'p_result2',
    INOUT p_result3      refcursor DEFAULT 'p_result3'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF COALESCE(p_quantity, 0) < 1 THEN
        p_quantity := 1;
    END IF;

    DROP TABLE IF EXISTS tmp_checkout_lines;
    CREATE TEMP TABLE tmp_checkout_lines (
        productid         INT NOT NULL,
        productvariantid INT NOT NULL,
        name               TEXT NOT NULL,
        slug               TEXT NULL,
        label              TEXT NULL,
        sku                TEXT NULL,
        price              NUMERIC(10, 2) NOT NULL,
        mrp                NUMERIC(10, 2) NOT NULL,
        quantity           INT NOT NULL,
        linetotal         NUMERIC(10, 2) NOT NULL,
        stock_quantity     INT NOT NULL,
        in_stock           BOOLEAN NOT NULL,
        imageurl          TEXT NULL
    ) ON COMMIT DROP;

    IF COALESCE(p_product_variant_id, 0) > 0 THEN
        INSERT INTO tmp_checkout_lines
        SELECT
            p.id_product,
            pv.id_productvariant,
            p.name,
            p.slug,
            CASE
                WHEN COALESCE(pv.variantlabel, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variantlabel
            END,
            COALESCE(pv.sku, ''),
            pv.sellingprice,
            COALESCE(pv.mrp, pv.sellingprice),
            p_quantity,
            pv.sellingprice * p_quantity,
            COALESCE(st.qty, 0),
            CASE
                WHEN COALESCE(p.cancelled, FALSE) = FALSE
                 AND p.isactive = TRUE
                 AND COALESCE(p.sellonline, FALSE) = TRUE
                 AND COALESCE(pv.cancelled, FALSE) = FALSE
                 AND pv.isactive = TRUE
                 AND COALESCE(pv.sellonline, FALSE) = TRUE
                 AND COALESCE(st.qty, 0) >= p_quantity
                THEN TRUE ELSE FALSE
            END,
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
            ))
        FROM productvariants AS pv
        INNER JOIN products AS p ON p.id_product = pv.fk_product
        LEFT JOIN (
            SELECT fk_productvariant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_productvariant
        ) AS st ON st.fk_productvariant = pv.id_productvariant
        WHERE pv.id_productvariant = p_product_variant_id;
    ELSE
        INSERT INTO tmp_checkout_lines
        SELECT
            ci.fk_product,
            COALESCE(ci.fk_productvariant, 0),
            p.name,
            p.slug,
            CASE
                WHEN COALESCE(pv.variantlabel, '') = '' THEN COALESCE(pv.sku, '')
                ELSE pv.variantlabel
            END,
            COALESCE(pv.sku, ''),
            COALESCE(pv.sellingprice, ci.price),
            COALESCE(pv.mrp, ci.price),
            ci.quantity,
            COALESCE(pv.sellingprice, ci.price) * ci.quantity,
            COALESCE(st.qty, 0),
            CASE
                WHEN COALESCE(p.cancelled, FALSE) = FALSE
                 AND p.isactive = TRUE
                 AND COALESCE(p.sellonline, FALSE) = TRUE
                 AND pv.id_productvariant IS NOT NULL
                 AND COALESCE(pv.cancelled, FALSE) = FALSE
                 AND pv.isactive = TRUE
                 AND COALESCE(pv.sellonline, FALSE) = TRUE
                 AND COALESCE(st.qty, 0) >= ci.quantity
                THEN TRUE ELSE FALSE
            END,
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
            ))
        FROM cartitems AS ci
        INNER JOIN cart AS c ON c.id_cart = ci.fk_cart
        INNER JOIN products AS p ON p.id_product = ci.fk_product
        LEFT JOIN productvariants AS pv
            ON pv.id_productvariant = ci.fk_productvariant
        LEFT JOIN (
            SELECT fk_productvariant, SUM(quantity) AS qty
            FROM stock
            WHERE COALESCE(cancelled, FALSE) = FALSE
            GROUP BY fk_productvariant
        ) AS st ON st.fk_productvariant = pv.id_productvariant
        WHERE c.fk_user = p_user_id;
    END IF;

    OPEN p_result FOR
    SELECT
        productid AS productid,
        productvariantid AS productvariantid,
        name AS name,
        slug AS slug,
        label AS label,
        sku AS sku,
        price AS price,
        mrp AS mrp,
        quantity AS quantity,
        linetotal AS linetotal,
        stock_quantity AS stockquantity,
        in_stock AS instock,
        imageurl AS imageurl
    FROM tmp_checkout_lines
    ORDER BY name;

    OPEN p_result2 FOR
    SELECT
        COALESCE(SUM(quantity), 0) AS totalquantity,
        COALESCE(SUM(linetotal), 0) AS subtotal,
        COALESCE(COUNT(1), 0) AS itemcount,
        CASE
            WHEN COUNT(1) > 0 AND MIN(CASE WHEN in_stock THEN 1 ELSE 0 END) = 1
            THEN TRUE ELSE FALSE
        END AS canplace
    FROM tmp_checkout_lines;

    OPEN p_result3 FOR
    SELECT
        COALESCE(u.fullname, u.username) AS fullname,
        COALESCE(u.phonenumber, '') AS phone,
        u.email AS email
    FROM users AS u
    WHERE u.id_user = p_user_id
      AND COALESCE(u.cancelled, FALSE) = FALSE;
END;
$$;
