/****** Object:  StoredProcedure [dbo].[ProVariantValueListSelect]    Script Date: 13-01-2026 22:07:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Variant Value List with Search, Filter, Sorting, Pagination
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProVariantValueListSelect]
(
    @SearchText NVARCHAR(255) = '',
    @FilterVariantIDs NVARCHAR(MAX) = '',    -- JSON: [{ "ID_Value":1 }]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = ''                -- ASC / DESC
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
    CREATE TABLE #tmpVariantValue
    (
        ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        ID_VariantValue INT,
        FK_Variant INT,
        ValueName NVARCHAR(255),
        Description NVARCHAR(500),
        ValueIcon NVARCHAR(MAX),
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
        INSERT INTO #tmpVariantValue
        (
            ID_VariantValue, FK_Variant, ValueName, Description, ValueIcon,
            DisplayOrder, CreatedOn, Cancelled, CancelledOn, CancelledReason
        )
        SELECT
            VV.ID_VariantValue,
            VV.FK_Variant,
            VV.ValueName,
            VV.Description,
            VV.ValueIcon,
            VV.DisplayOrder,
            VV.CreatedOn,
            VV.Cancelled,
            VV.CancelledOn,
            VV.CancelledReason
        FROM VariantValue VV WITH(NOLOCK)
        WHERE 1 = 1
    ';

    -------------------------------------------------------------------
    -- SEARCH
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += ' AND VV.ValueName LIKE ''%' + @SearchText + '%'' ';

    -------------------------------------------------------------------
    -- FILTER BY VARIANT IDs (JSON)
    -------------------------------------------------------------------
    IF (@FilterVariantIDs <> '' AND @FilterVariantIDs <> '[]')
    BEGIN
        SET @SqlStr += '
            AND VV.FK_Variant IN (
                SELECT ID_Value FROM OPENJSON(@FilterVariantIDs)
                WITH (ID_Value INT ''$.ID_Value'')
            )
        ';
    END

    -------------------------------------------------------------------
    -- SORTING
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
        SET @SqlStr += ' ORDER BY VV.' + @SortColumn + ' ' + @SortMode;
    ELSE
        SET @SqlStr += ' ORDER BY VV.ID_VariantValue DESC';

    -------------------------------------------------------------------
    -- EXECUTE INSERT QUERY
    -------------------------------------------------------------------
    EXEC sp_executesql 
        @SqlStr,
        N'@FilterVariantIDs NVARCHAR(MAX)',
        @FilterVariantIDs;

    SET @TotalCount = @@ROWCOUNT;

    -------------------------------------------------------------------
    -- PAGINATION
    -------------------------------------------------------------------
    SELECT 
        ID_VariantValue,
        FK_Variant,
        ValueName,
        Description,
        ValueIcon,
        DisplayOrder,
        CreatedOn,
        Cancelled,
        CancelledOn,
        CancelledReason
    FROM #tmpVariantValue
    WHERE ID BETWEEN ((@PageIndex - 1) * @PageSize + 1)
              AND (@PageIndex * @PageSize);

    SELECT @TotalCount AS TotalCount,
           @PageIndex AS PageIndex,
           @PageSize AS PageSize;

    DROP TABLE #tmpVariantValue;
END;

