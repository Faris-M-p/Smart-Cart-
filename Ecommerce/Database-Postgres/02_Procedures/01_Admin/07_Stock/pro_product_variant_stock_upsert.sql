/**********************************************************************
Stored Procedure : pro_product_variant_stock_upsert
Source           : ProProductVariantStockUpsert (SQL Server)
Created By       : Muhammed Faris
Created On       : 11/12/2025

PURPOSE
  Insert / update / soft-delete stock rows linked to purchase_detail
  and product_variants (SKU).

ACTIONS
  1 → Insert
  2 → Update
  3 → Soft delete

NOTE
  Source referenced PurchaseDetails/PurchaseDetailID and ProductVariant;
  mapped to purchase_detail / id_purchase_detail and product_variants.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_product_variant_stock_upsert(
    IN p_user_action INT,
    IN p_stock_id INT DEFAULT 0,
    IN p_fk_purchase_detail INT DEFAULT 0,
    IN p_fk_product_variant INT DEFAULT 0,
    IN p_quantity INT DEFAULT 0,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_stock_id INT := COALESCE(p_stock_id, 0);
BEGIN
    BEGIN
        IF COALESCE(p_fk_purchase_detail, 0) = 0 THEN
            RAISE EXCEPTION 'Invalid PurchaseDetail.' USING ERRCODE = 'P0001';
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM purchase_detail
            WHERE id_purchase_detail = p_fk_purchase_detail
              AND cancelled = FALSE
        ) THEN
            RAISE EXCEPTION 'PurchaseDetail does not exist or deleted.' USING ERRCODE = 'P0001';
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM product_variants
            WHERE id_product_variant = p_fk_product_variant
              AND cancelled = FALSE
        ) THEN
            RAISE EXCEPTION 'Invalid SKU (ProductVariant).' USING ERRCODE = 'P0001';
        END IF;

        IF p_user_action = 1 AND COALESCE(p_quantity, 0) <= 0 THEN
            RAISE EXCEPTION 'Quantity must be greater than zero.' USING ERRCODE = 'P0001';
        END IF;

        IF p_user_action = 1 THEN
            IF EXISTS (
                SELECT 1 FROM stock
                WHERE fk_purchase_detail = p_fk_purchase_detail
                  AND cancelled = FALSE
            ) THEN
                RAISE EXCEPTION 'Stock already exists for this Purchase Detail.'
                    USING ERRCODE = 'P0001';
            END IF;

            INSERT INTO stock (
                fk_purchase_detail,
                fk_product_variant,
                quantity,
                created_on,
                enter_by,
                cancelled,
                cancelled_on,
                cancelled_reason,
                cancelled_by
            )
            VALUES (
                p_fk_purchase_detail,
                p_fk_product_variant,
                p_quantity,
                v_now,
                p_enter_by,
                FALSE,
                NULL,
                NULL,
                NULL
            )
            RETURNING id_stock INTO v_stock_id;

            OPEN p_result FOR
                SELECT v_stock_id AS response_code,
                       'Stock added successfully.' AS response_msg,
                       TRUE AS status_code;
            RETURN;
        END IF;

        IF p_user_action = 2 THEN
            IF NOT EXISTS (
                SELECT 1 FROM stock
                WHERE id_stock = v_stock_id AND cancelled = FALSE
            ) THEN
                RAISE EXCEPTION 'Invalid or deleted StockID.' USING ERRCODE = 'P0001';
            END IF;

            UPDATE stock
            SET quantity = p_quantity
            WHERE id_stock = v_stock_id;

            OPEN p_result FOR
                SELECT v_stock_id AS response_code,
                       'Stock updated successfully.' AS response_msg,
                       TRUE AS status_code;
            RETURN;
        END IF;

        IF p_user_action = 3 THEN
            IF NOT EXISTS (SELECT 1 FROM stock WHERE id_stock = v_stock_id) THEN
                RAISE EXCEPTION 'Invalid StockID.' USING ERRCODE = 'P0001';
            END IF;

            UPDATE stock
            SET cancelled = TRUE,
                cancelled_on = v_now,
                cancelled_reason = p_cancelled_reason,
                cancelled_by = p_enter_by
            WHERE id_stock = v_stock_id;

            OPEN p_result FOR
                SELECT v_stock_id AS response_code,
                       'Stock deleted successfully.' AS response_msg,
                       TRUE AS status_code;
            RETURN;
        END IF;

        RAISE EXCEPTION 'Invalid UserAction.' USING ERRCODE = 'P0001';

    EXCEPTION
        WHEN SQLSTATE 'P0001' THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       SQLERRM AS response_msg,
                       FALSE AS status_code;
        WHEN OTHERS THEN
            OPEN p_result FOR
                SELECT -1 AS response_code,
                       SQLERRM AS response_msg,
                       FALSE AS status_code;
    END;
END;
$$;
