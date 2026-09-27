/* =============================================================================
   UserAddresses Table
   -----------------------------------------------------------------------------
   Stores customer saved delivery addresses with address type (Home, Work, Office, Other).
   ============================================================================= */

IF OBJECT_ID(N'[dbo].[UserAddresses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserAddresses] (
        [AddressId] INT IDENTITY(1,1) NOT NULL,
        [UserId] INT NOT NULL,
        [AddressType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_UserAddresses_AddressType] DEFAULT N'Home',
        [ReceiverName] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(20) NOT NULL,
        [AddressLine] NVARCHAR(300) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [Pincode] NVARCHAR(20) NOT NULL,
        [Latitude] DECIMAL(10, 7) NULL,
        [Longitude] DECIMAL(10, 7) NULL,
        [IsDefault] BIT NOT NULL CONSTRAINT [DF_UserAddresses_IsDefault] DEFAULT 0,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_UserAddresses_CreatedAt] DEFAULT GETDATE(),
        [Cancelled] BIT NOT NULL CONSTRAINT [DF_UserAddresses_Cancelled] DEFAULT 0,
        CONSTRAINT [PK_UserAddresses] PRIMARY KEY CLUSTERED ([AddressId] ASC),
        CONSTRAINT [FK_UserAddresses_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
    );

    CREATE NONCLUSTERED INDEX [IX_UserAddresses_UserId] ON [dbo].[UserAddresses] ([UserId], [Cancelled]);
END;
GO
