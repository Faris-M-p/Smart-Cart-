SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Supplier]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Supplier](
        [ID_Supplier] INT IDENTITY(1,1) NOT NULL,

        [Name] NVARCHAR(250) NOT NULL,
        [CompanyName] NVARCHAR(250) NULL,

        [Email] NVARCHAR(250) NULL,
        [Phone] NVARCHAR(20) NULL,

        [State] NVARCHAR(150) NOT NULL,
        [District] NVARCHAR(150) NOT NULL,
        [City] NVARCHAR(150) NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [Pincode] NVARCHAR(10) NULL,

        [Description] NVARCHAR(1000) NULL,

        [IsActive] BIT NOT NULL CONSTRAINT [DF_Supplier_IsActive] DEFAULT 1,

        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Supplier_CreatedAt] DEFAULT GETDATE(),
        [UpdatedAt] DATETIME NULL,

        [Cancelled] BIT NOT NULL CONSTRAINT [DF_Supplier_Cancelled] DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        [CancelledReason] NVARCHAR(255) NULL,

        CONSTRAINT [PK_Supplier] PRIMARY KEY CLUSTERED ([ID_Supplier] ASC),
        CONSTRAINT [UQ_Supplier_Email] UNIQUE ([Email])
    );

    CREATE INDEX [IX_Supplier_Name] ON [dbo].[Supplier]([Name]);
END
GO
