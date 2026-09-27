SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users](
        [UserId] [int] IDENTITY(1,1) NOT NULL,
        [UserName] [nvarchar](100) NOT NULL,
        [FullName] [nvarchar](150) NULL,
        [PasswordHash] [nvarchar](255) NOT NULL,
        [Email] [nvarchar](100) NOT NULL,
        [PhoneNumber] [nvarchar](15) NULL,
        [IsAdmin] [bit] NOT NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (getdate()),
        [UpdatedAt] [datetime] NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_Users_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([UserId] ASC)
    );
END
GO
