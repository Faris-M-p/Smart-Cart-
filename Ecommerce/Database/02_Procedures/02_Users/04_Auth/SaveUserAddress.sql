SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[SaveUserAddress]
(
    @UserId INT,
    @AddressId INT = 0,
    @AddressType NVARCHAR(50) = N'Home',
    @ReceiverName NVARCHAR(200),
    @Phone NVARCHAR(20),
    @AddressLine NVARCHAR(300),
    @City NVARCHAR(100),
    @Pincode NVARCHAR(20),
    @Latitude DECIMAL(10, 7) = NULL,
    @Longitude DECIMAL(10, 7) = NULL,
    @IsDefault BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @AddressType = ISNULL(NULLIF(LTRIM(RTRIM(@AddressType)), N''), N'Home');
    SET @ReceiverName = LTRIM(RTRIM(ISNULL(@ReceiverName, N'')));
    SET @Phone = LTRIM(RTRIM(ISNULL(@Phone, N'')));
    SET @AddressLine = LTRIM(RTRIM(ISNULL(@AddressLine, N'')));
    SET @City = LTRIM(RTRIM(ISNULL(@City, N'')));
    SET @Pincode = LTRIM(RTRIM(ISNULL(@Pincode, N'')));

    IF @ReceiverName = N''
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'Receiver name is required.' AS ResponseMsg;
        RETURN;
    END;

    IF @Phone = N''
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'Phone number is required.' AS ResponseMsg;
        RETURN;
    END;

    IF @AddressLine = N''
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'Address line is required.' AS ResponseMsg;
        RETURN;
    END;

    IF @City = N''
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'City is required.' AS ResponseMsg;
        RETURN;
    END;

    IF @Pincode = N''
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'Pincode is required.' AS ResponseMsg;
        RETURN;
    END;

    -- Enforce strictly 1 address per type per user (Home, Work, Office, Other)
    IF ISNULL(@AddressId, 0) <= 0
    BEGIN
        SELECT TOP 1 @AddressId = AddressId
        FROM [dbo].[UserAddresses]
        WHERE UserId = @UserId 
          AND LOWER(AddressType) = LOWER(@AddressType) 
          AND ISNULL(Cancelled, 0) = 0;
    END;

    IF @IsDefault = 1
    BEGIN
        UPDATE [dbo].[UserAddresses]
        SET IsDefault = 0
        WHERE UserId = @UserId AND ISNULL(Cancelled, 0) = 0;
    END;

    IF ISNULL(@AddressId, 0) > 0 AND EXISTS (SELECT 1 FROM [dbo].[UserAddresses] WHERE AddressId = @AddressId AND UserId = @UserId AND ISNULL(Cancelled, 0) = 0)
    BEGIN
        UPDATE [dbo].[UserAddresses]
        SET AddressType = @AddressType,
            ReceiverName = @ReceiverName,
            Phone = @Phone,
            AddressLine = @AddressLine,
            City = @City,
            Pincode = @Pincode,
            Latitude = @Latitude,
            Longitude = @Longitude,
            IsDefault = @IsDefault
        WHERE AddressId = @AddressId AND UserId = @UserId;

        SELECT @AddressId AS ResponseCode, CAST(1 AS BIT) AS StatusCode, N'Address updated successfully.' AS ResponseMsg;
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAddresses] WHERE UserId = @UserId AND ISNULL(Cancelled, 0) = 0)
        BEGIN
            SET @IsDefault = 1;
        END;

        INSERT INTO [dbo].[UserAddresses]
        (
            UserId, AddressType, ReceiverName, Phone, AddressLine, City, Pincode, Latitude, Longitude, IsDefault, CreatedAt, Cancelled
        )
        VALUES
        (
            @UserId, @AddressType, @ReceiverName, @Phone, @AddressLine, @City, @Pincode, @Latitude, @Longitude, @IsDefault, GETDATE(), 0
        );

        DECLARE @NewId INT = SCOPE_IDENTITY();
        SELECT @NewId AS ResponseCode, CAST(1 AS BIT) AS StatusCode, N'Address saved successfully.' AS ResponseMsg;
    END;
END;
GO
