SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Permissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permissions](
        [ID_Permission] INT IDENTITY(1,1) NOT NULL,
        [FK_Module] INT NOT NULL,
        [PermissionName] NVARCHAR(100) NOT NULL,
        [PermissionCode] NVARCHAR(150) NOT NULL,
        [DisplayOrder] INT NOT NULL CONSTRAINT [DF_Permissions_DisplayOrder] DEFAULT ((0)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Permissions_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Permissions_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_Permissions_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([ID_Permission] ASC),
        CONSTRAINT [UQ_Permissions_PermissionCode] UNIQUE ([PermissionCode]),
        CONSTRAINT [FK_Permissions_Module] FOREIGN KEY ([FK_Module])
            REFERENCES [dbo].[Modules] ([ID_Module])
    );
END
GO
