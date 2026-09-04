SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Modules]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Modules](
        [ID_Module] INT IDENTITY(1,1) NOT NULL,
        [ModuleName] NVARCHAR(100) NOT NULL,
        [DisplayName] NVARCHAR(150) NOT NULL,
        [DisplayOrder] INT NOT NULL CONSTRAINT [DF_Modules_DisplayOrder] DEFAULT ((0)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Modules_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Modules_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_Modules_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        CONSTRAINT [PK_Modules] PRIMARY KEY CLUSTERED ([ID_Module] ASC),
        CONSTRAINT [UQ_Modules_ModuleName] UNIQUE ([ModuleName])
    );
END
GO
