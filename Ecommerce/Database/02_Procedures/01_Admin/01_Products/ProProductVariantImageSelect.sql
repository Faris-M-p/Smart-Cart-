/****** Object:  StoredProcedure [dbo].[ProProductVariantImageSelect]    Script Date: 13-01-2026 22:04:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantImageSelect
Created By       : Muhammed Faris
Created On       : 10/12/2025

====================  PROCEDURE PURPOSE  =====================
This procedure returns the list of all images for a specific 
Product Variant (SKU) in the SmartCart system.

It provides:
    • ImageURL
    • IsDefault status
    • Created date
    • Cancelled status (if needed)
    • Sorted output (Default → CreatedOn → ID)

====================  BUSINESS USE CASES  =====================
1) Admin opens Variant → Image Gallery
2) Admin wants to reorder or review SKU images
3) Admin wants to show all uploaded images of a variant
4) Display image list before delete/update/set-default operations

====================  WHO USES THIS PROCEDURE?  =====================
✔ Admin Panel  
✔ SmartCart Backoffice API  
❌ Not used by normal customer

====================  WHERE USED IN SMARTCART?  =====================
✔ Product Edit → Variant Image Manager  
✔ SKU Media Control  
✔ Image Sorting / Selection modules  

====================  INPUT PARAMETERS  =====================
@FK_ProductVariant     → Required SKU ID  
@IncludeCancelled      → 0 = show only active (default)
                         1 = show deleted images also
@PageIndex             → Pagination page number  
@PageSize              → Rows per page  
@SortColumn            → Sorting field  
@SortMode              → ASC / DESC  

====================  OUTPUT  =====================
Returns:
  • ID_ProductVariantImage  
  • ImageURL  
  • IsDefault  
  • CreatedOn  
  • Cancelled  
  • CancelledOn  
  • CancelledReason  

Also returns:
  • TotalCount  
  • PageIndex  
  • PageSize  

**********************************************************************/

CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantImageSelect]
(
    @FK_ProductVariant INT,
    @IncludeCancelled BIT = 0,                 -- 0 = only active images
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

    IF NOT EXISTS (SELECT 1 FROM ProductVariant WHERE ID_ProductVariant = @FK_ProductVariant AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'ProductVariant does not exist or deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    -------------------------------------------------------------------
    -- TEMP TABLE
    -------------------------------------------------------------------
    CREATE TABLE #tmpImage
    (
        RowID BIGINT IDENTITY(1,1),
        ID_ProductVariantImage INT,
        FK_ProductVariant INT,
        ImageURL NVARCHAR(MAX),
        IsDefault BIT,
        CreatedOn DATETIME,
        Cancelled BIT,
        CancelledOn DATETIME,
        CancelledReason NVARCHAR(500)
    );

    -------------------------------------------------------------------
    -- INSERT IMAGES
    -------------------------------------------------------------------
    INSERT INTO #tmpImage
    (
        ID_ProductVariantImage,
        FK_ProductVariant,
        ImageURL,
        IsDefault,
        CreatedOn,
        Cancelled,
        CancelledOn,
        CancelledReason
    )
    SELECT
        i.ID_ProductVariantImage,
        i.FK_ProductVariant,
        i.ImageURL,
        i.IsDefault,
        i.CreatedOn,
        i.Cancelled,
        i.CancelledOn,
        i.CancelledReason
    FROM ProductVariantImage i WITH(NOLOCK)
    WHERE i.FK_ProductVariant = @FK_ProductVariant
      AND (@IncludeCancelled = 1 OR i.Cancelled = 0);

    -------------------------------------------------------------------
    -- TOTAL COUNT
    -------------------------------------------------------------------
    DECLARE @TotalCount BIGINT;
    SELECT @TotalCount = COUNT(*) FROM #tmpImage;

    -------------------------------------------------------------------
    -- SORTING
    -------------------------------------------------------------------
    DECLARE @Sql NVARCHAR(MAX);

    SET @Sql = '
        SELECT 
            ID_ProductVariantImage,
            FK_ProductVariant,
            ImageURL,
            IsDefault,
            CreatedOn,
            Cancelled,
            CancelledOn,
            CancelledReason
        FROM #tmpImage
        ORDER BY ' +
        CASE 
            WHEN @SortColumn = 'CreatedOn' THEN 'CreatedOn'
            WHEN @SortColumn = 'IsDefault' THEN 'IsDefault'
            ELSE 'IsDefault DESC, CreatedOn DESC'
        END + ' ' + @SortMode + '
        OFFSET ' + CAST((@PageIndex - 1) * @PageSize AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(@PageSize AS VARCHAR) + ' ROWS ONLY;
    ';

    EXEC(@Sql);

    -------------------------------------------------------------------
    -- META INFO
    -------------------------------------------------------------------
    SELECT @TotalCount AS TotalCount,
           @PageIndex AS PageIndex,
           @PageSize AS PageSize;

    DROP TABLE #tmpImage;
END;

