SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetWishlist
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetWishlist]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        WI.WishlistItemId,
        WI.ProductId,
        P.Name,
        P.Slug,
        ISNULL(C.Name, N'') AS CategoryName,
        ISNULL(B.BrandName, N'') AS BrandName,
        ISNULL(PR.Price, 0) AS Price,
        ISNULL(PR.MRP, 0) AS MRP,
        CAST(CASE WHEN ISNULL(PR.StockQty, 0) > 0 THEN 1 ELSE 0 END AS BIT) AS InStock,
        ISNULL((
            SELECT TOP (1) PM.MediaUrl
            FROM [dbo].[ProductMedia] AS PM WITH (NOLOCK)
            WHERE PM.FK_Product = P.ID_Product
              AND PM.MediaType = N'Image'
              AND PM.MediaUrl IS NOT NULL
              AND PM.MediaUrl <> N''
            ORDER BY PM.IsPrimary DESC, PM.DisplayOrder ASC
        ), N'') AS ImageUrl
    FROM [dbo].[WishlistItems] AS WI WITH (NOLOCK)
    INNER JOIN [dbo].[Wishlist] AS W WITH (NOLOCK) ON W.WishlistId = WI.WishlistId
    INNER JOIN [dbo].[Products] AS P WITH (NOLOCK) ON P.ID_Product = WI.ProductId
    INNER JOIN [dbo].[SubCategory] AS SC WITH (NOLOCK) ON SC.ID_SubCategory = P.FK_SubCategory
    INNER JOIN [dbo].[Category] AS C WITH (NOLOCK) ON C.ID_Category = SC.FK_Category
    LEFT JOIN [dbo].[Brand] AS B WITH (NOLOCK) ON B.ID_Brand = P.FK_Brand
    LEFT JOIN (
        SELECT
            PV.FK_Product,
            MIN(PV.SellingPrice) AS Price,
            MIN(PV.MRP) AS MRP,
            SUM(ISNULL(ST.Qty, 0)) AS StockQty
        FROM [dbo].[ProductVariants] AS PV WITH (NOLOCK)
        LEFT JOIN (
            SELECT FK_ProductVariant, SUM(Quantity) AS Qty
            FROM [dbo].[Stock] WITH (NOLOCK)
            WHERE ISNULL(Cancelled, 0) = 0
            GROUP BY FK_ProductVariant
        ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
        WHERE ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
        GROUP BY PV.FK_Product
    ) AS PR ON PR.FK_Product = P.ID_Product
    WHERE W.UserId = @UserId
      AND ISNULL(W.Cancelled, 0) = 0
      AND ISNULL(WI.Cancelled, 0) = 0
      AND ISNULL(P.Cancelled, 0) = 0
      AND P.IsActive = 1
    ORDER BY WI.WishlistItemId DESC;
END
GO
