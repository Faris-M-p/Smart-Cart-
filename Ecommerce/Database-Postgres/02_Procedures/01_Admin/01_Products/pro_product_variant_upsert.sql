/**********************************************************************
Stored Procedure : pro_product_variant_upsert
Created By       : Muhammed Faris
Created On       : 09/12/2025
Source           : ProProductVariantUpsert (SQL Server)

p_user_action: 1=Insert, 2=Update, 3=Delete (soft)
p_variant_attributes JSON: [{fk_variant:1,fk_variantvalue:21}, ...]
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
        fk_variantvalue INT NOT NULL
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
                SELECT 1 FROM productvariants WHERE id_productvariant = v_id
            ) THEN
                OPEN p_result FOR
                    SELECT -1 AS response_code,
                           'Invalid ProductVariant ID.' AS response_msg,
                           FALSE AS status_code;
                RETURN;
            END IF;

            IF (v_fk_product = 0) THEN
                SELECT fk_product INTO v_fk_product
                FROM productvariants
                WHERE id_productvariant = v_id;
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
        INSERT INTO tmp_variant_attrs (fk_variant, fk_variantvalue)
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
            LEFT JOIN variantvalues vv ON vv.id_variantvalue = a.fk_variantvalue
            WHERE v.id_variant IS NULL
               OR vv.id_variantvalue IS NULL
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
    -- Incoming attribute signature (ordered): 1:21,2:33
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM tmp_variant_attrs) THEN
        SELECT string_agg(a.fk_variant::TEXT || ':' || a.fk_variantvalue::TEXT, ',' ORDER BY a.fk_variant)
        INTO v_incoming_sig
        FROM tmp_variant_attrs a;
    END IF;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (same product, same attribute signature)
    -------------------------------------------------------------------
    IF (v_incoming_sig IS NOT NULL) THEN
        SELECT pv.id_productvariant
        INTO v_dup_existing_id
        FROM productvariants pv
        WHERE pv.fk_product = v_fk_product
          AND COALESCE(pv.cancelled, FALSE) = FALSE
          AND (
                SELECT string_agg(
                    pva.fk_variant::TEXT || ':' || pva.fk_variantvalue::TEXT,
                    ',' ORDER BY pva.fk_variant
                )
                FROM productvariantattributes pva
                WHERE pva.fk_productvariant = pv.id_productvariant
              ) = v_incoming_sig
        LIMIT 1;
    END IF;

    v_price := COALESCE(p_price_adjustment, 0);

    -------------------------------------------------------------------
    -- INSERT (CREATE sku)
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
            UPDATE productvariants
            SET isdefault = FALSE
            WHERE fk_product = v_fk_product AND COALESCE(cancelled, FALSE) = FALSE;
        END IF;

        SELECT string_agg(v.name || ':' || vv.name, ' | ' ORDER BY a.fk_variant)
        INTO v_label
        FROM tmp_variant_attrs a
        INNER JOIN variants v ON v.id_variant = a.fk_variant
        INNER JOIN variantvalues vv ON vv.id_variantvalue = a.fk_variantvalue;

        v_label := COALESCE(NULLIF(TRIM(v_label), ''), 'SKU');
        v_sku := 'SKU-' || v_fk_product::TEXT || '-' || substr(md5(v_incoming_sig || clock_timestamp()::TEXT), 1, 10);

        INSERT INTO productvariants (
            fk_product, sku, barcode, variantlabel, description,
            mrp, sellingprice, isdefault, isactive, sellonline,
            createdat, cancelled, cancelledon
        )
        VALUES (
            v_fk_product, v_sku, NULL, v_label, NULL,
            v_price, v_price, COALESCE(p_is_default, FALSE), TRUE, FALSE,
            v_now, FALSE, NULL
        )
        RETURNING id_productvariant INTO v_id;

        INSERT INTO productvariantattributes (fk_productvariant, fk_variant, fk_variantvalue)
        SELECT v_id, fk_variant, fk_variantvalue
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
        IF NOT EXISTS (SELECT 1 FROM productvariants WHERE id_productvariant = v_id) THEN
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
            UPDATE productvariants
            SET isdefault = FALSE
            WHERE fk_product = v_fk_product AND COALESCE(cancelled, FALSE) = FALSE;
        END IF;

        UPDATE productvariants
        SET sellingprice = v_price,
            mrp = v_price,
            isdefault = COALESCE(p_is_default, FALSE)
        WHERE id_productvariant = v_id;

        IF (v_incoming_sig IS NOT NULL) THEN
            DELETE FROM productvariantattributes WHERE fk_productvariant = v_id;

            INSERT INTO productvariantattributes (fk_productvariant, fk_variant, fk_variantvalue)
            SELECT v_id, fk_variant, fk_variantvalue
            FROM tmp_variant_attrs;

            SELECT string_agg(v.name || ':' || vv.name, ' | ' ORDER BY a.fk_variant)
            INTO v_label
            FROM tmp_variant_attrs a
            INNER JOIN variants v ON v.id_variant = a.fk_variant
            INNER JOIN variantvalues vv ON vv.id_variantvalue = a.fk_variantvalue;

            IF (v_label IS NOT NULL) THEN
                UPDATE productvariants SET variantlabel = v_label WHERE id_productvariant = v_id;
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
        IF NOT EXISTS (SELECT 1 FROM productvariants WHERE id_productvariant = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid ProductVariant ID.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        -- productvariants has cancelled / cancelledon only (no cancelledreason / cancelledby)
        UPDATE productvariants
        SET cancelled = TRUE,
            cancelledon = v_now,
            isactive = FALSE,
            isdefault = FALSE
        WHERE id_productvariant = v_id;

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
