USE [SmartCart]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Brand](
    [ID_Brand] INT IDENTITY(1,1) NOT NULL,

    [BrandName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(1000) NULL,

    [IsActive] BIT NOT NULL DEFAULT 1,

    -- Soft delete
    [Cancelled] BIT NOT NULL DEFAULT 0,
    [CancelledOn] DATETIME NULL,
    [CancelledReason] NVARCHAR(255) NULL,

    PRIMARY KEY CLUSTERED 
    (
        [ID_Brand] ASC
    )
) ON [PRIMARY]
GO