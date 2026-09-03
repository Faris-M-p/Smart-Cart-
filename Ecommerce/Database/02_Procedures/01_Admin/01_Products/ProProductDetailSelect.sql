/****** Object:  StoredProcedure [dbo].[ProProductDetailSelect]    Script Date: 13-01-2026 22:03:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductDetailSelect
Created By       : Muhammed Faris
Created On       : 13/12/2025

PURPOSE
  Fetch complete product detail including:
    • Product info
    • Product images
    • Variants (SKU)
    • Variant Attributes
    • Variant Stock summary

USED BY
  ✔ Admin Product Edit Page
  ✔ User Product Detail Page
  ✔ Mobile App Product Info

**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProProductDetailSelect]
(
    @ID_Product INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid or deleted Product ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    -------------------------------------------------------------------
    -- RESULT 1: PRODUCT BASIC INFO
    -------------------------------------------------------------------
    SELECT 
        P.ID_Product,
        P.Name,
        P.Description,
        P.Price,
        P.MRP,
        P.FK_Category,
        P.FK_SubCategory,
        P.FK_Brand,
        P.Rating,
        P.Gender,
        P.FK_Status,
        P.CreatedOn,
        P.UpdatedOn
    FROM Product P
    WHERE P.ID_Product = @ID_Product;


    -------------------------------------------------------------------
    -- RESULT 2: PRODUCT IMAGES
    -------------------------------------------------------------------
    SELECT 
        PI.ID_ProductImage,
        PI.FK_Product,
        PI.Image,
        PI.CreatedOn,
        PI.Cancelled
    FROM ProductImage PI
    WHERE PI.FK_Product = @ID_Product
      AND PI.Cancelled = 0
    ORDER BY PI.ID_ProductImage ASC;


    -------------------------------------------------------------------
    -- RESULT 3: PRODUCT VARIANTS (SKU)
    -------------------------------------------------------------------
    SELECT 
        PV.ID_ProductVariant,
        PV.FK_Product,
        PV.PriceAdjustment,
        PV.IsDefault,
        PV.CreatedOn,
        PV.Cancelled
    FROM ProductVariant PV
    WHERE PV.FK_Product = @ID_Product
      AND PV.Cancelled = 0
    ORDER BY PV.ID_ProductVariant ASC;


    -------------------------------------------------------------------
    -- RESULT 4: VARIANT ATTRIBUTES
    -------------------------------------------------------------------
    SELECT
        PVA.FK_ProductVariant,
        V.VariantName,
        VV.ValueName
    FROM ProductVariantAttribute PVA
    INNER JOIN Variant V ON V.ID_Variant = PVA.FK_Variant
    INNER JOIN VariantValue VV ON VV.ID_VariantValue = PVA.FK_VariantValue
    WHERE PVA.FK_ProductVariant IN (
        SELECT ID_ProductVariant 
        FROM ProductVariant 
        WHERE FK_Product = @ID_Product AND Cancelled = 0
    )
    ORDER BY PVA.FK_ProductVariant, V.DisplayOrder;


    -------------------------------------------------------------------
    -- RESULT 5: STOCK SUMMARY PER VARIANT
    -------------------------------------------------------------------
    SELECT
        PV.ID_ProductVariant,
        SUM(S.Quantity) AS AvailableStock
    FROM ProductVariant PV
    LEFT JOIN Stock S ON S.FK_ProductVariant = PV.ID_ProductVariant AND S.Cancelled = 0
    WHERE PV.FK_Product = @ID_Product
      AND PV.Cancelled = 0
    GROUP BY PV.ID_ProductVariant;

END;

