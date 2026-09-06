SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetCart
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Storefront cart lines for the authenticated user, plus totals.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetCart]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CartId INT;

    SELECT @CartId = C.CartId
    FROM [dbo].[Cart] AS C WITH (NOLOCK)
    WHERE C.UserId = @UserId;

    SELECT
        CI.CartItemId,
        CI.ProductId,
        ISNULL(CI.ProductVariantId, 0) AS ProductVariantId,
        P.Name,
        P.Slug,
        CASE
            WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN ISNULL(PV.SKU, N'')
            ELSE PV.VariantLabel
        END AS Label,
        ISNULL(PV.SKU, N'') AS SKU,
        ISNULL(PV.SellingPrice, CI.Price) AS Price,
        ISNULL(PV.MRP, CI.Price) AS MRP,
        CI.Quantity,
        ISNULL(PV.SellingPrice, CI.Price) * CI.Quantity AS LineTotal,
        ISNULL(ST.Qty, 0) AS StockQuantity,
        CAST(CASE
            WHEN ISNULL(P.Cancelled, 0) = 0
             AND P.IsActive = 1
             AND PV.ID_ProductVariant IS NOT NULL
             AND ISNULL(PV.Cancelled, 0) = 0
             AND PV.IsActive = 1
             AND ISNULL(ST.Qty, 0) > 0
            THEN 1 ELSE 0
        END AS BIT) AS InStock,
        ISNULL((
            SELECT TOP (1) SM.MediaUrl
            FROM [dbo].[SkuMedia] AS SM WITH (NOLOCK)
            WHERE SM.FK_ProductSKU = PV.ID_ProductVariant
              AND SM.MediaUrl IS NOT NULL
              AND SM.MediaUrl <> N''
            ORDER BY SM.IsPrimary DESC, SM.DisplayOrder ASC
        ), (
            SELECT TOP (1) PM.MediaUrl
            FROM [dbo].[ProductMedia] AS PM WITH (NOLOCK)
            WHERE PM.FK_Product = P.ID_Product
              AND PM.MediaType = N'Image'
              AND PM.MediaUrl IS NOT NULL
              AND PM.MediaUrl <> N''
            ORDER BY PM.IsPrimary DESC, PM.DisplayOrder ASC
        )) AS ImageUrl
    FROM [dbo].[CartItems] AS CI WITH (NOLOCK)
    INNER JOIN [dbo].[Products] AS P WITH (NOLOCK) ON P.ID_Product = CI.ProductId
    LEFT JOIN [dbo].[ProductVariants] AS PV WITH (NOLOCK)
        ON PV.ID_ProductVariant = CI.ProductVariantId
    LEFT JOIN (
        SELECT FK_ProductVariant, SUM(Quantity) AS Qty
        FROM [dbo].[Stock] WITH (NOLOCK)
        WHERE ISNULL(Cancelled, 0) = 0
        GROUP BY FK_ProductVariant
    ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
    WHERE CI.CartId = @CartId
    ORDER BY CI.CartItemId DESC;

    SELECT
        ISNULL(SUM(CI.Quantity), 0) AS TotalQuantity,
        ISNULL(SUM(ISNULL(PV.SellingPrice, CI.Price) * CI.Quantity), 0) AS Subtotal,
        ISNULL(COUNT(1), 0) AS ItemCount
    FROM [dbo].[CartItems] AS CI WITH (NOLOCK)
    LEFT JOIN [dbo].[ProductVariants] AS PV WITH (NOLOCK)
        ON PV.ID_ProductVariant = CI.ProductVariantId
    WHERE CI.CartId = @CartId;
END
GO
