SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[ProductStatus]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductStatus](
        [StatusId] [int] IDENTITY(1,1) NOT NULL,
        [StatusName] [nvarchar](50) NOT NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_ProductStatus_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([StatusId] ASC)
    );
END
GO
