/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Insert / Update Product Master With Validation
Source      : ProProductUpdate (SQL Server)
------------------------------------------------------------------------*/
CREATE OR REPLACE PROCEDURE pro_product_update(
    IN p_user_action INT,              -- 1 = Insert, 2 = Update
    IN p_id_product INT DEFAULT 0,
    IN p_name TEXT DEFAULT NULL,
    IN p_description TEXT DEFAULT '',
    IN p_price NUMERIC(10,2) DEFAULT NULL,
    IN p_mrp NUMERIC(10,2) DEFAULT NULL,
    IN p_fk_category INT DEFAULT NULL,
    IN p_fk_subcategory INT DEFAULT NULL,
    IN p_fk_brand INT DEFAULT NULL,
    IN p_rating NUMERIC(3,1) DEFAULT NULL,
    IN p_gender TEXT DEFAULT NULL,
    IN p_fk_status INT DEFAULT 1,
    IN p_enter_by INT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_date TIMESTAMP := NOW();
    v_is_duplicate INT := 0;
    v_id_product INT := COALESCE(p_id_product, 0);
    v_slug TEXT;
    v_slug_base TEXT;
    v_slug_n INT := 0;
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (TRIM(COALESCE(p_name, '')) = '') THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Product name is required.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF (p_price <= 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Price must be greater than zero.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF (p_mrp < p_price) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'MRP must be >= Price.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM category
        WHERE id_category = p_fk_category AND cancelled = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid Category.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM subcategory
        WHERE id_subcategory = p_fk_subcategory
          AND cancelled = FALSE
          AND fk_category = p_fk_category
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid SubCategory.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK: Same name in same category/subcategory
    -------------------------------------------------------------------
    SELECT COUNT(*) INTO v_is_duplicate
    FROM products p
    INNER JOIN subcategory sc ON sc.id_subcategory = p.fk_subcategory
    WHERE p.name = p_name
      AND sc.fk_category = p_fk_category
      AND p.fk_subcategory = p_fk_subcategory
      AND p.id_product <> v_id_product
      AND p.cancelled = FALSE;

    IF (v_is_duplicate > 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Duplicate product exists.' AS response_msg, FALSE AS status_code;
        RETURN;
    END IF;

    -- slug from name (products.slug is NOT NULL / UNIQUE)
    v_slug_base := LOWER(TRIM(BOTH '-' FROM regexp_replace(p_name, '[^a-zA-Z0-9]+', '-', 'g')));
    IF (v_slug_base = '') THEN
        v_slug_base := 'product';
    END IF;
    v_slug := v_slug_base;

    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        WHILE EXISTS (SELECT 1 FROM products WHERE slug = v_slug) LOOP
            v_slug_n := v_slug_n + 1;
            v_slug := v_slug_base || '-' || v_slug_n::TEXT;
        END LOOP;

        INSERT INTO products (
            fk_subcategory, fk_brand, name, slug, description,
            isactive, sellonline, createdat, modifiedat,
            cancelled, cancelledon
        )
        VALUES (
            p_fk_subcategory, p_fk_brand, p_name, v_slug, p_description,
            COALESCE(p_fk_status, 1) = 1, FALSE, v_user_date, NULL,
            FALSE, NULL
        )
        RETURNING id_product INTO v_id_product;

        -- p_price / p_mrp / p_rating / p_gender / p_enter_by: accepted for API parity;
        -- price lives on productvariants; products has no rating/gender/enterby.
        OPEN p_result FOR
            SELECT v_id_product AS response_code,
                   'Product created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM products WHERE id_product = v_id_product) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid Product ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (SELECT 1 FROM products WHERE id_product = v_id_product AND cancelled = TRUE) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Product is deleted.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        WHILE EXISTS (
            SELECT 1 FROM products WHERE slug = v_slug AND id_product <> v_id_product
        ) LOOP
            v_slug_n := v_slug_n + 1;
            v_slug := v_slug_base || '-' || v_slug_n::TEXT;
        END LOOP;

        UPDATE products
        SET name = p_name,
            description = p_description,
            fk_subcategory = p_fk_subcategory,
            fk_brand = p_fk_brand,
            slug = v_slug,
            isactive = COALESCE(p_fk_status, 1) = 1,
            modifiedat = v_user_date
        WHERE id_product = v_id_product;

        OPEN p_result FOR
            SELECT v_id_product AS response_code,
                   'Product updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    OPEN p_result FOR
        SELECT -1 AS response_code, 'Invalid UserAction.' AS response_msg, FALSE AS status_code;

EXCEPTION
    WHEN OTHERS THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, SQLERRM AS response_msg, FALSE AS status_code;
END;
$$;
