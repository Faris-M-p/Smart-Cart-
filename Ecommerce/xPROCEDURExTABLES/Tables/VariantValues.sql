CREATE TABLE [dbo].[VariantValues](
    [ID_VariantValue] INT IDENTITY(1,1) NOT NULL,

    [FK_Variant] INT NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(500) NULL, -- 🔥 added

    [DisplayOrder] INT DEFAULT 0,

    [Cancelled] BIT DEFAULT 0,
    [CancelledOn] DATETIME NULL,

    CONSTRAINT PK_VariantValues PRIMARY KEY ([ID_VariantValue]),

    CONSTRAINT FK_VariantValues_Variant
        FOREIGN KEY ([FK_Variant]) REFERENCES [Variants]([ID_Variant]),

    CONSTRAINT UQ_Variant_Value UNIQUE (FK_Variant, Name)
);