CREATE TABLE [dbo].[Variants](
    [ID_Variant] INT IDENTITY(1,1) NOT NULL,

    [Name] NVARCHAR(255) NOT NULL UNIQUE,
    [Description] NVARCHAR(500) NULL, -- 🔥 added

    [DisplayOrder] INT DEFAULT 0,

    [IsActive] BIT DEFAULT 1,

    [Cancelled] BIT DEFAULT 0,
    [CancelledOn] DATETIME NULL,

    CONSTRAINT PK_Variants PRIMARY KEY ([ID_Variant])
);