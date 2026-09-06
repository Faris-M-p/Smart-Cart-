IF OBJECT_ID(N'[dbo].[ProductMedia]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductMedia]
    (
        [ID_ProductMedia] INT IDENTITY(1,1) NOT NULL,
        [FK_Product] INT NOT NULL,
        [MediaType] NVARCHAR(20) NOT NULL,
        [MediaUrl] NVARCHAR(500) NOT NULL,
        [DisplayOrder] INT NOT NULL CONSTRAINT [DF_ProductMedia_DisplayOrder] DEFAULT ((0)),
        [IsPrimary] BIT NOT NULL CONSTRAINT [DF_ProductMedia_IsPrimary] DEFAULT ((0)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_ProductMedia_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [PK_ProductMedia] PRIMARY KEY CLUSTERED ([ID_ProductMedia] ASC),
        CONSTRAINT [FK_ProductMedia_Product] FOREIGN KEY ([FK_Product])
            REFERENCES [dbo].[Products] ([ID_Product]),
        CONSTRAINT [CK_ProductMedia_MediaType] CHECK ([MediaType] IN (N'Image', N'Video'))
    );

    CREATE NONCLUSTERED INDEX [IX_ProductMedia_FK_Product]
        ON [dbo].[ProductMedia] ([FK_Product] ASC, [DisplayOrder] ASC);
END
GO
