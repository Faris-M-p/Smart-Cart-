SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[UserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [ID_UserRole] INT IDENTITY(1,1) NOT NULL,
        [RoleName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [IsSystemRole] BIT NOT NULL CONSTRAINT [DF_UserRoles_IsSystemRole] DEFAULT ((0)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_UserRoles_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_UserRoles_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_UserRoles_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([ID_UserRole] ASC),
        CONSTRAINT [UQ_UserRoles_RoleName] UNIQUE ([RoleName])
    );
END
GO
