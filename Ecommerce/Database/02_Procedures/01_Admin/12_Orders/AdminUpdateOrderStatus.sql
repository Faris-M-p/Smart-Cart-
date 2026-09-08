SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : AdminUpdateOrderStatus
Created By       : Muhammed Faris
Created On       : 09/09/2026

PURPOSE
  Let admin set Shipped after Confirm and before Deliver.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[AdminUpdateOrderStatus]
(
    @OrderId INT,
    @OrderStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @IsCancelled BIT;
    DECLARE @NextStatus NVARCHAR(50) = LTRIM(RTRIM(ISNULL(@OrderStatus, N'')));

    SELECT
        @CurrentStatus = O.OrderStatus,
        @IsCancelled = ISNULL(O.Cancelled, 0)
    FROM [dbo].[Orders] AS O
    WHERE O.OrderId = @OrderId;

    IF @CurrentStatus IS NULL
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order was not found.' AS ResponseMsg;
        RETURN;
    END

    IF @IsCancelled = 1 OR @CurrentStatus = N'Cancelled'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This order is cancelled.' AS ResponseMsg;
        RETURN;
    END

    IF @NextStatus <> N'Shipped'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Only Shipped can be set from here. Use Confirm or Deliver for the other steps.' AS ResponseMsg;
        RETURN;
    END

    IF @CurrentStatus <> N'Confirmed'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Confirm the order before setting it as Shipped.' AS ResponseMsg;
        RETURN;
    END

    BEGIN TRAN;

    UPDATE [dbo].[Orders]
    SET OrderStatus = N'Shipped'
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0
      AND OrderStatus = N'Confirmed';

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRAN;
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order status could not be updated.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[Shipping]
    SET ShippingStatus = N'Shipped',
        ShippingDate = GETDATE()
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0;

    COMMIT TRAN;

    SELECT @OrderId AS ResponseCode, 1 AS StatusCode, N'Order status updated to Shipped.' AS ResponseMsg;
END
GO
