SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[homepage_banners]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[homepage_banners](
        [id_homepage_banner] INT IDENTITY(1,1) NOT NULL,
        [title] NVARCHAR(255) NULL,
        [subtitle] NVARCHAR(500) NULL,
        [image_url] NVARCHAR(500) NULL,
        [target_url] NVARCHAR(500) NULL,
        [is_active] BIT NOT NULL CONSTRAINT [DF_homepage_banners_is_active] DEFAULT ((1)),
        [display_order] INT NOT NULL CONSTRAINT [DF_homepage_banners_display_order] DEFAULT ((0)),
        [created_at] DATETIME NOT NULL CONSTRAINT [DF_homepage_banners_created_at] DEFAULT (GETDATE()),
        [updated_at] DATETIME NULL,
        [cancelled] BIT NOT NULL CONSTRAINT [DF_homepage_banners_cancelled] DEFAULT ((0)),
        [cancelled_on] DATETIME NULL,
        [cancelled_reason] NVARCHAR(500) NULL,
        CONSTRAINT [PK_homepage_banners] PRIMARY KEY CLUSTERED ([id_homepage_banner] ASC)
    );
END
GO
