USE [Ecommerse]
GO

/****** Object:  StoredProcedure [dbo].[GetProducts]    Script Date: 18-01-2025 23:02:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Altering the GetProducts stored procedure
CREATE PROCEDURE [dbo].[GetProducts]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SearchName NVARCHAR(255) = NULL,
    @SortField NVARCHAR(50) = 'ProductId',
    @CategoryIds NVARCHAR(MAX) = NULL,
    @SubCategoryIds NVARCHAR(MAX) = NULL,
    @BrandIds NVARCHAR(MAX) = NULL,
    @Ratings NVARCHAR(MAX) = NULL,
    @Gender NVARCHAR(10) = NULL,
    @PriceFrom DECIMAL(18, 2) = 50,
    @PriceTo DECIMAL(18, 2) = 60,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Temporary table to store filtered data
    CREATE TABLE #FilteredProducts (
        StockId INT,
		ProductId INT,
        Name NVARCHAR(255),
        CategoryId INT,
        SubCategoryId INT,
        BrandId INT,
        Rating INT,
        Gender NVARCHAR(10),
        Price DECIMAL(18, 2),
        MRP DECIMAL(18, 2)
    );

    -- Inserting filtered data into the temporary table with stock quantity condition
    INSERT INTO #FilteredProducts
    SELECT 
	    s.StockId,
        P.ProductId, 
        P.Name, 
        P.CategoryId, 
        P.SubCategoryId, 
        P.BrandId,
        S.Rating, 
        P.Gender, 
        S.Price,
        S.MRP
    FROM 
        Products AS P WITH(NOLOCK)
    JOIN
        Stock AS S WITH(NOLOCK) ON P.ProductId = S.ProductId
    WHERE 
        (@SearchName IS NULL OR P.Name LIKE '%' + @SearchName + '%')
        AND (@CategoryIds IS NULL OR P.CategoryId IN (SELECT value FROM STRING_SPLIT(@CategoryIds, ',')))
        AND (@SubCategoryIds IS NULL OR P.SubCategoryId IN (SELECT value FROM STRING_SPLIT(@SubCategoryIds, ',')))
        AND (@BrandIds IS NULL OR P.BrandId IN (SELECT value FROM STRING_SPLIT(@BrandIds, ',')))
        AND (@Ratings IS NULL OR S.Rating IN (SELECT value FROM STRING_SPLIT(@Ratings, ',')))
        AND (@Status IS NULL OR P.StatusId IN (SELECT value FROM STRING_SPLIT(@Status, ',')))
        AND (@Gender IS NULL OR P.Gender = @Gender)
        AND (@PriceFrom IS NULL OR S.Price >= @PriceFrom)
        AND (@PriceTo IS NULL OR S.Price <= @PriceTo)
        AND S.Quantity > 0;

    -- Calculating total count of filtered records
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM #FilteredProducts;

    -- Selecting paginated data
    SELECT 
        ProductId, 
        Name, 
        CategoryId, 
        SubCategoryId, 
        BrandId,
        Rating, 
        Gender, 
        Price,
        MRP
    FROM 
        #FilteredProducts
    ORDER BY 
        CASE 
            WHEN @SortField = 'Price' THEN Price
            WHEN @SortField = 'Rating' THEN Rating
            WHEN @SortField = 'Name' THEN Name
            ELSE ProductId
        END
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Returning pagination metadata
    SELECT 
        @TotalCount AS TotalCount, 
        @PageSize AS PageSize, 
        @PageIndex AS PageIndex;

    -- Dropping the temporary table
    DROP TABLE #FilteredProducts;
END;
GO

