SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[DeleteUserAddress]
(
    @UserId INT,
    @AddressId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [dbo].[UserAddresses] WHERE AddressId = @AddressId AND UserId = @UserId AND ISNULL(Cancelled, 0) = 0)
    BEGIN
        UPDATE [dbo].[UserAddresses]
        SET Cancelled = 1
        WHERE AddressId = @AddressId AND UserId = @UserId;

        SELECT @AddressId AS ResponseCode, CAST(1 AS BIT) AS StatusCode, N'Address deleted successfully.' AS ResponseMsg;
    END
    ELSE
    BEGIN
        SELECT -1 AS ResponseCode, CAST(0 AS BIT) AS StatusCode, N'Address not found.' AS ResponseMsg;
    END;
END;
GO
