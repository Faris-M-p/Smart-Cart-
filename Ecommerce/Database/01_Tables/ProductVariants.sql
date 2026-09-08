SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[ProductVariants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductVariants](
        [ID_ProductVariant] INT IDENTITY(1,1) NOT NULL,
        [FK_Product] INT NOT NULL,
        [SKU] NVARCHAR(100) NOT NULL UNIQUE,
        [Barcode] NVARCHAR(100) NULL UNIQUE,
        [VariantLabel] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [MRP] DECIMAL(10,2) NOT NULL,
        [SellingPrice] DECIMAL(10,2) NOT NULL,
        [UnitOfMeasure] NVARCHAR(20) NULL,
        [UnitValue] DECIMAL(10,3) NULL,
        [IsDefault] BIT DEFAULT 0,
        [MaxOrderQty] INT DEFAULT 10,
        [IsActive] BIT DEFAULT 1,
        [SellOnline] BIT NOT NULL CONSTRAINT [DF_ProductVariants_SellOnline] DEFAULT ((0)),
        [CreatedAt] DATETIME DEFAULT GETDATE(),
        [Cancelled] BIT DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([ID_ProductVariant]),
        CONSTRAINT [FK_ProductVariants_Product]
            FOREIGN KEY ([FK_Product]) REFERENCES [dbo].[Products]([ID_Product])
    );
END
GO

IF OBJECT_ID(N'[dbo].[ProductVariants]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.ProductVariants', N'SellOnline') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProductVariants]
        ADD [SellOnline] BIT NOT NULL CONSTRAINT [DF_ProductVariants_SellOnline] DEFAULT ((0));
END
GO
