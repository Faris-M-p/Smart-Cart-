CREATE TABLE [dbo].[ProductVariants](
    [ID_ProductVariant] INT IDENTITY(1,1) NOT NULL,

    [FK_Product] INT NOT NULL,

    [SKU] NVARCHAR(100) NOT NULL UNIQUE,
    [Barcode] NVARCHAR(100) NULL UNIQUE,
    [VariantLabel] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000) NULL, -- 🔥 added

    [MRP] DECIMAL(10,2) NOT NULL,
    [SellingPrice] DECIMAL(10,2) NOT NULL,

    [UnitOfMeasure] NVARCHAR(20) NULL,
    [UnitValue] DECIMAL(10,3) NULL,

    [IsDefault] BIT DEFAULT 0,
    [MaxOrderQty] INT DEFAULT 10,

    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),

    [Cancelled] BIT DEFAULT 0,
    [CancelledOn] DATETIME NULL,

    CONSTRAINT PK_ProductVariants PRIMARY KEY ([ID_ProductVariant]),

    CONSTRAINT FK_ProductVariants_Product
        FOREIGN KEY ([FK_Product]) REFERENCES [Products]([ID_Product])
);