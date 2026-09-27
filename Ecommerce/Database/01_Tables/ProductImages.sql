SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[ProductImages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductImages](
        [ProductImageId] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [ImageUrl] [nvarchar](max) NOT NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_ProductImages_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([ProductImageId] ASC),
        CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ID_Product])
    );
END
GO
