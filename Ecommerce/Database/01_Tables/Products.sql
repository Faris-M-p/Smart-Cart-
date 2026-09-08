-- Target schema for admin Product module (EF: ProductEntity).

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Products](
        [ID_Product] INT IDENTITY(1,1) NOT NULL,
        [FK_SubCategory] INT NOT NULL,
        [FK_Brand] INT NULL,
        [Name] NVARCHAR(255) NOT NULL,
        [Slug] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),
        [SellOnline] BIT NOT NULL CONSTRAINT [DF_Products_SellOnline] DEFAULT ((0)),
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (GETDATE()),
        [ModifiedAt] DATETIME NULL,
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_Products_Cancelled] DEFAULT ((0)),
        [CancelledOn] DATETIME NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ID_Product] ASC),
        CONSTRAINT [UQ_Products_Slug] UNIQUE ([Slug]),
        CONSTRAINT [FK_Products_SubCategory]
            FOREIGN KEY ([FK_SubCategory]) REFERENCES [dbo].[SubCategory]([ID_SubCategory]),
        CONSTRAINT [FK_Products_Brand]
            FOREIGN KEY ([FK_Brand]) REFERENCES [dbo].[Brand]([ID_Brand])
    );
END
GO

IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Products', N'SellOnline') IS NULL
BEGIN
    ALTER TABLE [dbo].[Products]
        ADD [SellOnline] BIT NOT NULL CONSTRAINT [DF_Products_SellOnline] DEFAULT ((0));
END
GO
