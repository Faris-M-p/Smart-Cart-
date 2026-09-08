SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[GetProducts]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SearchName NVARCHAR(255) = NULL,
    @SortColumn INT = 4,
    @SortMode NVARCHAR(10) = N'DESC',
    @CategoryIds NVARCHAR(MAX) = NULL,
    @SubCategoryIds NVARCHAR(MAX) = NULL,
    @BrandIds NVARCHAR(MAX) = NULL,
    @PriceFrom DECIMAL(18, 2) = NULL,
    @PriceTo DECIMAL(18, 2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageIndex IS NULL OR @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize IS NULL OR @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 50 SET @PageSize = 50;
    IF @SearchName = N'' SET @SearchName = NULL;
    IF @CategoryIds = N'' SET @CategoryIds = NULL;
    IF @SubCategoryIds = N'' SET @SubCategoryIds = NULL;
    IF @BrandIds = N'' SET @BrandIds = NULL;
    IF @PriceFrom IS NOT NULL AND @PriceFrom <= 0 SET @PriceFrom = NULL;
    IF @PriceTo IS NOT NULL AND @PriceTo <= 0 SET @PriceTo = NULL;
    IF @PriceFrom IS NOT NULL AND @PriceTo IS NOT NULL AND @PriceFrom > @PriceTo
    BEGIN
        DECLARE @Swap DECIMAL(18, 2) = @PriceFrom;
        SET @PriceFrom = @PriceTo;
        SET @PriceTo = @Swap;
    END;

    DECLARE @SubIds TABLE (Id INT PRIMARY KEY);
    DECLARE @HasSubFilter BIT = 0;

    INSERT INTO @SubIds (Id)
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(ISNULL(@SubCategoryIds, N''), N',')
    WHERE TRY_CAST(value AS INT) > 0;

    IF @SubCategoryIds IS NOT NULL
        SET @HasSubFilter = 1;

    IF @CategoryIds IS NOT NULL
    BEGIN
        SET @HasSubFilter = 1;

        DECLARE @FromCats TABLE (Id INT PRIMARY KEY);
        INSERT INTO @FromCats (Id)
        SELECT DISTINCT SC.ID_SubCategory
        FROM SubCategory AS SC WITH (NOLOCK)
        WHERE ISNULL(SC.Cancelled, 0) = 0
          AND SC.FK_Category IN (
              SELECT TRY_CAST(value AS INT)
              FROM STRING_SPLIT(@CategoryIds, N',')
              WHERE TRY_CAST(value AS INT) > 0
          );

        IF EXISTS (SELECT 1 FROM @SubIds)
            DELETE FROM @SubIds WHERE Id NOT IN (SELECT Id FROM @FromCats);
        ELSE
            INSERT INTO @SubIds (Id)
            SELECT Id FROM @FromCats;
    END;

    DECLARE @BrandFilter TABLE (Id INT PRIMARY KEY);
    INSERT INTO @BrandFilter (Id)
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(ISNULL(@BrandIds, N''), N',')
    WHERE TRY_CAST(value AS INT) > 0;

    ;WITH SkuPrice AS (
        SELECT
            PV.FK_Product,
            MIN(PV.SellingPrice) AS MinPrice
        FROM ProductVariants AS PV WITH (NOLOCK)
        WHERE ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(PV.SellOnline, 0) = 1
        GROUP BY PV.FK_Product
    ),
    CheapestSku AS (
        SELECT
            PV.FK_Product,
            PV.SellingPrice,
            PV.MRP,
            ROW_NUMBER() OVER (
                PARTITION BY PV.FK_Product
                ORDER BY PV.SellingPrice ASC, PV.IsDefault DESC, PV.ID_ProductVariant ASC
            ) AS rn
        FROM ProductVariants AS PV WITH (NOLOCK)
        WHERE ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(PV.SellOnline, 0) = 1
    ),
    StockByProduct AS (
        SELECT
            PV.FK_Product,
            SUM(S.Quantity) AS Qty
        FROM ProductVariants AS PV WITH (NOLOCK)
        INNER JOIN Stock AS S WITH (NOLOCK)
            ON S.FK_ProductVariant = PV.ID_ProductVariant
           AND ISNULL(S.Cancelled, 0) = 0
        WHERE ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(PV.SellOnline, 0) = 1
        GROUP BY PV.FK_Product
    ),
    ProductImage AS (
        SELECT
            PM.FK_Product,
            PM.MediaUrl,
            ROW_NUMBER() OVER (
                PARTITION BY PM.FK_Product
                ORDER BY PM.IsPrimary DESC, PM.DisplayOrder ASC, PM.ID_ProductMedia ASC
            ) AS rn
        FROM ProductMedia AS PM WITH (NOLOCK)
        WHERE PM.MediaType = N'Image'
          AND PM.MediaUrl IS NOT NULL
          AND PM.MediaUrl <> N''
    ),
    SkuImage AS (
        SELECT
            PV.FK_Product,
            SM.MediaUrl,
            ROW_NUMBER() OVER (
                PARTITION BY PV.FK_Product
                ORDER BY SM.IsPrimary DESC, SM.DisplayOrder ASC
            ) AS rn
        FROM ProductVariants AS PV WITH (NOLOCK)
        INNER JOIN SkuMedia AS SM WITH (NOLOCK)
            ON SM.FK_ProductSKU = PV.ID_ProductVariant
        WHERE ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(PV.SellOnline, 0) = 1
          AND SM.MediaType = N'Image'
          AND SM.MediaUrl IS NOT NULL
          AND SM.MediaUrl <> N''
    ),
    Filtered AS (
        SELECT
            P.ID_Product AS ProductId,
            P.Name,
            P.Slug,
            C.ID_Category AS CategoryId,
            C.Name AS CategoryName,
            P.FK_SubCategory AS SubCategoryId,
            ISNULL(P.FK_Brand, 0) AS BrandId,
            ISNULL(B.BrandName, N'') AS BrandName,
            ISNULL(PI.MediaUrl, SI.MediaUrl) AS ImageUrl,
            CAST(0 AS INT) AS Rating,
            CAST(N'' AS NVARCHAR(10)) AS Gender,
            ISNULL(CS.SellingPrice, 0) AS Price,
            ISNULL(CS.MRP, 0) AS MRP,
            CAST(CASE WHEN ISNULL(ST.Qty, 0) > 0 THEN 1 ELSE 0 END AS BIT) AS InStock,
            P.CreatedAt
        FROM Products AS P WITH (NOLOCK)
        INNER JOIN SubCategory AS SC WITH (NOLOCK) ON SC.ID_SubCategory = P.FK_SubCategory
        INNER JOIN Category AS C WITH (NOLOCK) ON C.ID_Category = SC.FK_Category
        LEFT JOIN Brand AS B WITH (NOLOCK) ON B.ID_Brand = P.FK_Brand
        LEFT JOIN SkuPrice AS SP ON SP.FK_Product = P.ID_Product
        LEFT JOIN CheapestSku AS CS ON CS.FK_Product = P.ID_Product AND CS.rn = 1
        LEFT JOIN StockByProduct AS ST ON ST.FK_Product = P.ID_Product
        LEFT JOIN ProductImage AS PI ON PI.FK_Product = P.ID_Product AND PI.rn = 1
        LEFT JOIN SkuImage AS SI ON SI.FK_Product = P.ID_Product AND SI.rn = 1
        WHERE ISNULL(P.Cancelled, 0) = 0
          AND P.IsActive = 1
          AND ISNULL(P.SellOnline, 0) = 1
          AND (
                @SearchName IS NULL
                OR P.Name LIKE N'%' + @SearchName + N'%'
                OR P.Slug LIKE N'%' + @SearchName + N'%'
              )
          AND (@HasSubFilter = 0 OR P.FK_SubCategory IN (SELECT Id FROM @SubIds))
          AND (NOT EXISTS (SELECT 1 FROM @BrandFilter) OR P.FK_Brand IN (SELECT Id FROM @BrandFilter))
          AND (@PriceFrom IS NULL OR SP.MinPrice >= @PriceFrom)
          AND (@PriceTo IS NULL OR SP.MinPrice <= @PriceTo)
    )
    SELECT
        ProductId,
        Name,
        Slug,
        CategoryId,
        CategoryName,
        SubCategoryId,
        BrandId,
        BrandName,
        ImageUrl,
        Rating,
        Gender,
        Price,
        MRP,
        InStock,
        CreatedAt
    INTO #PageRows
    FROM Filtered;

    DECLARE @TotalCount INT = (SELECT COUNT(*) FROM #PageRows);

    SELECT
        ProductId,
        Name,
        Slug,
        CategoryId,
        CategoryName,
        SubCategoryId,
        BrandId,
        BrandName,
        ImageUrl,
        Rating,
        Gender,
        Price,
        MRP,
        InStock
    FROM #PageRows
    ORDER BY
        CASE WHEN @SortColumn = 1 AND @SortMode = N'ASC' THEN Name END ASC,
        CASE WHEN @SortColumn = 1 AND @SortMode = N'DESC' THEN Name END DESC,
        CASE WHEN @SortColumn = 2 AND @SortMode = N'ASC' THEN Price END ASC,
        CASE WHEN @SortColumn = 2 AND @SortMode = N'DESC' THEN Price END DESC,
        CASE WHEN @SortColumn = 3 AND @SortMode = N'ASC' THEN SubCategoryId END ASC,
        CASE WHEN @SortColumn = 3 AND @SortMode = N'DESC' THEN SubCategoryId END DESC,
        CASE WHEN @SortColumn = 4 AND @SortMode = N'ASC' THEN CreatedAt END ASC,
        CASE WHEN @SortColumn = 4 AND @SortMode = N'DESC' THEN CreatedAt END DESC,
        CASE WHEN @SortColumn = 0 AND @SortMode = N'ASC' THEN ProductId END ASC,
        CASE WHEN ISNULL(@SortColumn, 4) NOT IN (0, 1, 2, 3) AND @SortMode = N'ASC' THEN CreatedAt END ASC,
        CASE WHEN ISNULL(@SortColumn, 4) NOT IN (0, 1, 2, 3) AND @SortMode <> N'ASC' THEN CreatedAt END DESC,
        ProductId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT
        @TotalCount AS TotalCount,
        @PageSize AS PageSize,
        @PageIndex AS PageIndex;

    DROP TABLE #PageRows;
END;
GO
