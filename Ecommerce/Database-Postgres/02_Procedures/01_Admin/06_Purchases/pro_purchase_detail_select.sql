/**********************************************************************
Stored Procedure : pro_purchase_detail_select
Source           : ProPurchaseDetailSelect (SQL Server)
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Fetch purchase header, detail lines (with variant attributes), and
  related stock batches for one purchase.
**********************************************************************/
CREATE OR REPLACE PROCEDURE pro_purchase_detail_select(
    IN p_id_purchase INT,
    INOUT p_header REFCURSOR DEFAULT 'p_header',
    INOUT p_details REFCURSOR DEFAULT 'p_details',
    INOUT p_stock REFCURSOR DEFAULT 'p_stock'
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM purchase WHERE id_purchase = p_id_purchase) THEN
        OPEN p_header FOR
            SELECT -1 AS response_code,
                   'Invalid Purchase ID.' AS response_msg,
                   FALSE AS status_code;
        OPEN p_details FOR SELECT NULL::INT AS id_purchase_detail WHERE FALSE;
        OPEN p_stock FOR SELECT NULL::INT AS id_stock WHERE FALSE;
        RETURN;
    END IF;

    OPEN p_header FOR
        SELECT
            p.id_purchase,
            p.fk_supplier,
            s.name AS supplier_name,
            p.invoice_number,
            p.purchase_date,
            p.total_amount,
            p.notes,
            p.created_on,
            p.cancelled,
            p.cancelled_on,
            p.cancelled_reason
        FROM purchase p
        LEFT JOIN supplier s ON s.id_supplier = p.fk_supplier
        WHERE p.id_purchase = p_id_purchase;

    OPEN p_details FOR
        SELECT
            pd.id_purchase_detail,
            pd.fk_purchase,
            pd.fk_product_variant,
            pv.fk_product,
            pr.name AS product_name,
            pd.quantity,
            pd.purchase_price,
            pd.mrp,
            pd.expiry_date,
            pd.created_on,
            pd.cancelled,
            (
                SELECT string_agg(v.name || ':' || vv.name, ', ' ORDER BY v.display_order)
                FROM product_variant_attributes pva
                INNER JOIN variants v ON v.id_variant = pva.fk_variant
                INNER JOIN variant_values vv ON vv.id_variant_value = pva.fk_variant_value
                WHERE pva.fk_product_variant = pd.fk_product_variant
            ) AS variant_attributes
        FROM purchase_detail pd
        INNER JOIN product_variants pv ON pv.id_product_variant = pd.fk_product_variant
        INNER JOIN products pr ON pr.id_product = pv.fk_product
        WHERE pd.fk_purchase = p_id_purchase
          AND pd.cancelled = FALSE
        ORDER BY pd.id_purchase_detail ASC;

    OPEN p_stock FOR
        SELECT
            s.id_stock,
            s.fk_purchase_detail,
            s.fk_product_variant,
            s.quantity,
            s.created_on,
            s.cancelled,
            s.cancelled_on,
            s.cancelled_reason
        FROM stock s
        INNER JOIN purchase_detail pd ON pd.id_purchase_detail = s.fk_purchase_detail
        WHERE pd.fk_purchase = p_id_purchase
        ORDER BY s.id_stock ASC;
END;
$$;
