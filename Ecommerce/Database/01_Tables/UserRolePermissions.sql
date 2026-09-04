SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[UserRolePermissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserRolePermissions](
        [ID_UserRolePermission] INT IDENTITY(1,1) NOT NULL,
        [FK_UserRole] INT NOT NULL,
        [FK_Permission] INT NOT NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_UserRolePermissions_CreatedAt] DEFAULT (GETDATE()),
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_UserRolePermissions_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        CONSTRAINT [PK_UserRolePermissions] PRIMARY KEY CLUSTERED ([ID_UserRolePermission] ASC),
        CONSTRAINT [UQ_UserRolePermissions_Role_Permission] UNIQUE ([FK_UserRole], [FK_Permission]),
        CONSTRAINT [FK_UserRolePermissions_UserRole] FOREIGN KEY ([FK_UserRole])
            REFERENCES [dbo].[UserRoles] ([ID_UserRole]),
        CONSTRAINT [FK_UserRolePermissions_Permission] FOREIGN KEY ([FK_Permission])
            REFERENCES [dbo].[Permissions] ([ID_Permission])
    );
END
GO
