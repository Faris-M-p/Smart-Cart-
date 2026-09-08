SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : AddCartItem
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Add or increase one selected SKU. Max 20 distinct cart products.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[AddCartItem]
(
    @UserId INT,
    @ProductVariantId INT,
    @Quantity INT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@UserId, 0) < 1
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please log in to add items to cart.' AS ResponseMsg;
        RETURN;
    END

    IF ISNULL(@Quantity, 0) < 1
        SET @Quantity = 1;

    DECLARE @ProductId INT;
    DECLARE @Price DECIMAL(10, 2);
    DECLARE @StockQty INT;
    DECLARE @MaxOrderQty INT;
    DECLARE @CartId INT;
    DECLARE @ExistingItemId INT;
    DECLARE @ExistingQty INT;
    DECLARE @DistinctCount INT;
    DECLARE @NewQty INT;

    SELECT
        @ProductId = PV.FK_Product,
        @Price = PV.SellingPrice,
        @MaxOrderQty = ISNULL(PV.MaxOrderQty, 10)
    FROM [dbo].[ProductVariants] AS PV
    INNER JOIN [dbo].[Products] AS P ON P.ID_Product = PV.FK_Product
    WHERE PV.ID_ProductVariant = @ProductVariantId
      AND ISNULL(PV.Cancelled, 0) = 0
      AND PV.IsActive = 1
      AND ISNULL(PV.SellOnline, 0) = 1
      AND ISNULL(P.Cancelled, 0) = 0
      AND P.IsActive = 1
      AND ISNULL(P.SellOnline, 0) = 1;

    IF @ProductId IS NULL
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This product option is not available.' AS ResponseMsg;
        RETURN;
    END

    SELECT @StockQty = ISNULL(SUM(S.Quantity), 0)
    FROM [dbo].[Stock] AS S
    WHERE S.FK_ProductVariant = @ProductVariantId
      AND ISNULL(S.Cancelled, 0) = 0;

    IF @StockQty < 1
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This variant is out of stock.' AS ResponseMsg;
        RETURN;
    END

    SELECT @CartId = CartId
    FROM [dbo].[Cart]
    WHERE UserId = @UserId;

    IF @CartId IS NULL
    BEGIN
        INSERT INTO [dbo].[Cart] ([UserId], [CreatedAt])
        VALUES (@UserId, GETDATE());
        SET @CartId = SCOPE_IDENTITY();
    END

    SELECT
        @ExistingItemId = CI.CartItemId,
        @ExistingQty = CI.Quantity
    FROM [dbo].[CartItems] AS CI
    WHERE CI.CartId = @CartId
      AND CI.ProductVariantId = @ProductVariantId;

    SET @NewQty = ISNULL(@ExistingQty, 0) + @Quantity;

    IF @ExistingItemId IS NULL
    BEGIN
        SELECT @DistinctCount = COUNT(1)
        FROM [dbo].[CartItems]
        WHERE CartId = @CartId;

        IF @DistinctCount >= 20
        BEGIN
            SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Your cart can hold 20 products. Remove one before adding another.' AS ResponseMsg;
            RETURN;
        END
    END

    IF @NewQty > @StockQty
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Only ' + CAST(@StockQty AS NVARCHAR(10)) + N' units are in stock.' AS ResponseMsg;
        RETURN;
    END

    IF @MaxOrderQty > 0 AND @NewQty > @MaxOrderQty
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'You can add up to ' + CAST(@MaxOrderQty AS NVARCHAR(10)) + N' of this item.' AS ResponseMsg;
        RETURN;
    END

    IF @ExistingItemId IS NULL
    BEGIN
        INSERT INTO [dbo].[CartItems] ([CartId], [ProductId], [ProductVariantId], [Quantity], [Price], [CreatedAt])
        VALUES (@CartId, @ProductId, @ProductVariantId, @NewQty, @Price, GETDATE());
        SET @ExistingItemId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [dbo].[CartItems]
        SET Quantity = @NewQty,
            Price = @Price
        WHERE CartItemId = @ExistingItemId;
    END

    SELECT @ExistingItemId AS ResponseCode, 1 AS StatusCode, N'Added to cart.' AS ResponseMsg;
END
GO
