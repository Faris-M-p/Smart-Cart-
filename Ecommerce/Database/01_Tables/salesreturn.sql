SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[salesreturn]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[salesreturn](
        [id_salesreturn] INT IDENTITY(1,1) NOT NULL,
        [fk_sale] INT NOT NULL,
        [invoicenumber] NVARCHAR(100) NULL,
        [returndate] DATE NOT NULL CONSTRAINT [DF_salesreturn_returndate] DEFAULT (CONVERT(DATE, GETDATE())),
        [totalamount] DECIMAL(12,2) NOT NULL CONSTRAINT [DF_salesreturn_totalamount] DEFAULT ((0)),
        [reason] NVARCHAR(500) NULL,
        [notes] NVARCHAR(500) NULL,
        [createdon] DATETIME NOT NULL CONSTRAINT [DF_salesreturn_createdon] DEFAULT (GETDATE()),
        [enterby] INT NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_salesreturn_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        [cancelledby] INT NULL,
        CONSTRAINT [PK_salesreturn] PRIMARY KEY CLUSTERED ([id_salesreturn] ASC),
        CONSTRAINT [FK_salesreturn_sales] FOREIGN KEY ([fk_sale]) REFERENCES [dbo].[sales] ([id_sale])
    );
END
GO
