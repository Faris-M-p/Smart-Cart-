SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetBagCounts
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Header badges: distinct cart SKUs and active wishlist products.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetBagCounts]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL((
            SELECT COUNT(1)
            FROM [dbo].[CartItems] AS CI WITH (NOLOCK)
            INNER JOIN [dbo].[Cart] AS C WITH (NOLOCK) ON C.CartId = CI.CartId
            WHERE C.UserId = @UserId
        ), 0) AS CartCount,
        ISNULL((
            SELECT COUNT(1)
            FROM [dbo].[WishlistItems] AS WI WITH (NOLOCK)
            INNER JOIN [dbo].[Wishlist] AS W WITH (NOLOCK) ON W.WishlistId = WI.WishlistId
            WHERE W.UserId = @UserId
              AND ISNULL(W.Cancelled, 0) = 0
              AND ISNULL(WI.Cancelled, 0) = 0
        ), 0) AS WishlistCount;
END
GO
