/**********************************************************************
Stored Procedure : pro_purchase_update
Source           : ProPurchaseUpdate (SQL Server)
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Insert / update / soft-delete purchase header, purchasedetail lines,
  and matching stock batches. purchase details arrive as JSON array text.

ACTIONS
  1 → Insert
  2 → Update
  3 → Soft delete

PurchaseDetails JSON:
[
  {
    id_purchasedetail: 0,
    fk_productvariant: 601,
    quantity: 30,
    purchaseprice: 100.00,
    mrp: 150.00,
    expirydate: 2026-05-20
  }
]
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_purchase_update(
    IN p_user_action INT,
    IN p_id_purchase INT DEFAULT 0,
    IN p_fk_supplier INT DEFAULT 0,
    IN p_purchase_date DATE DEFAULT NULL,
    IN p_invoice_number TEXT DEFAULT NULL,
    IN p_notes TEXT DEFAULT NULL,
    IN p_purchase_details TEXT DEFAULT NULL,
    IN p_enter_by INT DEFAULT NULL,
    IN p_cancelled_reason TEXT DEFAULT NULL,
    INOUT p_result REFCURSOR DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := NOW();
    v_id_purchase INT := COALESCE(p_id_purchase, 0);
    v_purchase_date DATE := p_purchase_date;
    v_detail_id INT;
    v_pv INT;
    v_qty INT;
    v_pprice NUMERIC(18,2);
    v_mrp NUMERIC(18,2);
    v_exp DATE;
    r_detail RECORD;
BEGIN
    CREATE TEMP TABLE IF NOT EXISTS tmp_purchase_details (
        id_purchasedetail INT,
        fk_productvariant INT,
        quantity INT,
        purchaseprice NUMERIC(18,2),
        mrp NUMERIC(18,2),
        expirydate DATE
    ) ON COMMIT DROP;

    DELETE FROM tmp_purchase_details;

    BEGIN
        IF p_user_action IN (1, 2) THEN
            IF p_user_action = 1 AND (COALESCE(p_fk_supplier, 0) = 0) THEN
                RAISE EXCEPTION 'FK_Supplier is required for insert.' USING ERRCODE = 'P0001';
            END IF;

            IF v_purchase_date IS NULL THEN
                v_purchase_date := v_now::DATE;
            END IF;

            IF NOT EXISTS (
                SELECT 1 FROM supplier
                WHERE id_supplier = p_fk_supplier AND cancelled = FALSE
            ) THEN
                RAISE EXCEPTION 'Invalid or deleted Supplier.' USING ERRCODE = 'P0001';
            END IF;
        END IF;

        IF p_user_action = 2 THEN
            IF v_id_purchase = 0 THEN
                RAISE EXCEPTION 'ID_Purchase is required for update.' USING ERRCODE = 'P0001';
            END IF;
            IF NOT EXISTS (SELECT 1 FROM purchase WHERE id_purchase = v_id_purchase) THEN
                RAISE EXCEPTION 'Purchase not found.' USING ERRCODE = 'P0001';
            END IF;
        END IF;

        IF p_user_action = 3 THEN
            IF v_id_purchase = 0 THEN
                RAISE EXCEPTION 'ID_Purchase is required for delete.' USING ERRCODE = 'P0001';
            END IF;
            IF NOT EXISTS (SELECT 1 FROM purchase WHERE id_purchase = v_id_purchase) THEN
                RAISE EXCEPTION 'Purchase not found.' USING ERRCODE = 'P0001';
            END IF;
        END IF;

        IF p_purchase_details IS NOT NULL AND TRIM(p_purchase_details) <> '' THEN
            INSERT INTO tmp_purchase_details (
                id_purchasedetail, fk_productvariant, quantity,
                purchaseprice, mrp, expirydate
            )
            SELECT
                COALESCE((elem->>'ID_PurchaseDetail')::INT, 0),
                NULLIF(elem->>'FK_ProductVariant', '')::INT,
                NULLIF(elem->>'Quantity', '')::INT,
                NULLIF(elem->>'PurchasePrice', '')::NUMERIC(18,2),
                NULLIF(elem->>'MRP', '')::NUMERIC(18,2),
                NULLIF(elem->>'ExpiryDate', '')::DATE
            FROM jsonb_array_elements(p_purchase_details::jsonb) AS elem;
        END IF;

        ------------------------------------------------------------------
        -- action 1: INSERT
        ------------------------------------------------------------------
        IF p_user_action = 1 THEN
            IF NOT EXISTS (SELECT 1 FROM tmp_purchase_details) THEN
                RAISE EXCEPTION 'At least one PurchaseDetail is required in PurchaseDetails JSON.'
                    USING ERRCODE = 'P0001';
            END IF;

            IF EXISTS (
                SELECT 1
                FROM tmp_purchase_details d
                WHERE d.fk_productvariant IS NULL
                   OR NOT EXISTS (
                        SELECT 1 FROM productvariants pv
                        WHERE pv.id_productvariant = d.fk_productvariant
                          AND pv.cancelled = FALSE
                   )
            ) THEN
                RAISE EXCEPTION 'One or more FK_ProductVariant are invalid or deleted.'
                    USING ERRCODE = 'P0001';
            END IF;

            IF EXISTS (
                SELECT 1 FROM tmp_purchase_details d
                WHERE d.quantity IS NULL OR d.quantity <= 0
            ) THEN
                RAISE EXCEPTION 'Quantity must be > 0 for all purchase details.'
                    USING ERRCODE = 'P0001';
            END IF;

            INSERT INTO purchase (
                fk_supplier, purchasedate, invoicenumber, totalamount, notes,
                createdon, enterby, cancelled, cancelledon, cancelledreason, cancelledby
            )
            VALUES (
                p_fk_supplier, v_purchase_date, p_invoice_number, 0.00, p_notes,
                v_now, p_enter_by, FALSE, NULL, NULL, NULL
            )
            RETURNING id_purchase INTO v_id_purchase;

            FOR r_detail IN
                SELECT fk_productvariant, quantity, purchaseprice, mrp, expirydate
                FROM tmp_purchase_details
            LOOP
                INSERT INTO purchasedetail (
                    fk_purchase, fk_productvariant, quantity, purchaseprice, mrp, expirydate,
                    createdon, enterby, cancelled, cancelledon, cancelledreason, cancelledby
                )
                VALUES (
                    v_id_purchase, r_detail.fk_productvariant, r_detail.quantity,
                    r_detail.purchaseprice, r_detail.mrp, r_detail.expirydate,
                    v_now, p_enter_by, FALSE, NULL, NULL, NULL
                )
                RETURNING id_purchasedetail INTO v_detail_id;

                INSERT INTO stock (
                    fk_purchasedetail, fk_productvariant, quantity,
                    createdon, enterby, cancelled, cancelledon, cancelledreason, cancelledby
                )
                VALUES (
                    v_detail_id, r_detail.fk_productvariant, r_detail.quantity,
                    v_now, p_enter_by, FALSE, NULL, NULL, NULL
                );
            END LOOP;

            UPDATE purchase
            SET totalamount = COALESCE((
                SELECT SUM(COALESCE(quantity, 0) * COALESCE(purchaseprice, 0))
                FROM purchasedetail
                WHERE fk_purchase = v_id_purchase AND cancelled = FALSE
            ), 0)
            WHERE id_purchase = v_id_purchase;

            OPEN p_result FOR
                SELECT v_id_purchase AS response_code,
                       'Purchase created successfully.' AS response_msg,
                       TRUE AS status_code;
            RETURN;
        END IF;

        ------------------------------------------------------------------
        -- action 2: UPDATE
        ------------------------------------------------------------------
        IF p_user_action = 2 THEN
            UPDATE purchase
            SET fk_supplier = p_fk_supplier,
                purchasedate = v_purchase_date,
                invoicenumber = p_invoice_number,
                notes = p_notes
            WHERE id_purchase = v_id_purchase;

            IF EXISTS (
                SELECT 1
                FROM tmp_purchase_details d
                WHERE d.fk_productvariant IS NULL
                   OR NOT EXISTS (
                        SELECT 1 FROM productvariants pv
                        WHERE pv.id_productvariant = d.fk_productvariant
                          AND pv.cancelled = FALSE
                   )
            ) THEN
                RAISE EXCEPTION 'One or more FK_ProductVariant are invalid or deleted.'
                    USING ERRCODE = 'P0001';
            END IF;

            UPDATE purchasedetail pd
            SET cancelled = TRUE,
                cancelledon = v_now,
                cancelledreason = 'Removed in update',
                cancelledby = p_enter_by
            WHERE pd.fk_purchase = v_id_purchase
              AND pd.cancelled = FALSE
              AND pd.id_purchasedetail NOT IN (
                    SELECT d.id_purchasedetail
                    FROM tmp_purchase_details d
                    WHERE d.id_purchasedetail > 0
              );

            UPDATE stock s
            SET cancelled = TRUE,
                cancelledon = v_now,
                cancelledreason = 'Removed due to purchase detail removal',
                cancelledby = p_enter_by
            WHERE s.fk_purchasedetail IN (
                SELECT pd.id_purchasedetail
                FROM purchasedetail pd
                WHERE pd.fk_purchase = v_id_purchase
                  AND pd.cancelled = TRUE
            )
              AND s.cancelled = FALSE;

            FOR r_detail IN
                SELECT id_purchasedetail, fk_productvariant, quantity,
                       purchaseprice, mrp, expirydate
                FROM tmp_purchase_details
            LOOP
                v_detail_id := r_detail.id_purchasedetail;
                v_pv := r_detail.fk_productvariant;
                v_qty := r_detail.quantity;
                v_pprice := r_detail.purchaseprice;
                v_mrp := r_detail.mrp;
                v_exp := r_detail.expirydate;

                IF v_detail_id > 0 THEN
                    UPDATE purchasedetail
                    SET fk_productvariant = v_pv,
                        quantity = v_qty,
                        purchaseprice = v_pprice,
                        mrp = v_mrp,
                        expirydate = v_exp,
                        enterby = p_enter_by
                    WHERE id_purchasedetail = v_detail_id;

                    UPDATE stock
                    SET quantity = v_qty
                    WHERE fk_purchasedetail = v_detail_id
                      AND fk_productvariant = v_pv
                      AND cancelled = FALSE;
                ELSE
                    INSERT INTO purchasedetail (
                        fk_purchase, fk_productvariant, quantity, purchaseprice, mrp, expirydate,
                        createdon, enterby, cancelled
                    )
                    VALUES (
                        v_id_purchase, v_pv, v_qty, v_pprice, v_mrp, v_exp,
                        v_now, p_enter_by, FALSE
                    )
                    RETURNING id_purchasedetail INTO v_detail_id;

                    INSERT INTO stock (
                        fk_purchasedetail, fk_productvariant, quantity,
                        createdon, enterby, cancelled
                    )
                    VALUES (
                        v_detail_id, v_pv, v_qty,
                        v_now, p_enter_by, FALSE
                    );
                END IF;
            END LOOP;

            UPDATE purchase
            SET totalamount = COALESCE((
                SELECT SUM(COALESCE(quantity, 0) * COALESCE(purchaseprice, 0))
                FROM purchasedetail
                WHERE fk_purchase = v_id_purchase AND cancelled = FALSE
            ), 0)
            WHERE id_purchase = v_id_purchase;

            OPEN p_result FOR
                SELECT v_id_purchase AS response_code,
                       'Purchase updated successfully.' AS response_msg,
                       TRUE AS status_code;
            RETURN;
        END IF;

        ------------------------------------------------------------------
        -- action 3: SOFT DELETE
        ------------------------------------------------------------------
        IF p_user_action = 3 THEN
            UPDATE purchase
            SET cancelled = TRUE,
                cancelledon = v_now,
                cancelledreason = p_cancelled_reason,
                cancelledby = p_enter_by
            WHERE id_purchase = v_id_purchase;

            UPDATE purchasedetail
            SET cancelled = TRUE,
                cancelledon = v_now,
                cancelledreason = p_cancelled_reason,
                cancelledby = p_enter_by
            WHERE fk_purchase = v_id_purchase;

            UPDATE stock
            SET cancelled = TRUE,
                cancelledon = v_now,
                cancelledreason = p_cancelled_reason,
                cancelledby = p_enter_by
            WHERE fk_purchasedetail IN (
                SELECT id_purchasedetail
                FROM purchasedetail
                WHERE fk_purchase = v_id_purchase
            );

            OPEN p_result FOR
                SELECT v_id_purchase AS response_code,
                       'Purchase deleted successfully.' AS response_msg,
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
