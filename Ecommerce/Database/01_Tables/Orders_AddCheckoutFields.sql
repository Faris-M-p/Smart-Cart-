IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'OrderNumber') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [OrderNumber] NVARCHAR(30) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'ReceiverName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [ReceiverName] NVARCHAR(150) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'Phone') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [Phone] NVARCHAR(15) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'AddressLine') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [AddressLine] NVARCHAR(300) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'City') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [City] NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Orders', N'Pincode') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
        ADD [Pincode] NVARCHAR(10) NULL;
END
GO
