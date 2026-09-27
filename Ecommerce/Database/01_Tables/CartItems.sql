SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems](
        [CartItemId] [int] IDENTITY(1,1) NOT NULL,
        [CartId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [ProductVariantId] [int] NULL,
        [Quantity] [int] NOT NULL,
        [Price] [decimal](10, 2) NOT NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (getdate()),
        PRIMARY KEY CLUSTERED ([CartItemId] ASC),
        CONSTRAINT [FK_CartItems_Cart] FOREIGN KEY ([CartId]) REFERENCES [dbo].[Cart] ([CartId]),
        CONSTRAINT [FK_CartItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ID_Product])
    );
END
GO
