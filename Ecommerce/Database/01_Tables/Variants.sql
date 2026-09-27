SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Variants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Variants](
        [ID_Variant] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(255) NOT NULL UNIQUE,
        [Description] NVARCHAR(500) NULL,
        [DisplayOrder] INT CONSTRAINT [DF_Variants_DisplayOrder] DEFAULT 0,
        [IsActive] BIT CONSTRAINT [DF_Variants_IsActive] DEFAULT 1,
        [Cancelled] BIT CONSTRAINT [DF_Variants_Cancelled] DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        CONSTRAINT [PK_Variants] PRIMARY KEY ([ID_Variant])
    );
END
GO
