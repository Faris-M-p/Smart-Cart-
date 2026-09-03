CREATE TABLE [dbo].[PurchaseDetail](
    [ID_PurchaseDetail] INT IDENTITY(1,1) NOT NULL,
    [FK_Purchase] INT NOT NULL,
    [FK_ProductVariant] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [PurchasePrice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_PurchaseDetail_PurchasePrice] DEFAULT ((0)),
    [MRP] DECIMAL(18,2) NULL,
    [ExpiryDate] DATE NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_PurchaseDetail_CreatedOn] DEFAULT (GETDATE()),
    [EnterBy] INT NULL,
    [Cancelled] BIT NOT NULL CONSTRAINT [DF_PurchaseDetail_Cancelled] DEFAULT ((0)),
    [CancelledOn] DATETIME NULL,
    [CancelledReason] NVARCHAR(500) NULL,
    [CancelledBy] INT NULL,
    CONSTRAINT [PK_PurchaseDetail] PRIMARY KEY CLUSTERED ([ID_PurchaseDetail] ASC),
    CONSTRAINT [FK_PurchaseDetail_Purchase] FOREIGN KEY ([FK_Purchase]) REFERENCES [dbo].[Purchase]([ID_Purchase]),
    CONSTRAINT [FK_PurchaseDetail_ProductVariant] FOREIGN KEY ([FK_ProductVariant]) REFERENCES [dbo].[ProductVariants]([ID_ProductVariant])
);
GO
