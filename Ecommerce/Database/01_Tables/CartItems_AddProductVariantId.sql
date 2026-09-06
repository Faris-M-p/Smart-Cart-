IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CartItems', N'ProductVariantId') IS NULL
BEGIN
    ALTER TABLE [dbo].[CartItems]
        ADD [ProductVariantId] INT NULL;
END
GO

IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CartItems', N'ProductVariantId') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_CartItems_ProductVariant'
          AND parent_object_id = OBJECT_ID(N'dbo.CartItems')
   )
BEGIN
    ALTER TABLE [dbo].[CartItems] WITH CHECK
        ADD CONSTRAINT [FK_CartItems_ProductVariant]
        FOREIGN KEY ([ProductVariantId])
        REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant]);
END
GO
