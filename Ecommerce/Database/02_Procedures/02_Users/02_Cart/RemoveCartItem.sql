SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : RemoveCartItem
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[RemoveCartItem]
(
    @UserId INT,
    @CartItemId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE CI
    FROM [dbo].[CartItems] AS CI
    INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
    WHERE CI.CartItemId = @CartItemId
      AND C.UserId = @UserId;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Cart item was not found.' AS ResponseMsg;
        RETURN;
    END

    SELECT 0 AS ResponseCode, 1 AS StatusCode, N'Item removed from cart.' AS ResponseMsg;
END
GO
