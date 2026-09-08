SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetAdminOrders
Created By       : Muhammed Faris
Created On       : 08/09/2026

PURPOSE
  Admin order list for all customers. Search, status, date, paging.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetAdminOrders]
(
    @SearchText NVARCHAR(255) = N'',
    @OrderStatus NVARCHAR(50) = N'',
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@PageIndex, 0) < 1
        SET @PageIndex = 1;
    IF ISNULL(@PageSize, 0) < 1
        SET @PageSize = 10;
    IF @PageSize > 100
        SET @PageSize = 100;

    SET @SearchText = LTRIM(RTRIM(ISNULL(@SearchText, N'')));
    SET @OrderStatus = LTRIM(RTRIM(ISNULL(@OrderStatus, N'')));

    ;WITH BaseOrders AS
    (
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
            ISNULL(O.ReceiverName, N'') AS ReceiverName,
            ISNULL(O.Phone, N'') AS Phone,
            ISNULL(O.City, N'') AS City,
            ISNULL(NULLIF(LTRIM(RTRIM(U.FullName)), N''), ISNULL(U.UserName, N'')) AS CustomerName,
            ISNULL(U.Email, N'') AS CustomerEmail,
            CAST(ISNULL(O.Cancelled, 0) AS BIT) AS Cancelled,
            ISNULL(Counts.ItemCount, 0) AS ItemCount,
            ISNULL(FirstItem.FirstProductName, N'') AS FirstProductName,
            ISNULL(FirstItem.FirstImageUrl, N'') AS FirstImageUrl
        FROM [dbo].[Orders] AS O WITH (NOLOCK)
        LEFT JOIN [dbo].[Users] AS U WITH (NOLOCK) ON U.UserId = O.UserId
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
    )
    SELECT *
    INTO #Filtered
    FROM BaseOrders
    WHERE (@SearchText = N''
           OR OrderNumber LIKE N'%' + @SearchText + N'%'
           OR ReceiverName LIKE N'%' + @SearchText + N'%'
           OR Phone LIKE N'%' + @SearchText + N'%'
           OR City LIKE N'%' + @SearchText + N'%'
           OR CustomerName LIKE N'%' + @SearchText + N'%'
           OR CustomerEmail LIKE N'%' + @SearchText + N'%')
      AND (@OrderStatus = N'' OR OrderStatus = @OrderStatus)
      AND (@FromDate IS NULL OR CAST(OrderDate AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(OrderDate AS DATE) <= @ToDate);

    DECLARE @TotalCount INT = (SELECT COUNT(*) FROM #Filtered);

    SELECT
        OrderId,
        OrderNumber,
        OrderDate,
        TotalAmount,
        OrderStatus,
        PaymentMethod,
        PaymentStatus,
        ReceiverName,
        Phone,
        City,
        CustomerName,
        Cancelled,
        ItemCount,
        FirstProductName,
        FirstImageUrl,
        CAST(CASE WHEN Cancelled = 0 AND OrderStatus IN (N'Placed', N'Pending') THEN 1 ELSE 0 END AS BIT) AS CanConfirm,
        CAST(CASE WHEN Cancelled = 0 AND OrderStatus = N'Confirmed' THEN 1 ELSE 0 END AS BIT) AS CanUpdateStatus,
        CAST(CASE WHEN Cancelled = 0 AND OrderStatus IN (N'Confirmed', N'Shipped') THEN 1 ELSE 0 END AS BIT) AS CanDeliver,
        CAST(CASE WHEN Cancelled = 0 AND OrderStatus IN (N'Placed', N'Pending', N'Confirmed') THEN 1 ELSE 0 END AS BIT) AS CanCancel
    FROM #Filtered
    ORDER BY OrderDate DESC, OrderId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT
        @TotalCount AS TotalCount,
        @PageSize AS PageSize,
        @PageIndex AS PageIndex;

    DROP TABLE #Filtered;
END
GO
