USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProSubCategoryListSelect]    Script Date: 13-01-2026 22:06:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Select SubCategory List with Filtering & Pagination
------------------------------------------------------------------------
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProSubCategoryListSelect]
    @SearchText NVARCHAR(255) = '',
    @FilterCategoryIDs NVARCHAR(MAX) = '',      -- JSON: [{ "ID_Value": 1 }]
    @FilterSubCategoryIDs NVARCHAR(MAX) = '',   -- JSON: [{ "ID_Value": 3 }]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = ''                   -- ASC or DESC
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @SqlStr NVARCHAR(MAX),
        @TotalCount BIGINT;

    -------------------------------------------------------------------
    -- TEMP TABLE
    -------------------------------------------------------------------
    CREATE TABLE #tmpSubCat
    (
        ID BIGINT IDENTITY(1,1),
        ID_SubCategory INT,
        SubCategoryName NVARCHAR(255),
        FK_Category INT,
        Description NVARCHAR(500),
        CreatedDate DATETIME,
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(500)
    );

    -------------------------------------------------------------------
    -- BASE QUERY
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpSubCat
        (
            ID_SubCategory, SubCategoryName, FK_Category, Description,
            CreatedDate, Cancelled, CancelledOn, CancelledReason
        )
        SELECT
            S.ID_SubCategory,
            S.SubCategoryName,
            S.FK_Category,
            S.Description,
            S.CreatedDate,
            S.Cancelled,
            S.CancelledOn,
            S.CancelledReason
        FROM SubCategory S WITH(NOLOCK)
        WHERE 1 = 1
          AND S.Cancelled = 0
    ';

    -------------------------------------------------------------------
    -- SEARCH FILTER
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND S.SubCategoryName LIKE ''%' + @SearchText + '%'' ';

    -------------------------------------------------------------------
    -- FILTER BY SUBCATEGORY IDs (HIGHEST PRIORITY)
    -------------------------------------------------------------------
    IF (@FilterSubCategoryIDs <> '' AND @FilterSubCategoryIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND S.ID_SubCategory IN (
                SELECT ID_Value FROM OPENJSON(@FilterSubCategoryIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END
    ELSE
    -------------------------------------------------------------------
    -- FILTER BY CATEGORY IDs
    -------------------------------------------------------------------
    IF (@FilterCategoryIDs <> '' AND @FilterCategoryIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND S.FK_Category IN (
                SELECT ID_Value FROM OPENJSON(@FilterCategoryIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END;

    -------------------------------------------------------------------
    -- SORTING
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY ' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY S.ID_SubCategory DESC';

    -------------------------------------------------------------------
    -- EXECUTE SQL
    -------------------------------------------------------------------
    EXEC sp_executesql @SqlStr,
        N'@FilterCategoryIDs NVARCHAR(MAX), @FilterSubCategoryIDs NVARCHAR(MAX)',
        @FilterCategoryIDs, @FilterSubCategoryIDs;

    SET @TotalCount = @@ROWCOUNT;

    -------------------------------------------------------------------
    -- PAGINATION OUTPUT
    -------------------------------------------------------------------
    SELECT 
        ID_SubCategory, SubCategoryName, FK_Category, Description,
        CreatedDate, Cancelled, CancelledOn, CancelledReason
    FROM #tmpSubCat
    WHERE ID BETWEEN ((@PageIndex - 1) * @PageSize + 1)
                AND ((@PageIndex - 1) * @PageSize + @PageSize);

    -------------------------------------------------------------------
    -- PAGINATION META
    -------------------------------------------------------------------
    SELECT @TotalCount AS TotalCount,
           @PageIndex AS PageIndex,
           @PageSize AS PageSize;

    DROP TABLE #tmpSubCat;
END
