/****** Object:  StoredProcedure [dbo].[ProVariantListSelect]    Script Date: 13-01-2026 22:07:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Variant List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------
Modification
On          By                  Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProVariantListSelect]
(
    @SearchText NVARCHAR(255) = '',
    @FilterVariantIDs NVARCHAR(MAX) = '',   -- JSON: [{ "ID_Value": 1 }]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = ''               -- ASC / DESC
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
    CREATE TABLE #tmpVariant
    (
        ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        ID_Variant INT,
        VariantName NVARCHAR(255),
        Description NVARCHAR(500),
        DisplayOrder INT,
        CreatedOn DATETIME,
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(500)
    );

    -------------------------------------------------------------------
    -- BASE QUERY
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpVariant
        (
            ID_Variant, VariantName, Description, DisplayOrder,
            CreatedOn, Cancelled, CancelledOn, CancelledReason
        )
        SELECT
            V.ID_Variant,
            V.VariantName,
            V.Description,
            V.DisplayOrder,
            V.CreatedOn,
            V.Cancelled,
            V.CancelledOn,
            V.CancelledReason
        FROM Variant V WITH(NOLOCK)
        WHERE 1 = 1
    ';

    -------------------------------------------------------------------
    -- SEARCH
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND V.VariantName LIKE ''%' + @SearchText + '%'' ';

    -------------------------------------------------------------------
    -- FILTER BY VARIANT IDs (JSON)
    -------------------------------------------------------------------
    IF (@FilterVariantIDs <> '' AND @FilterVariantIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND V.ID_Variant IN (
                SELECT ID_Value FROM OPENJSON(@FilterVariantIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END

    -------------------------------------------------------------------
    -- SORTING
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY V.' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY V.ID_Variant DESC';

    -------------------------------------------------------------------
    -- EXECUTESQL
    -------------------------------------------------------------------
    EXEC sp_executesql 
        @SqlStr,
        N'@FilterVariantIDs NVARCHAR(MAX)',
        @FilterVariantIDs;

    SET @TotalCount = @@ROWCOUNT;

    -------------------------------------------------------------------
    -- PAGINATION RESULT
    -------------------------------------------------------------------
    IF (@PageIndex > 0 AND @PageSize > 0)
    BEGIN
        SELECT 
            ID_Variant,
            VariantName,
            Description,
            DisplayOrder,
            CreatedOn,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpVariant
        WHERE ID BETWEEN ((@PageIndex - 1) * @PageSize + 1)
                      AND (@PageIndex * @PageSize);

        SELECT @TotalCount AS TotalCount, 
               @PageIndex AS PageIndex, 
               @PageSize AS PageSize;
    END
    ELSE
    BEGIN
        SELECT 
            ID_Variant,
            VariantName,
            Description,
            DisplayOrder,
            CreatedOn,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpVariant;

        SELECT @TotalCount AS TotalCount, 
               @PageIndex AS PageIndex, 
               @PageSize AS PageSize;
    END

    DROP TABLE #tmpVariant;
END

