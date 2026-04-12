-- Target schema for admin Product module (EF: ProductEntity).
-- Apply to your database when migrating from legacy Products.

CREATE TABLE [dbo].[Products](
    [ID_Product] INT IDENTITY(1,1) NOT NULL,
    [FK_SubCategory] INT NOT NULL,
    [FK_Brand] INT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Slug] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),
    [CreatedAt] DATETIME NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (GETDATE()),
    [ModifiedAt] DATETIME NULL,
    [Cancelled] BIT NOT NULL CONSTRAINT [DF_Products_Cancelled] DEFAULT ((0)),
    [CancelledOn] DATETIME NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ID_Product] ASC),
    CONSTRAINT [UQ_Products_Slug] UNIQUE ([Slug])
);
GO

ALTER TABLE [dbo].[Products] WITH CHECK ADD CONSTRAINT [FK_Products_SubCategory]
    FOREIGN KEY ([FK_SubCategory]) REFERENCES [dbo].[SubCategory]([ID_SubCategory]);
GO

ALTER TABLE [dbo].[Products] WITH CHECK ADD CONSTRAINT [FK_Products_Brand]
    FOREIGN KEY ([FK_Brand]) REFERENCES [dbo].[Brand]([ID_Brand]);
GO

-- ProductImages FK (column name may remain ProductId) references ID_Product
-- ALTER TABLE [dbo].[ProductImages] ADD CONSTRAINT [FK_ProductImages_Products]
--     FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([ID_Product]);
