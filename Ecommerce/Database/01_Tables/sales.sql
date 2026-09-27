SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[sales]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[sales](
        [id_sale] INT IDENTITY(1,1) NOT NULL,
        [invoicenumber] NVARCHAR(100) NULL,
        [saledate] DATE NOT NULL CONSTRAINT [DF_sales_saledate] DEFAULT (CONVERT(DATE, GETDATE())),
        [customername] NVARCHAR(200) NULL,
        [customerphone] NVARCHAR(30) NULL,
        [paymentmethod] NVARCHAR(30) NOT NULL CONSTRAINT [DF_sales_paymentmethod] DEFAULT ('Cash'),
        [totalamount] DECIMAL(12,2) NOT NULL CONSTRAINT [DF_sales_totalamount] DEFAULT ((0)),
        [notes] NVARCHAR(500) NULL,
        [createdon] DATETIME NOT NULL CONSTRAINT [DF_sales_createdon] DEFAULT (GETDATE()),
        [enterby] INT NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_sales_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        [cancelledby] INT NULL,
        CONSTRAINT [PK_sales] PRIMARY KEY CLUSTERED ([id_sale] ASC)
    );
END
GO
