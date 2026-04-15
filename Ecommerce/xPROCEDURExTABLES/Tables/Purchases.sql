CREATE TABLE [dbo].[Purchase](
    [ID_Purchase] INT IDENTITY(1,1) NOT NULL,
    [FK_Supplier] INT NOT NULL,
    [PurchaseDate] DATE NOT NULL CONSTRAINT [DF_Purchase_PurchaseDate] DEFAULT (CONVERT(DATE, GETDATE())),
    [InvoiceNumber] NVARCHAR(100) NULL,
    [TotalAmount] DECIMAL(12,2) NOT NULL CONSTRAINT [DF_Purchase_TotalAmount] DEFAULT ((0)),
    [Notes] NVARCHAR(500) NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Purchase_CreatedOn] DEFAULT (GETDATE()),
    [EnterBy] INT NULL,
    [Cancelled] BIT NOT NULL CONSTRAINT [DF_Purchase_Cancelled] DEFAULT ((0)),
    [CancelledOn] DATETIME NULL,
    [CancelledReason] NVARCHAR(500) NULL,
    [CancelledBy] INT NULL,
    CONSTRAINT [PK_Purchase] PRIMARY KEY CLUSTERED ([ID_Purchase] ASC),
    CONSTRAINT [FK_Purchase_Supplier] FOREIGN KEY ([FK_Supplier]) REFERENCES [dbo].[Supplier]([ID_Supplier])
);
GO