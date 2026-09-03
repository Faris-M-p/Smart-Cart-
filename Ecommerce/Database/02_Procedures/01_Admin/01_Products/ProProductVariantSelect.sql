/****** Object:  StoredProcedure [dbo].[ProProductVariantSelect]    Script Date: 13-01-2026 22:04:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantSelect
Created By       : Muhammed Faris
Created On       : 10/12/2025

====================  PROCEDURE PURPOSE  =====================
This procedure returns the complete SKU (ProductVariant) list 
for a given product in the SmartCart system.

This includes:
  • SKU Details (PriceAdjustment, IsDefault, CreatedOn)
  • Variant + VariantValue combinations (e.g., Color = Black)
  • SKU-level Images (default + all images)
  • SKU-level Stock Summary
  • Filters + Search + Pagination support

====================  BUSINESS USE CASES  =====================
1) Admin opens a Product detail page → Variant tab loads SKUs  
2) Admin wants to view all variants with stock & images  
3) Admin wants to search SKU combinations (ex: “Black”, “XL”)  
4) Admin wants to filter SKUs by Variant, VariantValue  
5) Admin wants paginated results for a product with many SKUs  

====================  WHO USES THIS PROCEDURE?  =====================
✔ Admin Panel (Web)  
✔ SmartCart Backoffice API  
❌ NOT used by normal customers  

====================  WHERE USED?  =====================
✔ Product Edit → “Variants” tab  
✔ Product Stock Management  
✔ Product Variant Image Editor  
✔ Inventory / Purchase / Order Linking  
✔ SKU-based analytics  

====================  INPUT PARAMETERS  =====================
@FK_Product           → Product ID  
@SearchText           → Search inside SKU attributes ("Black", "XL", etc.)  
@FilterVariantIDs     → JSON: [ {"ID_Value":1}, {"ID_Value":2} ]  
@FilterVariantValueIDs→ JSON: [ {"ID_Value":21}, {"ID_Value":33} ]  
@PageIndex            → Page number  
@PageSize             → Rows per page  
@SortColumn           → Sorting column  
@SortMode             → ASC / DESC  

====================  OUTPUT STRUCTURE  =====================
1) List of SKUs  
2) Meta info: TotalCount, PageIndex, PageSize  
3) Each SKU includes:  
   - ID_ProductVariant  
   - PriceAdjustment  
   - IsDefault  
   - CreatedOn  
   - Variant Signature (e.g., "Color:Black | Print:Spiderman")  
   - StockAvailable  
   - Default ImageURL  
**********************************************************************/

CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantSelect]
(
    @FK_Product INT,
    @SearchText NVARCHAR(255) = '',
    @FilterVariantIDs NVARCHAR(MAX) = '',
    @FilterVariantValueIDs NVARCHAR(MAX) = '',
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
    IF (@FK_Product = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid FK_Product' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    -------------------------------------------------------------------
    -- TEMP TABLE TO STORE FINAL RESULTS
    -------------------------------------------------------------------
    CREATE TABLE #tmpSKU
    (
        RowID BIGINT IDENTITY(1,1),
        ID_ProductVariant INT,
        PriceAdjustment DECIMAL(10,2),
        IsDefault BIT,
        CreatedOn DATETIME,
        AttributeSignature NVARCHAR(MAX),
        StockAvailable INT,
        ImageURL NVARCHAR(500)
    );

    -------------------------------------------------------------------
    -- INSERT BASE SKU LIST
    -------------------------------------------------------------------
    INSERT INTO #tmpSKU (ID_ProductVariant, PriceAdjustment, IsDefault, CreatedOn)
    SELECT 
        pv.ID_ProductVariant,
        pv.PriceAdjustment,
        pv.IsDefault,
        pv.CreatedOn
    FROM ProductVariant pv WITH(NOLOCK)
    WHERE pv.FK_Product = @FK_Product
      AND pv.Cancelled = 0;

    -------------------------------------------------------------------
    -- BUILD SIGNATURE FOR EACH SKU
    -------------------------------------------------------------------
    UPDATE t
    SET t.AttributeSignature =
    (
        SELECT STRING_AGG(v.VariantName + ':' + vv.ValueName, ' | ')
               WITHIN GROUP (ORDER BY pva.FK_Variant)
        FROM ProductVariantAttribute pva
        INNER JOIN Variant v ON pva.FK_Variant = v.ID_Variant
        INNER JOIN VariantValue vv ON pva.FK_VariantValue = vv.ID_VariantValue
        WHERE pva.FK_ProductVariant = t.ID_ProductVariant
    )
    FROM #tmpSKU t;

    -------------------------------------------------------------------
    -- ADD IMAGE (DEFAULT SKU IMAGE)
    -------------------------------------------------------------------
    UPDATE t
    SET t.ImageURL =
    (
        SELECT TOP 1 ImageURL 
        FROM ProductVariantImage 
        WHERE FK_ProductVariant = t.ID_ProductVariant AND Cancelled = 0
        ORDER BY IsDefault DESC, ID_ProductVariantImage ASC
    )
    FROM #tmpSKU t;

    -------------------------------------------------------------------
    -- ADD STOCK SUMMARY
    -------------------------------------------------------------------
    UPDATE t
    SET t.StockAvailable =
    (
        SELECT ISNULL(SUM(s.Quantity), 0)
        FROM Stock s
        WHERE s.FK_VariantProduct = t.ID_ProductVariant AND s.Cancelled = 0
    )
    FROM #tmpSKU t;

    -------------------------------------------------------------------
    -- FILTER BY SEARCH TEXT
    -------------------------------------------------------------------
    IF (@SearchText <> '' AND LEN(@SearchText) >= 2)
    BEGIN
        DELETE FROM #tmpSKU
        WHERE AttributeSignature NOT LIKE '%' + @SearchText + '%';
    END

    -------------------------------------------------------------------
    -- FILTER BY VARIANT IDs (JSON)
    -------------------------------------------------------------------
    IF (@FilterVariantIDs <> '' AND @FilterVariantIDs <> '[]')
    BEGIN
        DELETE FROM #tmpSKU
        WHERE ID_ProductVariant NOT IN
        (
            SELECT DISTINCT pva.FK_ProductVariant
            FROM ProductVariantAttribute pva
            WHERE pva.FK_Variant IN (
                SELECT ID_Value FROM OPENJSON(@FilterVariantIDs)
                    WITH (ID_Value INT '$.ID_Value')
            )
        );
    END

    -------------------------------------------------------------------
    -- FILTER BY VARIANT VALUE IDs (JSON)
    -------------------------------------------------------------------
    IF (@FilterVariantValueIDs <> '' AND @FilterVariantValueIDs <> '[]')
    BEGIN
        DELETE FROM #tmpSKU
        WHERE ID_ProductVariant NOT IN
        (
            SELECT DISTINCT pva.FK_ProductVariant
            FROM ProductVariantAttribute pva
            WHERE pva.FK_VariantValue IN (
                SELECT ID_Value FROM OPENJSON(@FilterVariantValueIDs)
                    WITH (ID_Value INT '$.ID_Value')
            )
        );
    END

    -------------------------------------------------------------------
    -- APPLY SORTING
    -------------------------------------------------------------------
    DECLARE @Sql NVARCHAR(MAX) = '
        SELECT * FROM #tmpSKU
        ORDER BY ' + 
        CASE 
            WHEN @SortColumn = 'PriceAdjustment' THEN 'PriceAdjustment'
            WHEN @SortColumn = 'CreatedOn' THEN 'CreatedOn'
            WHEN @SortColumn = 'StockAvailable' THEN 'StockAvailable'
            ELSE 'ID_ProductVariant'
        END + ' ' + @SortMode + '
        OFFSET ' + CAST((@PageIndex-1)*@PageSize AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(@PageSize AS VARCHAR) + ' ROWS ONLY;
    ';

    EXEC(@Sql);

    -------------------------------------------------------------------
    -- META INFO
    -------------------------------------------------------------------
    SELECT 
        (SELECT COUNT(*) FROM #tmpSKU) AS TotalCount,
        @PageIndex AS PageIndex,
        @PageSize AS PageSize;

    DROP TABLE #tmpSKU;
END;

