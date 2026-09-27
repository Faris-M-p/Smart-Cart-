SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[ProductVariantAttributes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductVariantAttributes](
        [ID_ProductVariantAttribute] INT IDENTITY(1,1) NOT NULL,
        [FK_ProductVariant] INT NOT NULL,
        [FK_Variant] INT NOT NULL,
        [FK_VariantValue] INT NOT NULL,
        [Description] NVARCHAR(500) NULL,

        CONSTRAINT [PK_ProductVariantAttributes] PRIMARY KEY ([ID_ProductVariantAttribute]),
        CONSTRAINT [FK_PVA_ProductVariant] FOREIGN KEY ([FK_ProductVariant]) REFERENCES [dbo].[ProductVariants]([ID_ProductVariant]),
        CONSTRAINT [FK_PVA_Variant] FOREIGN KEY ([FK_Variant]) REFERENCES [dbo].[Variants]([ID_Variant]),
        CONSTRAINT [FK_PVA_VariantValue] FOREIGN KEY ([FK_VariantValue]) REFERENCES [dbo].[VariantValues]([ID_VariantValue]),
        CONSTRAINT [UQ_ProductVariantAttributes] UNIQUE ([FK_ProductVariant], [FK_Variant])
    );
END
GO
