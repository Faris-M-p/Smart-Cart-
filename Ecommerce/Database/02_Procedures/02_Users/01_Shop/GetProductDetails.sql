SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[GetProductDetails]
    @Slug NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProductId INT;

    IF TRY_CAST(@Slug AS INT) > 0
    BEGIN
        SELECT @ProductId = P.ID_Product
        FROM Products AS P WITH (NOLOCK)
        WHERE ISNULL(P.Cancelled, 0) = 0
          AND P.IsActive = 1
          AND (P.ID_Product = TRY_CAST(@Slug AS INT) OR P.Slug = @Slug);
    END
    ELSE
    BEGIN
        SELECT @ProductId = P.ID_Product
        FROM Products AS P WITH (NOLOCK)
        WHERE ISNULL(P.Cancelled, 0) = 0
          AND P.IsActive = 1
          AND P.Slug = @Slug;
    END;

    SELECT
        P.ID_Product AS ProductId,
        P.Name,
        P.Slug,
        ISNULL(P.Description, N'') AS Description,
        C.ID_Category AS CategoryId,
        C.Name AS CategoryName,
        P.FK_SubCategory AS SubCategoryId,
        SC.Name AS SubCategoryName,
        ISNULL(P.FK_Brand, 0) AS BrandId,
        ISNULL(B.BrandName, N'') AS BrandName
    FROM Products AS P WITH (NOLOCK)
    INNER JOIN SubCategory AS SC WITH (NOLOCK) ON SC.ID_SubCategory = P.FK_SubCategory
    INNER JOIN Category AS C WITH (NOLOCK) ON C.ID_Category = SC.FK_Category
    LEFT JOIN Brand AS B WITH (NOLOCK) ON B.ID_Brand = P.FK_Brand
    WHERE P.ID_Product = @ProductId;

    SELECT
        PM.MediaUrl
    FROM ProductMedia AS PM WITH (NOLOCK)
    WHERE PM.FK_Product = @ProductId
      AND PM.MediaType = N'Image'
      AND PM.MediaUrl IS NOT NULL
      AND PM.MediaUrl <> N''
    ORDER BY PM.IsPrimary DESC, PM.DisplayOrder ASC, PM.ID_ProductMedia ASC;

    SELECT
        PV.ID_ProductVariant AS ProductVariantId,
        PV.SKU,
        CASE WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN PV.SKU ELSE PV.VariantLabel END AS Label,
        PV.SellingPrice AS Price,
        PV.MRP,
        CAST(CASE WHEN ISNULL(ST.Qty, 0) > 0 THEN 1 ELSE 0 END AS BIT) AS InStock,
        PV.IsDefault
    FROM ProductVariants AS PV WITH (NOLOCK)
    LEFT JOIN (
        SELECT FK_ProductVariant, SUM(Quantity) AS Qty
        FROM Stock WITH (NOLOCK)
        WHERE ISNULL(Cancelled, 0) = 0
        GROUP BY FK_ProductVariant
    ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
    WHERE PV.FK_Product = @ProductId
      AND ISNULL(PV.Cancelled, 0) = 0
      AND PV.IsActive = 1
    ORDER BY PV.IsDefault DESC, PV.SellingPrice ASC, PV.ID_ProductVariant ASC;

    SELECT
        PVA.FK_ProductVariant AS ProductVariantId,
        V.ID_Variant AS VariantId,
        V.Name AS VariantName,
        VV.ID_VariantValue AS VariantValueId,
        VV.Name AS VariantValueName
    FROM ProductVariantAttributes AS PVA WITH (NOLOCK)
    INNER JOIN ProductVariants AS PV WITH (NOLOCK) ON PV.ID_ProductVariant = PVA.FK_ProductVariant
    INNER JOIN Variants AS V WITH (NOLOCK) ON V.ID_Variant = PVA.FK_Variant
    INNER JOIN VariantValues AS VV WITH (NOLOCK) ON VV.ID_VariantValue = PVA.FK_VariantValue
    WHERE PV.FK_Product = @ProductId
      AND ISNULL(PV.Cancelled, 0) = 0
      AND PV.IsActive = 1
    ORDER BY V.DisplayOrder, V.Name, VV.DisplayOrder, VV.Name;

    SELECT
        SM.FK_ProductSKU AS ProductVariantId,
        SM.MediaUrl AS ImageUrl
    FROM SkuMedia AS SM WITH (NOLOCK)
    INNER JOIN ProductVariants AS PV WITH (NOLOCK) ON PV.ID_ProductVariant = SM.FK_ProductSKU
    WHERE PV.FK_Product = @ProductId
      AND ISNULL(PV.Cancelled, 0) = 0
      AND PV.IsActive = 1
      AND SM.MediaType = N'Image'
      AND SM.MediaUrl IS NOT NULL
      AND SM.MediaUrl <> N''
    ORDER BY SM.IsPrimary DESC, SM.DisplayOrder ASC;
END;
GO
