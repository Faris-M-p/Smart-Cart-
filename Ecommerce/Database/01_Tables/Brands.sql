SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Brand]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Brand](
        [ID_Brand] INT IDENTITY(1,1) NOT NULL,
        [BrandName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ImageUrl] NVARCHAR(500) NULL,
        [Cancelled] BIT NOT NULL DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        PRIMARY KEY CLUSTERED
        (
            [ID_Brand] ASC
        )
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID(N'[dbo].[Brand]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Brand', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[Brand]
        ADD [ImageUrl] NVARCHAR(500) NULL;
END
GO
