IF OBJECT_ID(N'[dbo].[SkuMedia]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SkuMedia]
    (
        [ID_SkuMedia] INT IDENTITY(1,1) NOT NULL,
        [FK_ProductSKU] INT NOT NULL,
        [MediaType] NVARCHAR(20) NOT NULL CONSTRAINT [DF_SkuMedia_MediaType] DEFAULT (N'Image'),
        [MediaUrl] NVARCHAR(500) NOT NULL,
        [DisplayOrder] INT NOT NULL CONSTRAINT [DF_SkuMedia_DisplayOrder] DEFAULT ((0)),
        [IsPrimary] BIT NOT NULL CONSTRAINT [DF_SkuMedia_IsPrimary] DEFAULT ((0)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_SkuMedia_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [PK_SkuMedia] PRIMARY KEY CLUSTERED ([ID_SkuMedia] ASC),
        CONSTRAINT [FK_SkuMedia_ProductVariant] FOREIGN KEY ([FK_ProductSKU])
            REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant]),
        CONSTRAINT [CK_SkuMedia_MediaType] CHECK ([MediaType] = N'Image')
    );

    CREATE NONCLUSTERED INDEX [IX_SkuMedia_FK_ProductSKU]
        ON [dbo].[SkuMedia] ([FK_ProductSKU] ASC, [DisplayOrder] ASC);
END
GO

IF OBJECT_ID(N'[dbo].[SkuMedia]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[ProductVariantImages]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[SkuMedia])
   AND EXISTS (SELECT 1 FROM [dbo].[ProductVariantImages])
BEGIN
    SET IDENTITY_INSERT [dbo].[SkuMedia] ON;

    INSERT INTO [dbo].[SkuMedia]
    (
        [ID_SkuMedia],
        [FK_ProductSKU],
        [MediaType],
        [MediaUrl],
        [DisplayOrder],
        [IsPrimary],
        [CreatedAt],
        [UpdatedAt]
    )
    SELECT
        pvi.[ID_ProductVariantImage],
        pvi.[FK_ProductVariant],
        N'Image',
        pvi.[ImageUrl],
        pvi.[DisplayOrder],
        pvi.[IsPrimary],
        ISNULL(pvi.[CreatedAt], GETDATE()),
        NULL
    FROM [dbo].[ProductVariantImages] pvi
    WHERE EXISTS (
        SELECT 1
        FROM [dbo].[ProductVariants] pv
        WHERE pv.[ID_ProductVariant] = pvi.[FK_ProductVariant]
    );

    SET IDENTITY_INSERT [dbo].[SkuMedia] OFF;

    DECLARE @maxId INT = ISNULL((SELECT MAX([ID_SkuMedia]) FROM [dbo].[SkuMedia]), 0);
    DBCC CHECKIDENT (N'dbo.SkuMedia', RESEED, @maxId);
END
GO
