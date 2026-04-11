USE [Ecommerse]
GO

/****** Object:  StoredProcedure [dbo].[GetProductDetailsById]    Script Date: 18-01-2025 23:05:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Creating or altering the GetProductDetailsByStockId stored procedure
CREATE OR ALTER  PROCEDURE [dbo].[GetProductDetailsById]
    @StockId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Selecting product and stock details based on StockId
    SELECT 
        P.ProductId,
        P.Name,
		p.Description,
        P.CategoryId,
        P.SubCategoryId,
        P.BrandId,
        P.Gender,
        P.StatusId,
        S.StockId,
        S.Price,
        S.MRP,
        S.Quantity,
        S.Rating
    FROM 
        Products AS P WITH(NOLOCK)
    JOIN 
        Stock AS S WITH(NOLOCK) ON P.ProductId = S.ProductId
    WHERE 
        S.StockId = @StockId;

END;
GO

