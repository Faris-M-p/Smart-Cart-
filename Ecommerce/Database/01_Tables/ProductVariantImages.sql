IF OBJECT_ID('dbo.ProductVariantImages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariantImages
    (
        ID_ProductVariantImage INT IDENTITY(1,1) PRIMARY KEY,
        FK_ProductVariant INT NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        IsPrimary BIT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_ProductVariantImages_ProductVariant
            FOREIGN KEY(FK_ProductVariant)
            REFERENCES dbo.ProductVariants(ID_ProductVariant)
    );

    CREATE INDEX IX_ProductVariantImages_FK_ProductVariant
        ON dbo.ProductVariantImages(FK_ProductVariant);
END
GO

