/**********************************************************************
Stored Procedure : pro_product_variant_image_select
Created By       : Muhammed Faris
Created On       : 10/12/2025
Source           : ProProductVariantImageSelect (SQL Server)
Target table     : skumedia
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_image_select(
    IN p_fk_product_variant INT,
    IN p_include_cancelled BOOLEAN DEFAULT FALSE,
    IN p_page_index INT DEFAULT 1,
    IN p_page_size INT DEFAULT 20,
    IN p_sort_column VARCHAR(50) DEFAULT '',
    IN p_sort_mode VARCHAR(5) DEFAULT 'ASC',
    INOUT p_result REFCURSOR DEFAULT 'p_result',
    INOUT p_meta REFCURSOR DEFAULT 'p_meta'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_count BIGINT;
    v_sort_col TEXT := LOWER(TRIM(COALESCE(p_sort_column, '')));
    v_sort_mode TEXT := UPPER(TRIM(COALESCE(p_sort_mode, 'ASC')));
    v_order TEXT;
BEGIN
    IF (COALESCE(p_fk_product_variant, 0) = 0) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code, 'Invalid FK_ProductVariant.' AS response_msg, FALSE AS status_code;
        OPEN p_meta FOR
            SELECT 0::BIGINT AS total_count, p_page_index AS page_index, p_page_size AS page_size;
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM productvariants
        WHERE id_productvariant = p_fk_product_variant
          AND COALESCE(cancelled, FALSE) = FALSE
    ) THEN
        OPEN p_result FOR
            SELECT -1 AS response_code,
                   'ProductVariant does not exist or deleted.' AS response_msg,
                   FALSE AS status_code;
        OPEN p_meta FOR
            SELECT 0::BIGINT AS total_count, p_page_index AS page_index, p_page_size AS page_size;
        RETURN;
    END IF;

    -- p_include_cancelled kept for API parity; skumedia has no cancelled column
    IF v_sort_mode NOT IN ('ASC', 'DESC') THEN
        v_sort_mode := 'ASC';
    END IF;

    IF v_sort_col IN ('createdon', 'created_on', 'created_at') THEN
        v_order := format('created_on %s', v_sort_mode);
    ELSIF v_sort_col IN ('isdefault', 'is_default', 'is_primary') THEN
        v_order := format('is_default %s', v_sort_mode);
    ELSE
        v_order := 'is_default DESC, created_on DESC';
    END IF;

    CREATE TEMP TABLE IF NOT EXISTS tmp_variant_image (
        rn BIGINT,
        id_productvariantimage INT,
        fk_productvariant INT,
        imageurl TEXT,
        isdefault BOOLEAN,
        createdon TIMESTAMP,
        cancelled BOOLEAN,
        cancelledon TIMESTAMP,
        cancelledreason TEXT
    ) ON COMMIT DROP;

    DELETE FROM tmp_variant_image;

    EXECUTE format($q$
        INSERT INTO tmp_variant_image (
            rn, id_productvariantimage, fk_productvariant, imageurl,
            isdefault, createdon, cancelled, cancelledon, cancelledreason
        )
        SELECT
            ROW_NUMBER() OVER (ORDER BY %s),
            sm.id_skumedia,
            sm.fk_productsku,
            sm.mediaurl,
            sm.isprimary,
            sm.createdat,
            FALSE,
            NULL::TIMESTAMP,
            NULL::TEXT
        FROM skumedia sm
        WHERE sm.fk_productsku = $1
    $q$, v_order)
    USING p_fk_product_variant;

    GET DIAGNOSTICS v_total_count = ROW_COUNT;

    OPEN p_result FOR
        SELECT
            id_productvariantimage,
            fk_productvariant,
            imageurl,
            isdefault,
            createdon,
            cancelled,
            cancelledon,
            cancelledreason
        FROM tmp_variant_image
        WHERE rn > ((GREATEST(p_page_index, 1) - 1) * GREATEST(p_page_size, 1))
          AND rn <= (GREATEST(p_page_index, 1) * GREATEST(p_page_size, 1));

    OPEN p_meta FOR
        SELECT v_total_count AS total_count, p_page_index AS page_index, p_page_size AS page_size;
END;
$$;
