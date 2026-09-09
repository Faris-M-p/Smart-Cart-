/**********************************************************************
Stored Procedure : pro_product_variant_upsert
Created By       : Muhammed Faris
Created On       : 09/12/2025
Source           : ProProductVariantUpsert (SQL Server)

p_user_action: 1=Insert, 2=Update, 3=Delete (soft)
p_variant_attributes JSON: [{"FK_Variant":1,"FK_VariantValue":21}, ...]
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_upsert(
    IN p_user_action INT,
    IN p_id_product_variant INT DEFAULT 0,
    IN p_fk_product INT DEFAULT 0,
    IN p_price_adjustment NUMERIC(10,2) DEFAULT 0,
    IN p_is_default BOOLEAN DEFAULT FALSE,
    IN p_variant_attributes TEXT DEFAULT NULL,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id INT := COALESCE(p_id_product_variant, 0);
    v_fk_product INT := COALESCE(p_fk_product, 0);
    v_incoming_sig TEXT := NULL;
    v_dup_existing_id INT := NULL;
    v_attr_count INT := 0;
    v_sku TEXT;
    v_label TEXT;
    v_price NUMERIC(10,2);
BEGIN
    CREATE TEMP TABLE IF NOT EXISTS tmp_variant_attrs (
        fk_variant INT NOT NULL,
        fk_variant_value INT NOT NULL
    ) ON COMMIT DROP;

    DELETE FROM tmp_variant_attrs;

    -------------------------------------------------------------------
    -- BASIC VALIDATIONS (Product existence)
    -------------------------------------------------------------------
    IF (p_user_action IN (1, 2)) THEN
        IF (p_user_action = 1 AND v_fk_product = 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'FK_Product is required for insert.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        IF (p_user_action = 2) THEN
            IF (v_id = 0) THEN
                OPEN p_result FOR
                    SELECT -1 AS response_code,
                           'ID_ProductVariant is required for update.' AS response_msg,
                           FALSE AS status_code;
                RETURN;
            END IF;

            IF NOT EXISTS (
                SELECT 1 FROM product_variants WHERE id_product_variant = v_id
            ) THEN
                OPEN p_result FOR
                    SELECT -1 AS response_code,
                           'Invalid ProductVariant ID.' AS response_msg,
                           FALSE AS status_code;
                RETURN;
            END IF;

            IF (v_fk_product = 0) THEN
                SELECT fk_product INTO v_fk_product
                FROM product_variants
                WHERE id_product_variant = v_id;
            END IF;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM products WHERE id_product = v_fk_product AND cancelled = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'Invalid or deleted Product.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;
    END IF;

    -------------------------------------------------------------------
    -- Parse VariantAttributes JSON
    -------------------------------------------------------------------
    IF (p_variant_attributes IS NOT NULL AND TRIM(p_variant_attributes) <> '') THEN
        INSERT INTO tmp_variant_attrs (fk_variant, fk_variant_value)
        SELECT
            CASE WHEN (elem->>'FK_Variant') ~ '^\d+$' THEN (elem->>'FK_Variant')::INT ELSE NULL END,
            CASE WHEN (elem->>'FK_VariantValue') ~ '^\d+$' THEN (elem->>'FK_VariantValue')::INT ELSE NULL END
        FROM jsonb_array_elements(p_variant_attributes::jsonb) AS elem
        WHERE (elem->>'FK_Variant') ~ '^\d+$'
          AND (elem->>'FK_VariantValue') ~ '^\d+$';

        SELECT COUNT(*) INTO v_attr_count FROM tmp_variant_attrs;

        IF (p_user_action = 1 AND v_attr_count = 0) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'At least one Variant attribute is required in VariantAttributes JSON for insert.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        IF EXISTS (
            SELECT 1
            FROM tmp_variant_attrs a
            LEFT JOIN variants v ON v.id_variant = a.fk_variant
            LEFT JOIN variant_values vv ON vv.id_variant_value = a.fk_variant_value
            WHERE v.id_variant IS NULL
               OR vv.id_variant_value IS NULL
               OR vv.fk_variant <> a.fk_variant
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'One or more Variant/Value pairs are invalid or mismatched.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;
    ELSE
        IF (p_user_action = 1) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'VariantAttributes JSON is required for insert (at least one attribute).' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;
    END IF;

    -------------------------------------------------------------------
    -- Incoming attribute signature (ordered): "1:21,2:33"
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM tmp_variant_attrs) THEN
        SELECT string_agg(a.fk_variant::TEXT || ':' || a.fk_variant_value::TEXT, ',' ORDER BY a.fk_variant)
        INTO v_incoming_sig
        FROM tmp_variant_attrs a;
    END IF;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (same product, same attribute signature)
    -------------------------------------------------------------------
    IF (v_incoming_sig IS NOT NULL) THEN
        SELECT pv.id_product_variant
        INTO v_dup_existing_id
        FROM product_variants pv
        WHERE pv.fk_product = v_fk_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND (
                SELECT string_agg(
                    pva.fk_variant::TEXT || ':' || pva.fk_variant_value::TEXT,
                    ',' ORDER BY pva.fk_variant
                )
                FROM product_variant_attributes pva
                WHERE pva.fk_product_variant = pv.id_product_variant
              ) = v_incoming_sig
        LIMIT 1;
    END IF;

    v_price := COALESCE(p_price_adjustment, 0);

    -------------------------------------------------------------------
    -- INSERT (CREATE SKU)
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF (v_dup_existing_id IS NOT NULL) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'Duplicate SKU exists (ProductVariant ID: ' || v_dup_existing_id || ').' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        IF (COALESCE(p_is_default, FALSE)) THEN
            UPDATE product_variants
            SET is_default = FALSE
            WHERE fk_product = v_fk_product AND COALESCE(cancelled, FALSE) = FALSE;
        END IF;

        SELECT string_agg(v.name || ':' || vv.name, ' | ' ORDER BY a.fk_variant)
        INTO v_label
        FROM tmp_variant_attrs a
        INNER JOIN variants v ON v.id_variant = a.fk_variant
        INNER JOIN variant_values vv ON vv.id_variant_value = a.fk_variant_value;

        v_label := COALESCE(NULLIF(TRIM(v_label), ''), 'SKU');
        v_sku := 'SKU-' || v_fk_product::TEXT || '-' || substr(md5(v_incoming_sig || clock_timestamp()::TEXT), 1, 10);

        INSERT INTO product_variants (
            fk_product, sku, barcode, variant_label, description,
            mrp, selling_price, is_default, is_active, sell_online,
            created_at, cancelled, cancelled_on
        )
        VALUES (
            v_fk_product, v_sku, NULL, v_label, NULL,
            v_price, v_price, COALESCE(p_is_default, FALSE), TRUE, FALSE,
            v_now, FALSE, NULL
        )
        RETURNING id_product_variant INTO v_id;

        INSERT INTO product_variant_attributes (fk_product_variant, fk_variant, fk_variant_value)
        SELECT v_id, fk_variant, fk_variant_value
        FROM tmp_variant_attrs;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'ProductVariant created successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF NOT EXISTS (SELECT 1 FROM product_variants WHERE id_product_variant = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid ProductVariant ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        IF (v_incoming_sig IS NOT NULL AND v_dup_existing_id IS NOT NULL AND v_dup_existing_id <> v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'Duplicate SKU exists (ProductVariant ID: ' || v_dup_existing_id || ').' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        IF (COALESCE(p_is_default, FALSE)) THEN
            UPDATE product_variants
            SET is_default = FALSE
            WHERE fk_product = v_fk_product AND COALESCE(cancelled, FALSE) = FALSE;
        END IF;

        UPDATE product_variants
        SET selling_price = v_price,
            mrp = v_price,
            is_default = COALESCE(p_is_default, FALSE)
        WHERE id_product_variant = v_id;

        IF (v_incoming_sig IS NOT NULL) THEN
            DELETE FROM product_variant_attributes WHERE fk_product_variant = v_id;

            INSERT INTO product_variant_attributes (fk_product_variant, fk_variant, fk_variant_value)
            SELECT v_id, fk_variant, fk_variant_value
            FROM tmp_variant_attrs;

            SELECT string_agg(v.name || ':' || vv.name, ' | ' ORDER BY a.fk_variant)
            INTO v_label
            FROM tmp_variant_attrs a
            INNER JOIN variants v ON v.id_variant = a.fk_variant
            INNER JOIN variant_values vv ON vv.id_variant_value = a.fk_variant_value;

            IF (v_label IS NOT NULL) THEN
                UPDATE product_variants SET variant_label = v_label WHERE id_product_variant = v_id;
            END IF;
        END IF;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'ProductVariant updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DELETE (SOFT)
    -------------------------------------------------------------------
    IF (p_user_action = 3) THEN
        IF NOT EXISTS (SELECT 1 FROM product_variants WHERE id_product_variant = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid ProductVariant ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        -- product_variants has cancelled / cancelled_on only (no cancelled_reason / cancelled_by)
        UPDATE product_variants
        SET cancelled = TRUE,
            cancelled_on = v_now,
            is_active = FALSE,
            is_default = FALSE
        WHERE id_product_variant = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'ProductVariant deleted (soft) successfully.' AS response_msg,
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
