/****** Object:  StoredProcedure [dbo].[GetProductDetailsById]    Script Date: 18-01-2025 23:05:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Creating or altering the GetProductDetailsByStockId stored procedure
CREATE OR ALTER PROCEDURE [dbo].[GetProductDetailsById]
    @StockId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Selecting product and stock details based on StockId
    SELECT 
        P.ID_Product AS ProductId,
        P.Name,
		P.Description,
        SC.FK_Category AS CategoryId,
        P.FK_SubCategory AS SubCategoryId,
        P.FK_Brand AS BrandId,
        CAST(NULL AS NVARCHAR(10)) AS Gender,
        CAST(NULL AS INT) AS StatusId,
        S.ID_Stock AS StockId,
        PV.SellingPrice AS Price,
        PV.MRP,
        S.Quantity,
        CAST(NULL AS INT) AS Rating
    FROM 
        Products AS P WITH(NOLOCK)
    INNER JOIN
        SubCategory AS SC WITH(NOLOCK) ON SC.ID_SubCategory = P.FK_SubCategory
    INNER JOIN
        ProductVariants AS PV WITH(NOLOCK) ON PV.FK_Product = P.ID_Product
    INNER JOIN 
        Stock AS S WITH(NOLOCK) ON S.FK_ProductVariant = PV.ID_ProductVariant
    WHERE 
        S.ID_Stock = @StockId;

END;
GO
