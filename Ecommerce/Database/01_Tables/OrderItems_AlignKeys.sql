IF OBJECT_ID(N'[dbo].[OrderItems]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.OrderItems', N'OrderItemId') IS NOT NULL
   AND COL_LENGTH(N'dbo.OrderItems', N'ID_OrderItem') IS NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[FK_OrderItems_Orders]', N'F') IS NOT NULL
        ALTER TABLE [dbo].[OrderItems] DROP CONSTRAINT [FK_OrderItems_Orders];

    IF OBJECT_ID(N'[dbo].[FK_OrderItems_Products]', N'F') IS NOT NULL
        ALTER TABLE [dbo].[OrderItems] DROP CONSTRAINT [FK_OrderItems_Products];

    IF OBJECT_ID(N'[dbo].[PK_OrderItems]', N'PK') IS NOT NULL
        ALTER TABLE [dbo].[OrderItems] DROP CONSTRAINT [PK_OrderItems];

    EXEC sp_rename N'dbo.OrderItems.OrderItemId', N'ID_OrderItem', N'COLUMN';
    EXEC sp_rename N'dbo.OrderItems.OrderId', N'FK_Order', N'COLUMN';
    EXEC sp_rename N'dbo.OrderItems.ProductId', N'FK_Product', N'COLUMN';
    EXEC sp_rename N'dbo.OrderItems.ProductVariantId', N'FK_ProductVariant', N'COLUMN';

    ALTER TABLE [dbo].[OrderItems]
        ADD CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([ID_OrderItem] ASC);

    ALTER TABLE [dbo].[OrderItems] WITH CHECK ADD CONSTRAINT [FK_OrderItems_Order]
        FOREIGN KEY ([FK_Order]) REFERENCES [dbo].[Orders] ([OrderId]);

    ALTER TABLE [dbo].[OrderItems] WITH CHECK ADD CONSTRAINT [FK_OrderItems_Product]
        FOREIGN KEY ([FK_Product]) REFERENCES [dbo].[Products] ([ID_Product]);

    IF COL_LENGTH(N'dbo.OrderItems', N'FK_ProductVariant') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[FK_OrderItems_ProductVariant]', N'F') IS NULL
    BEGIN
        ALTER TABLE [dbo].[OrderItems] WITH CHECK ADD CONSTRAINT [FK_OrderItems_ProductVariant]
            FOREIGN KEY ([FK_ProductVariant]) REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant]);
    END
END
GO
