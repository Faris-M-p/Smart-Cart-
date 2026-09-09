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
        OPEN p_details FOR SELECT NULL::INT AS id_purchasedetail WHERE FALSE;
        OPEN p_stock FOR SELECT NULL::INT AS id_stock WHERE FALSE;
        RETURN;
    END IF;

    OPEN p_header FOR
        SELECT
            p.id_purchase,
            p.fk_supplier,
            s.name AS supplier_name,
            p.invoicenumber,
            p.purchasedate,
            p.totalamount,
            p.notes,
            p.createdon,
            p.cancelled,
            p.cancelledon,
            p.cancelledreason
        FROM purchase p
        LEFT JOIN supplier s ON s.id_supplier = p.fk_supplier
        WHERE p.id_purchase = p_id_purchase;

    OPEN p_details FOR
        SELECT
            pd.id_purchasedetail,
            pd.fk_purchase,
            pd.fk_productvariant,
            pv.fk_product,
            pr.name AS productname,
            pd.quantity,
            pd.purchaseprice,
            pd.mrp,
            pd.expirydate,
            pd.createdon,
            pd.cancelled,
            (
                SELECT string_agg(v.name || ':' || vv.name, ', ' ORDER BY v.displayorder)
                FROM productvariantattributes pva
                INNER JOIN variants v ON v.id_variant = pva.fk_variant
                INNER JOIN variantvalues vv ON vv.id_variantvalue = pva.fk_variantvalue
                WHERE pva.fk_productvariant = pd.fk_productvariant
            ) AS variant_attributes
        FROM purchasedetail pd
        INNER JOIN productvariants pv ON pv.id_productvariant = pd.fk_productvariant
        INNER JOIN products pr ON pr.id_product = pv.fk_product
        WHERE pd.fk_purchase = p_id_purchase
          AND pd.cancelled = FALSE
        ORDER BY pd.id_purchasedetail ASC;

    OPEN p_stock FOR
        SELECT
            s.id_stock,
            s.fk_purchasedetail,
            s.fk_productvariant,
            s.quantity,
            s.createdon,
            s.cancelled,
            s.cancelledon,
            s.cancelledreason
        FROM stock s
        INNER JOIN purchasedetail pd ON pd.id_purchasedetail = s.fk_purchasedetail
        WHERE pd.fk_purchase = p_id_purchase
        ORDER BY s.id_stock ASC;
END;
$$;
