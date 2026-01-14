USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProProductListSelect]    Script Date: 13-01-2026 22:03:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 09/12/2025
Purpose     : Product List with Search, Filters & Pagination
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProProductListSelect]
(
    @SearchText NVARCHAR(255) = '',
    @FilterCategoryIDs NVARCHAR(MAX) = '',      -- JSON: [{ "ID_Value": 1 }]
    @FilterSubCategoryIDs NVARCHAR(MAX) = '',   -- JSON: [{ "ID_Value": 3 }]
    @FilterBrandIDs NVARCHAR(MAX) = '',         -- JSON
    @FilterStatusIDs NVARCHAR(MAX) = '',        -- JSON
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = ''                   -- ASC / DESC
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @SqlStr NVARCHAR(MAX),
        @TotalCount BIGINT;

    -------------------------------------------------------------------
    -- TEMP TABLE
    -------------------------------------------------------------------
    CREATE TABLE #tmpProduct
    (
        ID BIGINT IDENTITY(1,1),
        ID_Product INT,
        Name NVARCHAR(255),
        Description NVARCHAR(MAX),
        Price DECIMAL(10,2),
        MRP DECIMAL(10,2),
        FK_Category INT,
        FK_SubCategory INT,
        FK_Brand INT,
        Rating DECIMAL(3,1),
        Gender NVARCHAR(50),
        FK_Status INT,
        CreatedOn DATETIME,
        UpdatedOn DATETIME,
        ImageData NVARCHAR(MAX),
        IsBase64 BIT
    );

    -------------------------------------------------------------------
    -- BASE QUERY
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpProduct
        (
            ID_Product, Name, Description, Price, MRP,
            FK_Category, FK_SubCategory, FK_Brand, Rating, Gender,
            FK_Status, CreatedOn, UpdatedOn,
            ImageData, IsBase64
        )
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
            P.UpdatedOn,

            -- First non-cancelled image
            (SELECT TOP 1 PI.ImageData 
             FROM ProductImage PI 
             WHERE PI.FK_Product = P.ID_Product AND PI.Cancelled = 0
             ORDER BY PI.ID_ProductImage ASC) AS ImageData,

            (SELECT TOP 1 PI.IsBase64 
             FROM ProductImage PI 
             WHERE PI.FK_Product = P.ID_Product AND PI.Cancelled = 0
             ORDER BY PI.ID_ProductImage ASC) AS IsBase64

        FROM Product P WITH(NOLOCK)
        WHERE P.Cancelled = 0
    ';

    -------------------------------------------------------------------
    -- SEARCH
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND P.Name LIKE ''%' + @SearchText + '%'' ';

    -------------------------------------------------------------------
    -- FILTER: SUBCATEGORY (Highest priority)
    -------------------------------------------------------------------
    IF (@FilterSubCategoryIDs <> '' AND @FilterSubCategoryIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND P.FK_SubCategory IN (
                SELECT ID_Value FROM OPENJSON(@FilterSubCategoryIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END
    ELSE
    -------------------------------------------------------------------
    -- FILTER: CATEGORY
    -------------------------------------------------------------------
    IF (@FilterCategoryIDs <> '' AND @FilterCategoryIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND P.FK_Category IN (
                SELECT ID_Value FROM OPENJSON(@FilterCategoryIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END

    -------------------------------------------------------------------
    -- FILTER: BRAND
    -------------------------------------------------------------------
    IF (@FilterBrandIDs <> '' AND @FilterBrandIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND P.FK_Brand IN (
                SELECT ID_Value FROM OPENJSON(@FilterBrandIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END

    -------------------------------------------------------------------
    -- FILTER: STATUS
    -------------------------------------------------------------------
    IF (@FilterStatusIDs <> '' AND @FilterStatusIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND P.FK_Status IN (
                SELECT ID_Value FROM OPENJSON(@FilterStatusIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END

    -------------------------------------------------------------------
    -- ORDER BY
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY P.' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY P.ID_Product DESC ';

    -------------------------------------------------------------------
    -- EXEC QUERY
    -------------------------------------------------------------------
    EXEC sp_executesql @SqlStr,
        N'@FilterCategoryIDs NVARCHAR(MAX), 
          @FilterSubCategoryIDs NVARCHAR(MAX),
          @FilterBrandIDs NVARCHAR(MAX),
          @FilterStatusIDs NVARCHAR(MAX)',
        @FilterCategoryIDs, @FilterSubCategoryIDs, @FilterBrandIDs, @FilterStatusIDs;

    SET @TotalCount = @@ROWCOUNT;

    -------------------------------------------------------------------
    -- PAGINATION RESULT
    -------------------------------------------------------------------
    SELECT 
        ID_Product,
        Name,
        Description,
        Price,
        MRP,
        FK_Category,
        FK_SubCategory,
        FK_Brand,
        Rating,
        Gender,
        FK_Status,
        CreatedOn,
        UpdatedOn,
        ImageData,
        IsBase64
    FROM #tmpProduct
    WHERE ID BETWEEN ((@PageIndex - 1) * @PageSize + 1)
                AND (@PageIndex * @PageSize);

    -------------------------------------------------------------------
    -- META INFO
    -------------------------------------------------------------------
    SELECT @TotalCount AS TotalCount,
           @PageIndex AS PageIndex,
           @PageSize AS PageSize;

    DROP TABLE #tmpProduct;
END
