IF OBJECT_ID(N'[dbo].[AdminUsers]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AdminUsers', N'ProfileImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[AdminUsers]
        ADD [ProfileImageUrl] NVARCHAR(500) NULL;
END
GO
