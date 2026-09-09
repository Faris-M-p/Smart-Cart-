/* =============================================================================
   Procedure : get_product_details
   Source    : GetProductDetails (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_product_details(
    p_slug          TEXT,
    INOUT p_result  refcursor DEFAULT 'p_result',
    INOUT p_result2 refcursor DEFAULT 'p_result2',
    INOUT p_result3 refcursor DEFAULT 'p_result3',
    INOUT p_result4 refcursor DEFAULT 'p_result4',
    INOUT p_result5 refcursor DEFAULT 'p_result5'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_product_id INT;
    v_slug_as_int INT;
BEGIN
    v_slug_as_int := CASE WHEN p_slug ~ '^\d+$' THEN p_slug::INT ELSE NULL END;

    IF v_slug_as_int IS NOT NULL AND v_slug_as_int > 0 THEN
        SELECT p.id_product
        INTO v_product_id
        FROM products AS p
        WHERE COALESCE(p.cancelled, FALSE) = FALSE
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE
          AND (p.id_product = v_slug_as_int OR p.slug = p_slug);
    ELSE
        SELECT p.id_product
        INTO v_product_id
        FROM products AS p
        WHERE COALESCE(p.cancelled, FALSE) = FALSE
          AND p.isactive = TRUE
          AND COALESCE(p.sellonline, FALSE) = TRUE
          AND p.slug = p_slug;
    END IF;

    OPEN p_result FOR
    SELECT
        p.id_product AS productid,
        p.name AS name,
        p.slug AS slug,
        COALESCE(p.description, '') AS description,
        c.id_category AS categoryid,
        c.name AS categoryname,
        p.fk_subcategory AS subcategoryid,
        sc.name AS subcategoryname,
        COALESCE(p.fk_brand, 0) AS brandid,
        COALESCE(b.brandname, '') AS brandname
    FROM products AS p
    INNER JOIN subcategory AS sc ON sc.id_subcategory = p.fk_subcategory
    INNER JOIN category AS c ON c.id_category = sc.fk_category
    LEFT JOIN brand AS b ON b.id_brand = p.fk_brand
    WHERE p.id_product = v_product_id;

    OPEN p_result2 FOR
    SELECT
        pm.mediaurl AS mediaurl
    FROM productmedia AS pm
    WHERE pm.fk_product = v_product_id
      AND pm.mediatype = 'Image'
      AND pm.mediaurl IS NOT NULL
      AND pm.mediaurl <> ''
    ORDER BY pm.isprimary DESC, pm.displayorder ASC, pm.id_productmedia ASC;

    OPEN p_result3 FOR
    SELECT
        pv.id_productvariant AS productvariantid,
        pv.sku AS sku,
        CASE WHEN COALESCE(pv.variantlabel, '') = '' THEN pv.sku ELSE pv.variantlabel END AS label,
        pv.sellingprice AS price,
        pv.mrp AS mrp,
        CASE WHEN COALESCE(st.qty, 0) > 0 THEN TRUE ELSE FALSE END AS instock,
        pv.isdefault AS isdefault
    FROM productvariants AS pv
    LEFT JOIN (
        SELECT fk_productvariant, SUM(quantity) AS qty
        FROM stock
        WHERE COALESCE(cancelled, FALSE) = FALSE
        GROUP BY fk_productvariant
    ) AS st ON st.fk_productvariant = pv.id_productvariant
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.isactive = TRUE
      AND COALESCE(pv.sellonline, FALSE) = TRUE
    ORDER BY pv.isdefault DESC, pv.sellingprice ASC, pv.id_productvariant ASC;

    OPEN p_result4 FOR
    SELECT
        pva.fk_productvariant AS productvariantid,
        v.id_variant AS variantid,
        v.name AS variantname,
        vv.id_variantvalue AS variantvalueid,
        vv.name AS variantvaluename
    FROM productvariantattributes AS pva
    INNER JOIN productvariants AS pv ON pv.id_productvariant = pva.fk_productvariant
    INNER JOIN variants AS v ON v.id_variant = pva.fk_variant
    INNER JOIN variantvalues AS vv ON vv.id_variantvalue = pva.fk_variantvalue
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.isactive = TRUE
      AND COALESCE(pv.sellonline, FALSE) = TRUE
    ORDER BY v.displayorder, v.name, vv.displayorder, vv.name;

    OPEN p_result5 FOR
    SELECT
        sm.fk_productsku AS productvariantid,
        sm.mediaurl AS imageurl
    FROM skumedia AS sm
    INNER JOIN productvariants AS pv ON pv.id_productvariant = sm.fk_productsku
    WHERE pv.fk_product = v_product_id
      AND COALESCE(pv.cancelled, FALSE) = FALSE
      AND pv.isactive = TRUE
      AND COALESCE(pv.sellonline, FALSE) = TRUE
      AND sm.mediatype = 'Image'
      AND sm.mediaurl IS NOT NULL
      AND sm.mediaurl <> ''
    ORDER BY sm.isprimary DESC, sm.displayorder ASC;
END;
$$;
