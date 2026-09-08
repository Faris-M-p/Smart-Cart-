SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : AdminDeliverOrder
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Mark a confirmed order delivered and collect COD.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[AdminDeliverOrder]
(
    @OrderId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus NVARCHAR(50);
    DECLARE @IsCancelled BIT;

    SELECT
        @OrderStatus = O.OrderStatus,
        @IsCancelled = ISNULL(O.Cancelled, 0)
    FROM [dbo].[Orders] AS O
    WHERE O.OrderId = @OrderId;

    IF @OrderStatus IS NULL
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order was not found.' AS ResponseMsg;
        RETURN;
    END

    IF @IsCancelled = 1 OR @OrderStatus = N'Cancelled'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This order is cancelled.' AS ResponseMsg;
        RETURN;
    END

    IF @OrderStatus NOT IN (N'Confirmed', N'Shipped')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Confirm the order before marking it delivered.' AS ResponseMsg;
        RETURN;
    END

    BEGIN TRAN;

    UPDATE [dbo].[Orders]
    SET OrderStatus = N'Delivered'
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0
      AND OrderStatus IN (N'Confirmed', N'Shipped');

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRAN;
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order could not be delivered.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[Payments]
    SET PaymentStatus = N'Collected',
        PaymentDate = GETDATE()
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0;

    UPDATE [dbo].[Shipping]
    SET ShippingStatus = N'Delivered',
        ShippingDate = GETDATE()
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0;

    COMMIT TRAN;

    SELECT @OrderId AS ResponseCode, 1 AS StatusCode, N'Order delivered. COD collected.' AS ResponseMsg;
END
GO
