SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[AdminUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AdminUsers](
        [ID_AdminUser] INT IDENTITY(1,1) NOT NULL,
        [FK_UserRole] INT NOT NULL,
        [UserName] NVARCHAR(50) NOT NULL,
        [PasswordHash] NVARCHAR(255) NOT NULL,
        [FullName] NVARCHAR(150) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PhoneNumber] NVARCHAR(15) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_AdminUsers_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_AdminUsers_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_AdminUsers_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,
        CONSTRAINT [PK_AdminUsers] PRIMARY KEY CLUSTERED ([ID_AdminUser] ASC),
        CONSTRAINT [UQ_AdminUsers_UserName] UNIQUE ([UserName]),
        CONSTRAINT [FK_AdminUsers_UserRole] FOREIGN KEY ([FK_UserRole])
            REFERENCES [dbo].[UserRoles] ([ID_UserRole])
    );
END
GO
