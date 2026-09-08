SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetAdminOrder
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Admin order header and lines. Not limited to one customer.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetAdminOrder]
(
    @OrderId INT
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
        ISNULL((
            SELECT TOP (1) S.ShippingStatus
            FROM [dbo].[Shipping] AS S WITH (NOLOCK)
            WHERE S.OrderId = O.OrderId
            ORDER BY S.ShippingId DESC
        ), N'Pending') AS ShippingStatus,
        ISNULL(O.ReceiverName, N'') AS ReceiverName,
        ISNULL(O.Phone, N'') AS Phone,
        ISNULL(O.AddressLine, N'') AS AddressLine,
        ISNULL(O.City, N'') AS City,
        ISNULL(O.Pincode, N'') AS Pincode,
        ISNULL(O.ShippingAddress, N'') AS ShippingAddress,
        ISNULL(NULLIF(LTRIM(RTRIM(U.FullName)), N''), ISNULL(U.UserName, N'')) AS CustomerName,
        ISNULL(U.Email, N'') AS CustomerEmail,
        CAST(ISNULL(O.Cancelled, 0) AS BIT) AS Cancelled,
        O.CancelledOn,
        ISNULL(O.CancelledReason, N'') AS CancelledReason,
        CAST(CASE WHEN ISNULL(O.Cancelled, 0) = 0 AND O.OrderStatus IN (N'Placed', N'Pending') THEN 1 ELSE 0 END AS BIT) AS CanConfirm,
        CAST(CASE WHEN ISNULL(O.Cancelled, 0) = 0 AND O.OrderStatus = N'Confirmed' THEN 1 ELSE 0 END AS BIT) AS CanUpdateStatus,
        CAST(CASE WHEN ISNULL(O.Cancelled, 0) = 0 AND O.OrderStatus IN (N'Confirmed', N'Shipped') THEN 1 ELSE 0 END AS BIT) AS CanDeliver,
        CAST(CASE WHEN ISNULL(O.Cancelled, 0) = 0 AND O.OrderStatus IN (N'Placed', N'Pending', N'Confirmed') THEN 1 ELSE 0 END AS BIT) AS CanCancel
    FROM [dbo].[Orders] AS O WITH (NOLOCK)
    LEFT JOIN [dbo].[Users] AS U WITH (NOLOCK) ON U.UserId = O.UserId
    WHERE O.OrderId = @OrderId;

    SELECT
        OI.ID_OrderItem AS OrderItemId,
        OI.FK_Product AS ProductId,
        ISNULL(OI.FK_ProductVariant, 0) AS ProductVariantId,
        OI.ProductName AS Name,
        ISNULL(PR.Slug, N'') AS Slug,
        ISNULL(OI.VariantLabel, N'') AS Label,
        ISNULL(OI.SKU, N'') AS SKU,
        OI.UnitPrice AS Price,
        OI.UnitPrice AS MRP,
        OI.Quantity,
        OI.LineTotal,
        CAST(1 AS BIT) AS InStock,
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
        )) AS ImageUrl
    FROM [dbo].[OrderItems] AS OI WITH (NOLOCK)
    LEFT JOIN [dbo].[Products] AS PR WITH (NOLOCK) ON PR.ID_Product = OI.FK_Product
    WHERE OI.FK_Order = @OrderId
    ORDER BY OI.ID_OrderItem;
END
GO
