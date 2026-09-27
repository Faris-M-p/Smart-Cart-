SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[salesreturndetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[salesreturndetail](
        [id_salesreturndetail] INT IDENTITY(1,1) NOT NULL,
        [fk_salesreturn] INT NOT NULL,
        [fk_salesdetail] INT NOT NULL,
        [fk_productvariant] INT NOT NULL,
        [quantity] INT NOT NULL,
        [sellingprice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_salesreturndetail_sellingprice] DEFAULT ((0)),
        [createdon] DATETIME NOT NULL CONSTRAINT [DF_salesreturndetail_createdon] DEFAULT (GETDATE()),
        [enterby] INT NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_salesreturndetail_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        [cancelledby] INT NULL,
        CONSTRAINT [PK_salesreturndetail] PRIMARY KEY CLUSTERED ([id_salesreturndetail] ASC),
        CONSTRAINT [FK_salesreturndetail_salesreturn] FOREIGN KEY ([fk_salesreturn]) REFERENCES [dbo].[salesreturn] ([id_salesreturn]),
        CONSTRAINT [FK_salesreturndetail_salesdetail] FOREIGN KEY ([fk_salesdetail]) REFERENCES [dbo].[salesdetail] ([id_salesdetail]),
        CONSTRAINT [FK_salesreturndetail_productvariant] FOREIGN KEY ([fk_productvariant]) REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant])
    );
END
GO
