CREATE TABLE [dbo].[ProductVariantAttributes](
    [ID_ProductVariantAttribute] INT IDENTITY(1,1) NOT NULL, -- Primary key (required for CRUD)

    [FK_ProductVariant] INT NOT NULL, -- Reference to Product Variant (SKU)
    [FK_Variant] INT NOT NULL, -- Reference to Variant axis (Size, Color)
    [FK_VariantValue] INT NOT NULL, -- Reference to selected value (e.g. Red, XL)

    [Description] NVARCHAR(500) NULL,

    CONSTRAINT PK_ProductVariantAttributes 
        PRIMARY KEY ([ID_ProductVariantAttribute]),

    CONSTRAINT FK_PVA_ProductVariant
        FOREIGN KEY ([FK_ProductVariant]) 
        REFERENCES [ProductVariants]([ID_ProductVariant]),

    CONSTRAINT FK_PVA_Variant
        FOREIGN KEY ([FK_Variant]) 
        REFERENCES [Variants]([ID_Variant]),

    CONSTRAINT FK_PVA_VariantValue
        FOREIGN KEY ([FK_VariantValue]) 
        REFERENCES [VariantValues]([ID_VariantValue]),

    -- 🔥 Prevent duplicate combination (IMPORTANT)
    CONSTRAINT UQ_ProductVariantAttributes 
        UNIQUE ([FK_ProductVariant], [FK_Variant])
);
GO

