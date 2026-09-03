CREATE TABLE [dbo].[Stock](
    [ID_Stock] INT IDENTITY(1,1) NOT NULL,
    [FK_PurchaseDetail] INT NOT NULL,
    [FK_ProductVariant] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Stock_CreatedOn] DEFAULT (GETDATE()),
    [EnterBy] INT NULL,
    [Cancelled] BIT NOT NULL CONSTRAINT [DF_Stock_Cancelled] DEFAULT ((0)),
    [CancelledOn] DATETIME NULL,
    [CancelledReason] NVARCHAR(255) NULL,
    [CancelledBy] INT NULL,
    CONSTRAINT [PK_Stock] PRIMARY KEY CLUSTERED ([ID_Stock] ASC),
    CONSTRAINT [FK_Stock_PurchaseDetail] FOREIGN KEY ([FK_PurchaseDetail]) REFERENCES [dbo].[PurchaseDetail]([ID_PurchaseDetail]),
    CONSTRAINT [FK_Stock_ProductVariant] FOREIGN KEY ([FK_ProductVariant]) REFERENCES [dbo].[ProductVariants]([ID_ProductVariant])
);
GO
