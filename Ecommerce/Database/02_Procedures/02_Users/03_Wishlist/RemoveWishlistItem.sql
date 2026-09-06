SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : RemoveWishlistItem
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[RemoveWishlistItem]
(
    @UserId INT,
    @WishlistItemId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE WI
    SET Cancelled = 1,
        CancelledOn = GETDATE(),
        CancelledReason = N'Removed from wishlist'
    FROM [dbo].[WishlistItems] AS WI
    INNER JOIN [dbo].[Wishlist] AS W ON W.WishlistId = WI.WishlistId
    WHERE WI.WishlistItemId = @WishlistItemId
      AND W.UserId = @UserId
      AND ISNULL(WI.Cancelled, 0) = 0
      AND ISNULL(W.Cancelled, 0) = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Wishlist item was not found.' AS ResponseMsg;
        RETURN;
    END

    SELECT 0 AS ResponseCode, 1 AS StatusCode, N'Removed from wishlist.' AS ResponseMsg;
END
GO
