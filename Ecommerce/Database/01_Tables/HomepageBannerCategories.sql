SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[homepage_banner_categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[homepage_banner_categories](
        [id_bannercategory] INT IDENTITY(1,1) NOT NULL,
        [fk_banner] INT NOT NULL,
        [fk_category] INT NOT NULL,
        CONSTRAINT [PK_homepage_banner_categories] PRIMARY KEY CLUSTERED ([id_bannercategory] ASC),
        CONSTRAINT [FK_hbc_banner] FOREIGN KEY ([fk_banner]) REFERENCES [dbo].[homepage_banners] ([id_homepage_banner]) ON DELETE CASCADE,
        CONSTRAINT [FK_hbc_category] FOREIGN KEY ([fk_category]) REFERENCES [dbo].[Category] ([ID_Category])
    );
END
GO
