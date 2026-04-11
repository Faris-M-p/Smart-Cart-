USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProCategoryListSelect]    Script Date: 13-01-2026 21:57:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Select Category List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProCategoryListSelect]
    @SearchText NVARCHAR(255) = '',
    @FilterCategoryIDs NVARCHAR(MAX) = '',
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = ''       -- ASC / DESC
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @SqlStr NVARCHAR(MAX),
        @TotalCount BIGINT;

    -------------------------------------------------------------------
    -- Temporary Table
    -------------------------------------------------------------------
    CREATE TABLE #tmpCategories
    (
        ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        ID_Category INT,
        [Name] NVARCHAR(255),
        [Description] NVARCHAR(1000),
        [IsActive] BIT, 
        CreatedDate DATETIME,      
    );

    -------------------------------------------------------------------
    -- Base Query
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpCategories
        (
            ID_Category, Name, Description, CreatedDate
            
        )
        SELECT 
            C.CategoryID,
            C.CategoryName,
            C.Description,
            C.CreatedDate,
            C.Cancelled,
            C.CancelledOn,
            C.CancelledReason
        FROM Categories C WITH(NOLOCK)
        WHERE 1 = 1
    ';


    -------------------------------------------------------------------
    -- Search by CategoryName
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND C.CategoryName LIKE ''%' + @SearchText + '%''';


    -------------------------------------------------------------------
    -- Filter using JSON Category ID list
    -- Example JSON:
    -- [ {"ID_Value":2}, {"ID_Value":5} ]
    -------------------------------------------------------------------
    IF (@FilterCategoryIDs <> '' AND @FilterCategoryIDs <> '[]')
        SET @SqlStr += '
            AND C.CategoryID IN (
                SELECT ID_Value FROM OPENJSON(@FilterCategoryIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';


    -------------------------------------------------------------------
    -- Sorting
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY ' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY C.CategoryID DESC';   -- Default


    -------------------------------------------------------------------
    -- Execute Insert Into Temp Table
    -------------------------------------------------------------------
    EXEC sp_executesql @SqlStr,
        N'@FilterCategoryIDs NVARCHAR(MAX)',
        @FilterCategoryIDs;

    SET @TotalCount = @@ROWCOUNT;


    -------------------------------------------------------------------
    -- Pagination
    -------------------------------------------------------------------
    IF (@PageIndex > 0 AND @PageSize > 0)
    BEGIN
        SELECT 
            CategoryID,
            CategoryName,
            Description,
            CreatedDate,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpCategories
        WHERE ID >= ((@PageIndex - 1) * @PageSize) + 1
          AND ID <= (((@PageIndex - 1) * @PageSize) + @PageSize);

        SELECT @TotalCount AS TotalCount, @PageIndex AS PageIndex, @PageSize AS PageSize;
    END
    ELSE
    BEGIN
        -- If no pagination → show all
        SELECT 
            CategoryID,
            CategoryName,
            Description,
            CreatedDate,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpCategories;

        SELECT @TotalCount AS TotalCount, @PageIndex AS PageIndex, @PageSize AS PageSize;
    END

    DROP TABLE #tmpCategories;
END
