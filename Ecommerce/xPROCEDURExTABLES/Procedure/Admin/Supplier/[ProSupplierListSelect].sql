USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProSupplierListSelect]    Script Date: 13-01-2026 22:06:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProSupplierListSelect
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Admin-side listing of Supplier Master with:
    • Search (by name, contact, phone, email, GST)
    • Pagination
    • Sorting
    • Cancelled/Active filter

USED BY
  ✔ Admin Panel → Supplier List Page
  ✔ Purchase Module (Supplier dropdown with search)
  ✔ Inventory Team (Supplier verification)

INPUT PARAMETERS
  @SearchText          → Search supplier fields
  @FilterSupplierIDs   → JSON: [{ "ID_Value": 3 }, { "ID_Value": 7 }]
  @ShowCancelled       → 0 = Only active, 1 = Show all
  @PageIndex           → Pagination index
  @PageSize            → Page size
  @SortColumn          → Column name
  @SortMode            → ASC / DESC

OUTPUT
  1) Supplier list (paged)
  2) TotalCount summary

**********************************************************************/
ALTER   PROCEDURE [dbo].[ProSupplierListSelect]
(
    @SearchText NVARCHAR(255) = '',
    @FilterSupplierIDs NVARCHAR(MAX) = '',
    @ShowCancelled BIT = 0,     -- 0=active only, 1=all
    @PageIndex INT = 1,
    @PageSize INT = 20,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = 'DESC'
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
    CREATE TABLE #tmpSuppliers
    (
        RowID BIGINT IDENTITY(1,1),
        ID_Supplier BIGINT,
        SupplierName NVARCHAR(255),
        ContactPerson NVARCHAR(100),
        Phone NVARCHAR(20),
        Email NVARCHAR(255),
        GSTNumber NVARCHAR(50),
        Address NVARCHAR(500),
        CreatedOn DATETIME,
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(500)
    );

    -------------------------------------------------------------------
    -- BASE QUERY
    -------------------------------------------------------------------
    SET @SqlStr = N'
        INSERT INTO #tmpSuppliers
        (
            ID_Supplier,
            SupplierName,
            ContactPerson,
            Phone,
            Email,
            GSTNumber,
            Address,
            CreatedOn,
            Cancelled,
            CancelledOn,
            CancelledReason
        )
        SELECT 
            s.ID_Supplier,
            s.SupplierName,
            s.ContactPerson,
            s.Phone,
            s.Email,
            s.GSTNumber,
            s.Address,
            s.CreatedOn,
            s.Cancelled,
            s.CancelledOn,
            s.CancelledReason
        FROM Supplier s WITH(NOLOCK)
        WHERE 1 = 1
    ';

    -------------------------------------------------------------------
    -- SHOW ONLY ACTIVE SUPPLIERS?
    -------------------------------------------------------------------
    IF (@ShowCancelled = 0)
        SET @SqlStr += ' AND s.Cancelled = 0 ';

    -------------------------------------------------------------------
    -- SEARCH FILTER
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
        SET @SqlStr += '
            AND (
                s.SupplierName LIKE ''%' + @SearchText + '%'' OR
                s.ContactPerson LIKE ''%' + @SearchText + '%'' OR
                s.Phone LIKE ''%' + @SearchText + '%'' OR
                s.Email LIKE ''%' + @SearchText + '%'' OR
                s.GSTNumber LIKE ''%' + @SearchText + '%''
            )
        ';

    -------------------------------------------------------------------
    -- SUPPLIER ID FILTER USING JSON
    -------------------------------------------------------------------
    IF (@FilterSupplierIDs <> '' AND @FilterSupplierIDs <> '[]')
        SET @SqlStr += '
            AND s.ID_Supplier IN (
                SELECT ID_Value FROM OPENJSON(@FilterSupplierIDs)
                WITH (ID_Value BIGINT ''$.ID_Value'')
            )
        ';

    -------------------------------------------------------------------
    -- SORTING SAFELY
    -------------------------------------------------------------------
    IF (@SortColumn <> '' AND @SortMode <> '')
    BEGIN
        SET @SqlStr += ' ORDER BY ' +
            CASE 
                WHEN @SortColumn = 'SupplierName' THEN 's.SupplierName'
                WHEN @SortColumn = 'CreatedOn' THEN 's.CreatedOn'
                WHEN @SortColumn = 'Phone' THEN 's.Phone'
                WHEN @SortColumn = 'GSTNumber' THEN 's.GSTNumber'
                ELSE 's.ID_Supplier'
            END + ' ' + @SortMode;
    END
    ELSE
        SET @SqlStr += ' ORDER BY s.ID_Supplier DESC';

    -------------------------------------------------------------------
    -- EXEC QUERY
    -------------------------------------------------------------------
    EXEC sp_executesql @SqlStr,
        N'@FilterSupplierIDs NVARCHAR(MAX)',
        @FilterSupplierIDs;

    SET @TotalCount = @@ROWCOUNT;

    -------------------------------------------------------------------
    -- PAGINATION RESULT
    -------------------------------------------------------------------
    SELECT 
        ID_Supplier,
        SupplierName,
        ContactPerson,
        Phone,
        Email,
        GSTNumber,
        Address,
        CreatedOn,
        Cancelled,
        CancelledOn,
        CancelledReason
    FROM #tmpSuppliers
    WHERE RowID BETWEEN ((@PageIndex - 1) * @PageSize + 1)
                    AND (@PageIndex * @PageSize);

    -------------------------------------------------------------------
    -- SUMMARY
    -------------------------------------------------------------------
    SELECT 
        @TotalCount AS TotalCount,
        @PageIndex AS PageIndex,
        @PageSize AS PageSize;

    DROP TABLE #tmpSuppliers;
END;
