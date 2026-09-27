SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[GetUserAddresses]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AddressId,
        UserId,
        AddressType,
        ReceiverName,
        Phone,
        AddressLine,
        City,
        Pincode,
        Latitude,
        Longitude,
        IsDefault,
        CreatedAt
    FROM [dbo].[UserAddresses] WITH (NOLOCK)
    WHERE UserId = @UserId
      AND ISNULL(Cancelled, 0) = 0
    ORDER BY IsDefault DESC, AddressId DESC;
END;
GO
