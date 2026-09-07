SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[OrderItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItems](
        [ID_OrderItem] INT IDENTITY(1,1) NOT NULL,
        [FK_Order] INT NOT NULL,
        [FK_Product] INT NOT NULL,
        [FK_ProductVariant] INT NULL,
        [ProductName] NVARCHAR(200) NOT NULL,
        [VariantLabel] NVARCHAR(200) NULL,
        [SKU] NVARCHAR(100) NULL,
        [UnitPrice] DECIMAL(10, 2) NOT NULL,
        [Quantity] INT NOT NULL,
        [LineTotal] DECIMAL(10, 2) NOT NULL,
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_OrderItems_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([ID_OrderItem] ASC),
        CONSTRAINT [FK_OrderItems_Order] FOREIGN KEY ([FK_Order]) REFERENCES [dbo].[Orders] ([OrderId]),
        CONSTRAINT [FK_OrderItems_Product] FOREIGN KEY ([FK_Product]) REFERENCES [dbo].[Products] ([ID_Product]),
        CONSTRAINT [FK_OrderItems_ProductVariant] FOREIGN KEY ([FK_ProductVariant]) REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant])
    );
END
GO
