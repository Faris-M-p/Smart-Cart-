SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetCheckoutPreview
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Cart checkout when @ProductVariantId is 0.
  Buy Now checkout when a variant is supplied.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetCheckoutPreview]
(
    @UserId INT,
    @ProductVariantId INT = 0,
    @Quantity INT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Quantity, 0) < 1
        SET @Quantity = 1;

    CREATE TABLE #Lines
    (
        ProductId INT NOT NULL,
        ProductVariantId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(255) NULL,
        Label NVARCHAR(200) NULL,
        SKU NVARCHAR(100) NULL,
        Price DECIMAL(10, 2) NOT NULL,
        MRP DECIMAL(10, 2) NOT NULL,
        Quantity INT NOT NULL,
        LineTotal DECIMAL(10, 2) NOT NULL,
        StockQuantity INT NOT NULL,
        InStock BIT NOT NULL,
        ImageUrl NVARCHAR(500) NULL
    );

    IF ISNULL(@ProductVariantId, 0) > 0
    BEGIN
        INSERT INTO #Lines
        SELECT
            P.ID_Product,
            PV.ID_ProductVariant,
            P.Name,
            P.Slug,
            CASE
                WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN ISNULL(PV.SKU, N'')
                ELSE PV.VariantLabel
            END,
            ISNULL(PV.SKU, N''),
            PV.SellingPrice,
            ISNULL(PV.MRP, PV.SellingPrice),
            @Quantity,
            PV.SellingPrice * @Quantity,
            ISNULL(ST.Qty, 0),
            CAST(CASE
                WHEN ISNULL(P.Cancelled, 0) = 0
                 AND P.IsActive = 1
                 AND ISNULL(PV.Cancelled, 0) = 0
                 AND PV.IsActive = 1
                 AND ISNULL(ST.Qty, 0) >= @Quantity
                THEN 1 ELSE 0
            END AS BIT),
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
            ))
        FROM [dbo].[ProductVariants] AS PV WITH (NOLOCK)
        INNER JOIN [dbo].[Products] AS P WITH (NOLOCK) ON P.ID_Product = PV.FK_Product
        LEFT JOIN (
            SELECT FK_ProductVariant, SUM(Quantity) AS Qty
            FROM [dbo].[Stock] WITH (NOLOCK)
            WHERE ISNULL(Cancelled, 0) = 0
            GROUP BY FK_ProductVariant
        ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
        WHERE PV.ID_ProductVariant = @ProductVariantId;
    END
    ELSE
    BEGIN
        INSERT INTO #Lines
        SELECT
            CI.ProductId,
            ISNULL(CI.ProductVariantId, 0),
            P.Name,
            P.Slug,
            CASE
                WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN ISNULL(PV.SKU, N'')
                ELSE PV.VariantLabel
            END,
            ISNULL(PV.SKU, N''),
            ISNULL(PV.SellingPrice, CI.Price),
            ISNULL(PV.MRP, CI.Price),
            CI.Quantity,
            ISNULL(PV.SellingPrice, CI.Price) * CI.Quantity,
            ISNULL(ST.Qty, 0),
            CAST(CASE
                WHEN ISNULL(P.Cancelled, 0) = 0
                 AND P.IsActive = 1
                 AND PV.ID_ProductVariant IS NOT NULL
                 AND ISNULL(PV.Cancelled, 0) = 0
                 AND PV.IsActive = 1
                 AND ISNULL(ST.Qty, 0) >= CI.Quantity
                THEN 1 ELSE 0
            END AS BIT),
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
            ))
        FROM [dbo].[CartItems] AS CI WITH (NOLOCK)
        INNER JOIN [dbo].[Cart] AS C WITH (NOLOCK) ON C.CartId = CI.CartId
        INNER JOIN [dbo].[Products] AS P WITH (NOLOCK) ON P.ID_Product = CI.ProductId
        LEFT JOIN [dbo].[ProductVariants] AS PV WITH (NOLOCK)
            ON PV.ID_ProductVariant = CI.ProductVariantId
        LEFT JOIN (
            SELECT FK_ProductVariant, SUM(Quantity) AS Qty
            FROM [dbo].[Stock] WITH (NOLOCK)
            WHERE ISNULL(Cancelled, 0) = 0
            GROUP BY FK_ProductVariant
        ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
        WHERE C.UserId = @UserId;
    END

    SELECT
        ProductId,
        ProductVariantId,
        Name,
        Slug,
        Label,
        SKU,
        Price,
        MRP,
        Quantity,
        LineTotal,
        StockQuantity,
        InStock,
        ImageUrl
    FROM #Lines
    ORDER BY Name;

    SELECT
        ISNULL(SUM(Quantity), 0) AS TotalQuantity,
        ISNULL(SUM(LineTotal), 0) AS Subtotal,
        ISNULL(COUNT(1), 0) AS ItemCount,
        CAST(CASE WHEN COUNT(1) > 0 AND MIN(CAST(InStock AS INT)) = 1 THEN 1 ELSE 0 END AS BIT) AS CanPlace
    FROM #Lines;

    SELECT
        ISNULL(U.FullName, U.UserName) AS FullName,
        ISNULL(U.PhoneNumber, N'') AS Phone,
        U.Email
    FROM [dbo].[Users] AS U WITH (NOLOCK)
    WHERE U.UserId = @UserId
      AND ISNULL(U.Cancelled, 0) = 0;

    DROP TABLE #Lines;
END
GO
