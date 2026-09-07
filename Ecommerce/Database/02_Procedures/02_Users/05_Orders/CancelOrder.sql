SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : CancelOrder
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Soft-cancel the current user's order, restore stock, and mark
  payment / shipping cancelled. Rows stay in the database.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[CancelOrder]
(
    @UserId INT,
    @OrderId INT,
    @Reason NVARCHAR(255) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus NVARCHAR(50);
    DECLARE @IsCancelled BIT;
    DECLARE @ShippingStatus NVARCHAR(50);
    DECLARE @CancelReason NVARCHAR(255) = NULLIF(LTRIM(RTRIM(ISNULL(@Reason, N''))), N'');

    IF @CancelReason IS NULL
    BEGIN
        SET @CancelReason = N'Cancelled by customer';
    END

    SELECT
        @OrderStatus = O.OrderStatus,
        @IsCancelled = ISNULL(O.Cancelled, 0)
    FROM [dbo].[Orders] AS O
    WHERE O.OrderId = @OrderId
      AND O.UserId = @UserId;

    IF @OrderStatus IS NULL
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order was not found.' AS ResponseMsg;
        RETURN;
    END

    IF @IsCancelled = 1
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This order is already cancelled.' AS ResponseMsg;
        RETURN;
    END

    IF @OrderStatus NOT IN (N'Placed', N'Pending')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This order can no longer be cancelled.' AS ResponseMsg;
        RETURN;
    END

    SELECT TOP (1)
        @ShippingStatus = S.ShippingStatus
    FROM [dbo].[Shipping] AS S
    WHERE S.OrderId = @OrderId
      AND ISNULL(S.Cancelled, 0) = 0
    ORDER BY S.ShippingId DESC;

    IF @ShippingStatus IN (N'Shipped', N'Out for delivery', N'Delivered', N'In Transit')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This order can no longer be cancelled.' AS ResponseMsg;
        RETURN;
    END

    DECLARE @VariantId INT;
    DECLARE @Qty INT;
    DECLARE @StockId INT;

    BEGIN TRAN;

    DECLARE restock CURSOR LOCAL FAST_FORWARD FOR
        SELECT OI.FK_ProductVariant, OI.Quantity
        FROM [dbo].[OrderItems] AS OI
        WHERE OI.FK_Order = @OrderId
          AND OI.FK_ProductVariant IS NOT NULL
          AND OI.FK_ProductVariant > 0;

    OPEN restock;
    FETCH NEXT FROM restock INTO @VariantId, @Qty;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @StockId = NULL;

        SELECT TOP (1)
            @StockId = S.ID_Stock
        FROM [dbo].[Stock] AS S WITH (UPDLOCK, ROWLOCK)
        WHERE S.FK_ProductVariant = @VariantId
          AND ISNULL(S.Cancelled, 0) = 0
        ORDER BY S.CreatedOn DESC, S.ID_Stock DESC;

        IF @StockId IS NOT NULL
        BEGIN
            UPDATE [dbo].[Stock]
            SET Quantity = Quantity + @Qty
            WHERE ID_Stock = @StockId;
        END

        FETCH NEXT FROM restock INTO @VariantId, @Qty;
    END

    CLOSE restock;
    DEALLOCATE restock;

    UPDATE [dbo].[Payments]
    SET Cancelled = 1,
        CancelledOn = GETDATE(),
        CancelledReason = @CancelReason,
        PaymentStatus = N'Cancelled'
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0;

    UPDATE [dbo].[Shipping]
    SET Cancelled = 1,
        CancelledOn = GETDATE(),
        CancelledReason = @CancelReason,
        ShippingStatus = N'Cancelled'
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0;

    UPDATE [dbo].[Orders]
    SET Cancelled = 1,
        CancelledOn = GETDATE(),
        CancelledReason = @CancelReason,
        OrderStatus = N'Cancelled'
    WHERE OrderId = @OrderId
      AND UserId = @UserId
      AND ISNULL(Cancelled, 0) = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRAN;
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order was not found.' AS ResponseMsg;
        RETURN;
    END

    COMMIT TRAN;

    SELECT @OrderId AS ResponseCode, 1 AS StatusCode, N'Order cancelled.' AS ResponseMsg;
END
GO
