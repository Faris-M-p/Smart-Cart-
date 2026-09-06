SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ClearCart
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ClearCart]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE CI
    FROM [dbo].[CartItems] AS CI
    INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
    WHERE C.UserId = @UserId;

    SELECT 0 AS ResponseCode, 1 AS StatusCode, N'Cart cleared.' AS ResponseMsg;
END
GO
