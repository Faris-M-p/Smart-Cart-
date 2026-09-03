/****** Object:  StoredProcedure [dbo].[ProBrandListSelect]    Script Date: 14-01-2026 00:00:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Select Brand List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProBrandListSelect]
    @SearchText NVARCHAR(255) = '',
    @FilterBrandIDs NVARCHAR(MAX) = '',
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
    CREATE TABLE #tmpBrands
    (
        ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        BrandID INT,
        BrandName NVARCHAR(100),
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(255)
    );

    -------------------------------------------------------------------
    -- Base Query
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpBrands
        (
            BrandID, BrandName, Cancelled, CancelledOn, CancelledReason
        )
        SELECT 
            B.BrandId,
            B.BrandName,
            B.Cancelled,
            B.CancelledOn,
            B.CancelledReason
        FROM Brands B WITH(NOLOCK)
        WHERE 1 = 1
    ';


    -------------------------------------------------------------------
    -- Search by BrandName
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND B.BrandName LIKE ''%' + @SearchText + '%''';


    -------------------------------------------------------------------
    -- Filter using JSON Brand ID list
    -- Example JSON:
    -- [ {"ID_Value":2}, {"ID_Value":5} ]
    -------------------------------------------------------------------
    IF (@FilterBrandIDs <> '' AND @FilterBrandIDs <> '[]')
        SET @SqlStr += '
            AND B.BrandId IN (
                SELECT ID_Value FROM OPENJSON(@FilterBrandIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';


    -------------------------------------------------------------------
    -- Sorting
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY ' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY B.BrandId DESC';   -- Default


    -------------------------------------------------------------------
    -- Execute Insert Into Temp Table
    -------------------------------------------------------------------
    EXEC sp_executesql @SqlStr,
        N'@FilterBrandIDs NVARCHAR(MAX)',
        @FilterBrandIDs;

    SET @TotalCount = @@ROWCOUNT;


    -------------------------------------------------------------------
    -- Pagination
    -------------------------------------------------------------------
    IF (@PageIndex > 0 AND @PageSize > 0)
    BEGIN
        SELECT 
            BrandID,
            BrandName,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpBrands
        WHERE ID >= ((@PageIndex - 1) * @PageSize) + 1
          AND ID <= (((@PageIndex - 1) * @PageSize) + @PageSize);

        SELECT @TotalCount AS TotalCount, @PageIndex AS PageIndex, @PageSize AS PageSize;
    END
    ELSE
    BEGIN
        -- If no pagination â†’ show all
        SELECT 
            BrandID,
            BrandName,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpBrands;

        SELECT @TotalCount AS TotalCount, @PageIndex AS PageIndex, @PageSize AS PageSize;
    END

    DROP TABLE #tmpBrands;
END

