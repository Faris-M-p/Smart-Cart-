SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetOrders
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Current user's order list, including soft-cancelled orders.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetOrders]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        O.OrderId,
        ISNULL(O.OrderNumber, N'SC' + RIGHT(N'000000' + CAST(O.OrderId AS NVARCHAR(10)), 6)) AS OrderNumber,
        O.OrderDate,
        O.TotalAmount,
        CASE WHEN ISNULL(O.Cancelled, 0) = 1 THEN N'Cancelled' ELSE O.OrderStatus END AS OrderStatus,
        O.PaymentMethod,
        ISNULL((
            SELECT TOP (1) P.PaymentStatus
            FROM [dbo].[Payments] AS P WITH (NOLOCK)
            WHERE P.OrderId = O.OrderId
            ORDER BY P.PaymentId DESC
        ), N'Pending') AS PaymentStatus,
        CAST(ISNULL(O.Cancelled, 0) AS BIT) AS Cancelled,
        CAST(
            CASE
                WHEN ISNULL(O.Cancelled, 0) = 0
                 AND O.OrderStatus IN (N'Placed', N'Pending')
                 AND NOT EXISTS (
                    SELECT 1
                    FROM [dbo].[Shipping] AS S WITH (NOLOCK)
                    WHERE S.OrderId = O.OrderId
                      AND ISNULL(S.Cancelled, 0) = 0
                      AND S.ShippingStatus IN (N'Shipped', N'Out for delivery', N'Delivered', N'In Transit')
                 )
                THEN 1
                ELSE 0
            END AS BIT
        ) AS CanCancel,
        ISNULL(Counts.ItemCount, 0) AS ItemCount,
        ISNULL(FirstItem.FirstProductName, N'') AS FirstProductName,
        ISNULL(FirstItem.FirstImageUrl, N'') AS FirstImageUrl
    FROM [dbo].[Orders] AS O WITH (NOLOCK)
    OUTER APPLY
    (
        SELECT COUNT(*) AS ItemCount
        FROM [dbo].[OrderItems] AS OI WITH (NOLOCK)
        WHERE OI.FK_Order = O.OrderId
    ) AS Counts
    OUTER APPLY
    (
        SELECT TOP (1)
            OI.ProductName AS FirstProductName,
            ISNULL((
                SELECT TOP (1) SM.MediaUrl
                FROM [dbo].[SkuMedia] AS SM WITH (NOLOCK)
                WHERE SM.FK_ProductSKU = OI.FK_ProductVariant
                  AND SM.MediaUrl IS NOT NULL
                  AND SM.MediaUrl <> N''
                ORDER BY SM.IsPrimary DESC, SM.DisplayOrder ASC
            ), (
                SELECT TOP (1) PM.MediaUrl
                FROM [dbo].[ProductMedia] AS PM WITH (NOLOCK)
                WHERE PM.FK_Product = OI.FK_Product
                  AND PM.MediaType = N'Image'
                  AND PM.MediaUrl IS NOT NULL
                  AND PM.MediaUrl <> N''
                ORDER BY PM.IsPrimary DESC, PM.DisplayOrder ASC
            )) AS FirstImageUrl
        FROM [dbo].[OrderItems] AS OI WITH (NOLOCK)
        WHERE OI.FK_Order = O.OrderId
        ORDER BY OI.ID_OrderItem
    ) AS FirstItem
    WHERE O.UserId = @UserId
    ORDER BY O.OrderDate DESC, O.OrderId DESC;
END
GO
