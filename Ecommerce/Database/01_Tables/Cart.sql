SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Cart]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Cart](
        [CartId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [SessionKey] [uniqueidentifier] NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_Cart_CreatedAt] DEFAULT (getdate()),
        PRIMARY KEY CLUSTERED ([CartId] ASC)
    );
END
GO
