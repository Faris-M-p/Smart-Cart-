/****** Object:  Table [dbo].[Categories]    Script Date: 18-01-2025 23:00:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Category]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Category](
        [ID_Category] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ImageUrl] NVARCHAR(500) NULL,
        [Cancelled] BIT NOT NULL DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED
    (
        [ID_Category] ASC
    )
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID(N'[dbo].[Category]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Category', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[Category]
        ADD [ImageUrl] NVARCHAR(500) NULL;
END
GO
