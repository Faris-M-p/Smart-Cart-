SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[WishlistItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WishlistItems](
        [WishlistItemId] [int] IDENTITY(1,1) NOT NULL,
        [WishlistId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_WishlistItems_CreatedAt] DEFAULT (getdate()),
        [Cancelled] [bit] NULL CONSTRAINT [DF_WishlistItems_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([WishlistItemId] ASC),
        CONSTRAINT [FK_WishlistItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ID_Product]),
        CONSTRAINT [FK_WishlistItems_Wishlist] FOREIGN KEY ([WishlistId]) REFERENCES [dbo].[Wishlist] ([WishlistId])
    );
END
GO
