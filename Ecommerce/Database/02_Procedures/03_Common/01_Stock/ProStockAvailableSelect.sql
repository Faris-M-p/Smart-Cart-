/****** Object:  StoredProcedure [dbo].[ProStockAvailableSelect]    Script Date: 13-01-2026 22:06:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProStockAvailableSelect
Created By       : Muhammed Faris
Created On       : 13/12/2025

PURPOSE
  Returns available stock for:
    • A single ProductVariant (SKU)
    • OR a full Product (sum of all variants)

USED BY
  ✔ Product Detail Page (User + Admin)
  ✔ Variant selection dropdown
  ✔ Add to Cart validation

PARAMETERS
  @ID_Product         → optional
  @ID_ProductVariant  → optional

**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProStockAvailableSelect]
(
    @ID_Product INT = 0,
    @ID_ProductVariant INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (@ID_Product = 0 AND @ID_ProductVariant = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Provide either ID_Product or ID_ProductVariant.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    -------------------------------------------------------------------
    -- SKU LEVEL STOCK
    -------------------------------------------------------------------
    IF (@ID_ProductVariant > 0)
    BEGIN
        SELECT 
            PV.ID_ProductVariant,
            PV.FK_Product,
            SUM(S.Quantity) AS AvailableStock
        FROM ProductVariant PV
        LEFT JOIN Stock S 
            ON S.FK_ProductVariant = PV.ID_ProductVariant
            AND S.Cancelled = 0
        WHERE PV.ID_ProductVariant = @ID_ProductVariant
          AND PV.Cancelled = 0
        GROUP BY PV.ID_ProductVariant, PV.FK_Product;

        RETURN;
    END

    -------------------------------------------------------------------
    -- PRODUCT LEVEL STOCK (SUM OF ALL SKUs)
    -------------------------------------------------------------------
    SELECT 
        P.ID_Product,
        SUM(S.Quantity) AS AvailableStock
    FROM Product P
    INNER JOIN ProductVariant PV ON PV.FK_Product = P.ID_Product AND PV.Cancelled = 0
    LEFT JOIN Stock S ON S.FK_ProductVariant = PV.ID_ProductVariant AND S.Cancelled = 0
    WHERE P.ID_Product = @ID_Product
    GROUP BY P.ID_Product;

END;

