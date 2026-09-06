SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ToggleWishlist
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Love-button toggle. Max 10 active wishlist products.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ToggleWishlist]
(
    @UserId INT,
    @ProductId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@UserId, 0) < 1
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please log in to use the wishlist.' AS ResponseMsg;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[Products]
        WHERE ID_Product = @ProductId
          AND ISNULL(Cancelled, 0) = 0
          AND IsActive = 1
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This product is not available.' AS ResponseMsg;
        RETURN;
    END

    DECLARE @WishlistId INT;
    DECLARE @ExistingItemId INT;
    DECLARE @ActiveCount INT;

    SELECT @WishlistId = WishlistId
    FROM [dbo].[Wishlist]
    WHERE UserId = @UserId
      AND ISNULL(Cancelled, 0) = 0;

    IF @WishlistId IS NULL
    BEGIN
        INSERT INTO [dbo].[Wishlist] ([UserId], [CreatedAt], [Cancelled])
        VALUES (@UserId, GETDATE(), 0);
        SET @WishlistId = SCOPE_IDENTITY();
    END

    SELECT @ExistingItemId = WishlistItemId
    FROM [dbo].[WishlistItems]
    WHERE WishlistId = @WishlistId
      AND ProductId = @ProductId
      AND ISNULL(Cancelled, 0) = 0;

    IF @ExistingItemId IS NOT NULL
    BEGIN
        UPDATE [dbo].[WishlistItems]
        SET Cancelled = 1,
            CancelledOn = GETDATE(),
            CancelledReason = N'Removed from wishlist'
        WHERE WishlistItemId = @ExistingItemId;

        SELECT 0 AS ResponseCode, 1 AS StatusCode, N'Removed from wishlist.' AS ResponseMsg;
        RETURN;
    END

    SELECT @ActiveCount = COUNT(1)
    FROM [dbo].[WishlistItems]
    WHERE WishlistId = @WishlistId
      AND ISNULL(Cancelled, 0) = 0;

    IF @ActiveCount >= 10
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Your wishlist can hold 10 products. Remove one before adding another.' AS ResponseMsg;
        RETURN;
    END

    INSERT INTO [dbo].[WishlistItems] ([WishlistId], [ProductId], [CreatedAt], [Cancelled])
    VALUES (@WishlistId, @ProductId, GETDATE(), 0);

    SELECT SCOPE_IDENTITY() AS ResponseCode, 1 AS StatusCode, N'Added to wishlist.' AS ResponseMsg;
END
GO
