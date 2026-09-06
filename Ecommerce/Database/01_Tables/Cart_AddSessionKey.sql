IF OBJECT_ID(N'[dbo].[Cart]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Cart', N'SessionKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cart]
        ADD [SessionKey] UNIQUEIDENTIFIER NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Cart]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Cart', N'SessionKey') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_Cart_SessionKey'
          AND object_id = OBJECT_ID(N'dbo.Cart')
   )
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Cart_SessionKey]
        ON [dbo].[Cart] ([SessionKey])
        WHERE [SessionKey] IS NOT NULL;
END
GO
