/**********************************************************************
Stored Procedure : pro_product_variant_image_update
Created By       : Muhammed Faris
Created On       : 10/12/2025
Source           : ProProductVariantImageUpdate (SQL Server)
Target table     : skumedia (canonical sku media; supersedes productvariantimages)

p_user_action:
  1 = Add, 2 = Update, 3 = Delete (hard), 4 = Set as Default (isprimary)
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_image_update(
    IN p_user_action INT,
    IN p_id_product_variant_image INT DEFAULT 0,
    IN p_fk_product_variant INT DEFAULT 0,
    IN p_image_url TEXT DEFAULT NULL,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id INT := COALESCE(p_id_product_variant_image, 0);
    v_sku INT;
    v_display_order INT;
BEGIN
    -------------------------------------------------------------------
    -- VALIDATION: sku must exist
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF NOT EXISTS (
            SELECT 1 FROM productvariants
            WHERE id_productvariant = p_fk_product_variant
              AND COALESCE(cancelled, FALSE) = FALSE
        ) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'Invalid FK_ProductVariant.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;
    END IF;

    IF (p_user_action IN (2, 3, 4)) THEN
        IF NOT EXISTS (SELECT 1 FROM skumedia WHERE id_skumedia = v_id) THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'Invalid ProductVariantImage ID.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;
    END IF;

    -------------------------------------------------------------------
    -- INSERT (ADD NEW IMAGE)
    -------------------------------------------------------------------
    IF (p_user_action = 1) THEN
        IF (COALESCE(p_image_url, '') = '') THEN
            OPEN p_result FOR
                SELECT -1 AS response_code, 'ImageURL cannot be empty.' AS response_msg, FALSE AS status_code;
            RETURN;
        END IF;

        SELECT COALESCE(MAX(displayorder), -1) + 1
        INTO v_display_order
        FROM skumedia
        WHERE fk_productsku = p_fk_product_variant;

        INSERT INTO skumedia (
            fk_productsku, mediatype, mediaurl, displayorder, isprimary, createdat, updatedat
        )
        VALUES (
            p_fk_product_variant, 'Image', p_image_url, v_display_order, FALSE, v_now, NULL
        )
        RETURNING id_skumedia INTO v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Variant image added successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- UPDATE EXISTING IMAGE
    -------------------------------------------------------------------
    IF (p_user_action = 2) THEN
        IF (COALESCE(p_image_url, '') = '') THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       'ImageURL cannot be empty for update.' AS response_msg,
                       FALSE AS status_code;
            RETURN;
        END IF;

        UPDATE skumedia
        SET mediaurl = p_image_url,
            updatedat = v_now
        WHERE id_skumedia = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Variant image updated successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- DELETE IMAGE (hard delete — skumedia has no cancelled columns)
    -------------------------------------------------------------------
    IF (p_user_action = 3) THEN
        DELETE FROM skumedia WHERE id_skumedia = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Variant image deleted successfully.' AS response_msg,
                   TRUE AS status_code;
        RETURN;
    END IF;

    -------------------------------------------------------------------
    -- SET DEFAULT IMAGE (isprimary)
    -------------------------------------------------------------------
    IF (p_user_action = 4) THEN
        SELECT fk_productsku INTO v_sku
        FROM skumedia
        WHERE id_skumedia = v_id;

        UPDATE skumedia
        SET isprimary = FALSE,
            updatedat = v_now
        WHERE fk_productsku = v_sku;

        UPDATE skumedia
        SET isprimary = TRUE,
            updatedat = v_now
        WHERE id_skumedia = v_id;

        OPEN p_result FOR
            SELECT v_id AS response_code,
                   'Variant default image updated successfully.' AS response_msg,
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
