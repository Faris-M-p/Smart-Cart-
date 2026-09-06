SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetWishlistStatus
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetWishlistStatus]
(
    @UserId INT,
    @ProductId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM [dbo].[WishlistItems] AS WI WITH (NOLOCK)
        INNER JOIN [dbo].[Wishlist] AS W WITH (NOLOCK) ON W.WishlistId = WI.WishlistId
        WHERE W.UserId = @UserId
          AND WI.ProductId = @ProductId
          AND ISNULL(W.Cancelled, 0) = 0
          AND ISNULL(WI.Cancelled, 0) = 0
    ) THEN 1 ELSE 0 END AS BIT) AS InWishlist;
END
GO
