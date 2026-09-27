SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Wishlist]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Wishlist](
        [WishlistId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [SessionKey] [uniqueidentifier] NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_Wishlist_CreatedAt] DEFAULT (getdate()),
        [Cancelled] [bit] NULL CONSTRAINT [DF_Wishlist_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([WishlistId] ASC)
    );
END
GO
