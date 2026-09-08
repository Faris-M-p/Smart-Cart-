SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : AdminConfirmOrder
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Accept a placed order so it can be packed and delivered.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[AdminConfirmOrder]
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

    IF @OrderStatus NOT IN (N'Placed', N'Pending')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Only placed orders can be confirmed.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[Orders]
    SET OrderStatus = N'Confirmed'
    WHERE OrderId = @OrderId
      AND ISNULL(Cancelled, 0) = 0
      AND OrderStatus IN (N'Placed', N'Pending');

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Order could not be confirmed.' AS ResponseMsg;
        RETURN;
    END

    SELECT @OrderId AS ResponseCode, 1 AS StatusCode, N'Order confirmed.' AS ResponseMsg;
END
GO
