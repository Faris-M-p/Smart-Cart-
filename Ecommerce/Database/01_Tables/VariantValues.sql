SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[VariantValues]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[VariantValues](
        [ID_VariantValue] INT IDENTITY(1,1) NOT NULL,
        [FK_Variant] INT NOT NULL,
        [Name] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [DisplayOrder] INT CONSTRAINT [DF_VariantValues_DisplayOrder] DEFAULT 0,
        [Cancelled] BIT CONSTRAINT [DF_VariantValues_Cancelled] DEFAULT 0,
        [CancelledOn] DATETIME NULL,
        CONSTRAINT [PK_VariantValues] PRIMARY KEY ([ID_VariantValue]),
        CONSTRAINT [FK_VariantValues_Variant] FOREIGN KEY ([FK_Variant]) REFERENCES [dbo].[Variants]([ID_Variant]),
        CONSTRAINT [UQ_Variant_Value] UNIQUE ([FK_Variant], [Name])
    );
END
GO
