/****** Object:  StoredProcedure [dbo].[ProProductVariantStockSelect]    Script Date: 13-01-2026 22:05:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantStockSelect
Created By       : Muhammed Faris
Created On       : 12/12/2025

====================  PROCEDURE PURPOSE  =====================
Fetch stock batches for a given Product Variant (SKU).  
Used by admin/inventory modules to view:

    - Stock quantity by batch
    - Purchase info
    - Supplier info
    - Expiry date
    - Total available stock
    - Pagination + sorting

====================  WHO USES THIS?  =====================
✔ Admin Panel  
✔ Inventory Team  
✔ Purchase Module  
❌ NOT used by normal users

====================  PARAMETERS  =====================
@FK_ProductVariant  → Required SKU ID  
@IncludeCancelled   → 0 = only active, 1 = include cancelled  
@PageIndex          → pagination  
@PageSize           → records per page  
@SortColumn         → dynamic sorting  
@SortMode           → ASC/DESC  

====================  OUTPUT  =====================
1) List of stock batches  
2) Summary:
      - TotalStock  
      - TotalBatchCount  
      - PageIndex  
      - PageSize  

**********************************************************************/

CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantStockSelect]
(
    @FK_ProductVariant INT,
    @IncludeCancelled BIT = 0,
    @PageIndex INT = 1,
    @PageSize INT = 20,
    @SortColumn VARCHAR(50) = '',
    @SortMode VARCHAR(5) = 'ASC'
)
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (@FK_ProductVariant = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid FK_ProductVariant.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM ProductVariant 
        WHERE ID_ProductVariant = @FK_ProductVariant AND Cancelled = 0
    )
    BEGIN
        SELECT -1 AS ResponseCode, 'ProductVariant does not exist or is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    -------------------------------------------------------------------
    -- TEMP TABLE FOR PAGINATION
    -------------------------------------------------------------------
    CREATE TABLE #tmpStock
    (
        RowID BIGINT IDENTITY(1,1),
        ID_Stock INT,
        FK_ProductVariant INT,
        FK_PurchaseDetail INT,
        Quantity INT,
        CreatedOn DATETIME,
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(500),
        PurchaseDate DATE,
        PurchasePrice DECIMAL(10,2),
        MRP DECIMAL(10,2),
        ExpiryDate DATE,
        SupplierName NVARCHAR(255)
    );

    -------------------------------------------------------------------
    -- INSERT STOCK BATCHES
    -------------------------------------------------------------------
    INSERT INTO #tmpStock
    (
        ID_Stock,
        FK_ProductVariant,
        FK_PurchaseDetail,
        Quantity,
        CreatedOn,
        Cancelled,
        CancelledOn,
        CancelledReason,
        PurchaseDate,
        PurchasePrice,
        MRP,
        ExpiryDate,
        SupplierName
    )
    SELECT
        s.ID_Stock,
        s.FK_ProductVariant,
        s.FK_PurchaseDetail,
        s.Quantity,
        s.CreatedOn,
        s.Cancelled,
        s.CancelledOn,
        s.CancelledReason,

        p.PurchaseDate,
        pd.PurchasePrice,
        pd.MRP,
        pd.ExpiryDate,

        sup.Name
    FROM Stock s  WITH(NOLOCK)
    INNER JOIN PurchaseDetail pd ON pd.ID_PurchaseDetail = s.FK_PurchaseDetail
    INNER JOIN Purchase p ON p.ID_Purchase = pd.FK_Purchase
    INNER JOIN Supplier sup ON sup.ID_Supplier = p.FK_Supplier
    WHERE 
        s.FK_ProductVariant = @FK_ProductVariant
        AND (@IncludeCancelled = 1 OR s.Cancelled = 0);

    -------------------------------------------------------------------
    -- TOTAL BATCHES & TOTAL STOCK
    -------------------------------------------------------------------
    DECLARE @TotalBatchCount INT = (SELECT COUNT(*) FROM #tmpStock);
    DECLARE @TotalStock INT = (
        SELECT ISNULL(SUM(Quantity), 0) 
        FROM #tmpStock 
        WHERE Cancelled = 0
    );

    -------------------------------------------------------------------
    -- DYNAMIC SORTING
    -------------------------------------------------------------------
    DECLARE @Sql NVARCHAR(MAX);

    SET @Sql = '
        SELECT 
            ID_Stock,
            FK_ProductVariant,
            FK_PurchaseDetail,
            Quantity,
            CreatedOn,
            Cancelled,
            CancelledOn,
            CancelledReason,
            PurchaseDate,
            PurchasePrice,
            MRP,
            ExpiryDate,
            SupplierName
        FROM #tmpStock
        ORDER BY ' +
        CASE 
            WHEN @SortColumn = 'Quantity' THEN 'Quantity'
            WHEN @SortColumn = 'CreatedOn' THEN 'CreatedOn'
            WHEN @SortColumn = 'PurchaseDate' THEN 'PurchaseDate'
            WHEN @SortColumn = 'ExpiryDate' THEN 'ExpiryDate'
            ELSE 'CreatedOn'  -- Default
        END + ' ' + @SortMode + '
        OFFSET ' + CAST((@PageIndex - 1) * @PageSize AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(@PageSize AS VARCHAR) + ' ROWS ONLY;
    ';

    EXEC(@Sql);

    -------------------------------------------------------------------
    -- SUMMARY
    -------------------------------------------------------------------
    SELECT 
        @TotalStock AS TotalStock,
        @TotalBatchCount AS TotalBatchCount,
        @PageIndex AS PageIndex,
        @PageSize AS PageSize;

    DROP TABLE #tmpStock;
END;

