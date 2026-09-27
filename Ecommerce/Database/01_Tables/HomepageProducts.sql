SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[homepage_products]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[homepage_products](
        [id_homepageproduct] INT IDENTITY(1,1) NOT NULL,
        [fk_product] INT NOT NULL,
        [isactive] BIT NOT NULL CONSTRAINT [DF_homepage_products_isactive] DEFAULT ((1)),
        [displayorder] INT NOT NULL CONSTRAINT [DF_homepage_products_displayorder] DEFAULT ((0)),
        [createdat] DATETIME NOT NULL CONSTRAINT [DF_homepage_products_createdat] DEFAULT (GETDATE()),
        [updatedat] DATETIME NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_homepage_products_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        CONSTRAINT [PK_homepage_products] PRIMARY KEY CLUSTERED ([id_homepageproduct] ASC),
        CONSTRAINT [UQ_homepage_products_product] UNIQUE ([fk_product]),
        CONSTRAINT [FK_hp_product] FOREIGN KEY ([fk_product]) REFERENCES [dbo].[Products] ([ID_Product])
    );
END
GO
