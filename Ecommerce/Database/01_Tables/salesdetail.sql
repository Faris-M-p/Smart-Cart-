SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[salesdetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[salesdetail](
        [id_salesdetail] INT IDENTITY(1,1) NOT NULL,
        [fk_sale] INT NOT NULL,
        [fk_productvariant] INT NOT NULL,
        [quantity] INT NOT NULL,
        [sellingprice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_salesdetail_sellingprice] DEFAULT ((0)),
        [mrp] DECIMAL(18,2) NULL,
        [createdon] DATETIME NOT NULL CONSTRAINT [DF_salesdetail_createdon] DEFAULT (GETDATE()),
        [enterby] INT NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_salesdetail_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        [cancelledby] INT NULL,
        CONSTRAINT [PK_salesdetail] PRIMARY KEY CLUSTERED ([id_salesdetail] ASC),
        CONSTRAINT [FK_salesdetail_sales] FOREIGN KEY ([fk_sale]) REFERENCES [dbo].[sales] ([id_sale]),
        CONSTRAINT [FK_salesdetail_productvariant] FOREIGN KEY ([fk_productvariant]) REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant])
    );
END
GO
