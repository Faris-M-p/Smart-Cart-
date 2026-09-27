SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[homepage_categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[homepage_categories](
        [id_homepagecategory] INT IDENTITY(1,1) NOT NULL,
        [fk_category] INT NOT NULL,
        [isactive] BIT NOT NULL CONSTRAINT [DF_homepage_categories_isactive] DEFAULT ((1)),
        [displayorder] INT NOT NULL CONSTRAINT [DF_homepage_categories_displayorder] DEFAULT ((0)),
        [createdat] DATETIME NOT NULL CONSTRAINT [DF_homepage_categories_createdat] DEFAULT (GETDATE()),
        [updatedat] DATETIME NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_homepage_categories_cancelled] DEFAULT ((0)),
        [cancelledon] DATETIME NULL,
        [cancelledreason] NVARCHAR(500) NULL,
        CONSTRAINT [PK_homepage_categories] PRIMARY KEY CLUSTERED ([id_homepagecategory] ASC),
        CONSTRAINT [UQ_homepage_categories_category] UNIQUE ([fk_category]),
        CONSTRAINT [FK_hc_category] FOREIGN KEY ([fk_category]) REFERENCES [dbo].[Category] ([ID_Category])
    );
END
GO
