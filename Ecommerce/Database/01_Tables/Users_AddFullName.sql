IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'FullName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
        ADD [FullName] NVARCHAR(150) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'UserName') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Users]
        ALTER COLUMN [UserName] NVARCHAR(100) NOT NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_Users_Email'
          AND object_id = OBJECT_ID(N'dbo.Users')
   )
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_Email]
        ON [dbo].[Users] ([Email]);
END
GO
