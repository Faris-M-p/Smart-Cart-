SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Ratings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Ratings](
        [RatingId] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [UserId] [int] NOT NULL,
        [RatingValue] [decimal](2, 1) NOT NULL,
        [Review] [nvarchar](max) NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_Ratings_CreatedAt] DEFAULT (getdate()),
        [Cancelled] [bit] NULL CONSTRAINT [DF_Ratings_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([RatingId] ASC),
        CONSTRAINT [FK_Ratings_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ID_Product]),
        CONSTRAINT [FK_Ratings_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
    );
END
GO
