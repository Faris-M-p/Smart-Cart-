USE [SmartCart]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Supplier](
    [ID_Supplier] INT IDENTITY(1,1) NOT NULL,

    [Name] NVARCHAR(250) NOT NULL,
    [CompanyName] NVARCHAR(250) NULL,

    [Email] NVARCHAR(250) NULL,
    [Phone] NVARCHAR(20) NULL,

    -- Location (Dropdown आधारित)
    [State] NVARCHAR(150) NOT NULL,
    [District] NVARCHAR(150) NOT NULL,
    [City] NVARCHAR(150) NOT NULL,
    [Address] NVARCHAR(500) NULL,
    [Pincode] NVARCHAR(10) NULL,

    [Description] NVARCHAR(1000) NULL,

    [IsActive] BIT NOT NULL DEFAULT 1,

    -- Audit
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NULL,

    -- Soft delete
    [Cancelled] BIT NOT NULL DEFAULT 0,
    [CancelledOn] DATETIME NULL,
    [CancelledReason] NVARCHAR(255) NULL,

    PRIMARY KEY CLUSTERED ([ID_Supplier] ASC)
);

CREATE INDEX IX_Supplier_Name ON Supplier(Name);

ALTER TABLE Supplier
ADD CONSTRAINT UQ_Supplier_Email UNIQUE (Email);