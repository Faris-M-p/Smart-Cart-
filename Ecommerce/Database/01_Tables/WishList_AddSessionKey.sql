IF OBJECT_ID(N'[dbo].[Wishlist]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Wishlist', N'SessionKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[Wishlist]
        ADD [SessionKey] UNIQUEIDENTIFIER NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Wishlist]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Wishlist', N'SessionKey') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_Wishlist_SessionKey'
          AND object_id = OBJECT_ID(N'dbo.Wishlist')
   )
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Wishlist_SessionKey]
        ON [dbo].[Wishlist] ([SessionKey])
        WHERE [SessionKey] IS NOT NULL;
END
GO
