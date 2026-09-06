SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : UpdateCartItem
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[UpdateCartItem]
(
    @UserId INT,
    @CartItemId INT,
    @Quantity INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Quantity, 0) < 1
    BEGIN
        DELETE CI
        FROM [dbo].[CartItems] AS CI
        INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
        WHERE CI.CartItemId = @CartItemId
          AND C.UserId = @UserId;

        SELECT 0 AS ResponseCode, 1 AS StatusCode, N'Item removed from cart.' AS ResponseMsg;
        RETURN;
    END

    DECLARE @ProductVariantId INT;
    DECLARE @StockQty INT;
    DECLARE @MaxOrderQty INT;
    DECLARE @Price DECIMAL(10, 2);

    SELECT
        @ProductVariantId = CI.ProductVariantId,
        @Price = PV.SellingPrice,
        @MaxOrderQty = ISNULL(PV.MaxOrderQty, 10)
    FROM [dbo].[CartItems] AS CI
    INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
    INNER JOIN [dbo].[ProductVariants] AS PV ON PV.ID_ProductVariant = CI.ProductVariantId
    WHERE CI.CartItemId = @CartItemId
      AND C.UserId = @UserId;

    IF @ProductVariantId IS NULL
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Cart item was not found.' AS ResponseMsg;
        RETURN;
    END

    SELECT @StockQty = ISNULL(SUM(S.Quantity), 0)
    FROM [dbo].[Stock] AS S
    WHERE S.FK_ProductVariant = @ProductVariantId
      AND ISNULL(S.Cancelled, 0) = 0;

    IF @Quantity > @StockQty
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Only ' + CAST(@StockQty AS NVARCHAR(10)) + N' units are in stock.' AS ResponseMsg;
        RETURN;
    END

    IF @MaxOrderQty > 0 AND @Quantity > @MaxOrderQty
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'You can add up to ' + CAST(@MaxOrderQty AS NVARCHAR(10)) + N' of this item.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[CartItems]
    SET Quantity = @Quantity,
        Price = ISNULL(@Price, Price)
    WHERE CartItemId = @CartItemId;

    SELECT @CartItemId AS ResponseCode, 1 AS StatusCode, N'Cart updated.' AS ResponseMsg;
END
GO
