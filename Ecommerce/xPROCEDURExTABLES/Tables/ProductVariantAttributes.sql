CREATE TABLE [dbo].[ProductVariantAttributes](
    [FK_ProductVariant] INT NOT NULL,
    [FK_Variant] INT NOT NULL,
    [FK_VariantValue] INT NOT NULL,
    [Description] NVARCHAR(500) NULL, -- 🔥 added

    CONSTRAINT PK_ProductVariantAttributes 
        PRIMARY KEY ([FK_ProductVariant], [FK_Variant]),

    CONSTRAINT FK_PVA_ProductVariant
        FOREIGN KEY ([FK_ProductVariant]) 
        REFERENCES [ProductVariants]([ID_ProductVariant]),

    CONSTRAINT FK_PVA_Variant
        FOREIGN KEY ([FK_Variant]) 
        REFERENCES [Variants]([ID_Variant]),

    CONSTRAINT FK_PVA_VariantValue
        FOREIGN KEY ([FK_VariantValue]) 
        REFERENCES [VariantValues]([ID_VariantValue])
);