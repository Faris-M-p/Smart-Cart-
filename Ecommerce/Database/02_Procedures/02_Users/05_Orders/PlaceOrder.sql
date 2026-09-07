SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : PlaceOrder
Created By       : Muhammed Faris
Created On       : 07/09/2026

PURPOSE
  Place a COD order from the cart or a single Buy Now SKU.
  UPI is reserved for a later payment step.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[PlaceOrder]
(
    @UserId INT,
    @ProductVariantId INT = 0,
    @Quantity INT = 1,
    @ReceiverName NVARCHAR(150),
    @Phone NVARCHAR(15),
    @AddressLine NVARCHAR(300),
    @City NVARCHAR(100),
    @Pincode NVARCHAR(10),
    @PaymentMethod NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF ISNULL(@UserId, 0) < 1
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please log in to place an order.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@ReceiverName, N''))) = N''
       OR LEN(LTRIM(RTRIM(@ReceiverName))) < 2
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter the receiver name.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@Phone, N''))) NOT LIKE N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter a 10-digit mobile number.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@AddressLine, N''))) = N''
       OR LEN(LTRIM(RTRIM(@AddressLine))) < 5
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter the delivery address.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@City, N''))) = N''
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter the city.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@Pincode, N''))) NOT LIKE N'[0-9][0-9][0-9][0-9][0-9][0-9]'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter a 6-digit pincode.' AS ResponseMsg;
        RETURN;
    END

    SET @PaymentMethod = UPPER(LTRIM(RTRIM(ISNULL(@PaymentMethod, N''))));
    IF @PaymentMethod = N'UPI'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'UPI will be available soon in this area. Please choose Cash on Delivery.' AS ResponseMsg;
        RETURN;
    END

    IF @PaymentMethod <> N'COD'
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please choose Cash on Delivery.' AS ResponseMsg;
        RETURN;
    END

    IF ISNULL(@Quantity, 0) < 1
        SET @Quantity = 1;

    DECLARE @FullAddress NVARCHAR(MAX) =
        LTRIM(RTRIM(@ReceiverName)) + N', ' +
        LTRIM(RTRIM(@Phone)) + N', ' +
        LTRIM(RTRIM(@AddressLine)) + N', ' +
        LTRIM(RTRIM(@City)) + N' - ' +
        LTRIM(RTRIM(@Pincode));

    CREATE TABLE #Lines
    (
        ProductId INT NOT NULL,
        ProductVariantId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Label NVARCHAR(200) NULL,
        SKU NVARCHAR(100) NULL,
        Price DECIMAL(10, 2) NOT NULL,
        Quantity INT NOT NULL,
        LineTotal DECIMAL(10, 2) NOT NULL,
        StockQty INT NOT NULL
    );

    IF ISNULL(@ProductVariantId, 0) > 0
    BEGIN
        INSERT INTO #Lines
        SELECT
            P.ID_Product,
            PV.ID_ProductVariant,
            P.Name,
            CASE
                WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN ISNULL(PV.SKU, N'')
                ELSE PV.VariantLabel
            END,
            ISNULL(PV.SKU, N''),
            PV.SellingPrice,
            @Quantity,
            PV.SellingPrice * @Quantity,
            ISNULL(ST.Qty, 0)
        FROM [dbo].[ProductVariants] AS PV
        INNER JOIN [dbo].[Products] AS P ON P.ID_Product = PV.FK_Product
        LEFT JOIN (
            SELECT FK_ProductVariant, SUM(Quantity) AS Qty
            FROM [dbo].[Stock]
            WHERE ISNULL(Cancelled, 0) = 0
            GROUP BY FK_ProductVariant
        ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
        WHERE PV.ID_ProductVariant = @ProductVariantId
          AND ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(P.Cancelled, 0) = 0
          AND P.IsActive = 1;
    END
    ELSE
    BEGIN
        INSERT INTO #Lines
        SELECT
            CI.ProductId,
            CI.ProductVariantId,
            P.Name,
            CASE
                WHEN ISNULL(PV.VariantLabel, N'') = N'' THEN ISNULL(PV.SKU, N'')
                ELSE PV.VariantLabel
            END,
            ISNULL(PV.SKU, N''),
            ISNULL(PV.SellingPrice, CI.Price),
            CI.Quantity,
            ISNULL(PV.SellingPrice, CI.Price) * CI.Quantity,
            ISNULL(ST.Qty, 0)
        FROM [dbo].[CartItems] AS CI
        INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
        INNER JOIN [dbo].[Products] AS P ON P.ID_Product = CI.ProductId
        INNER JOIN [dbo].[ProductVariants] AS PV ON PV.ID_ProductVariant = CI.ProductVariantId
        LEFT JOIN (
            SELECT FK_ProductVariant, SUM(Quantity) AS Qty
            FROM [dbo].[Stock]
            WHERE ISNULL(Cancelled, 0) = 0
            GROUP BY FK_ProductVariant
        ) AS ST ON ST.FK_ProductVariant = PV.ID_ProductVariant
        WHERE C.UserId = @UserId
          AND ISNULL(PV.Cancelled, 0) = 0
          AND PV.IsActive = 1
          AND ISNULL(P.Cancelled, 0) = 0
          AND P.IsActive = 1;
    END

    IF NOT EXISTS (SELECT 1 FROM #Lines)
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'There is nothing to order.' AS ResponseMsg;
        DROP TABLE #Lines;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM #Lines WHERE StockQty < Quantity OR ProductVariantId IS NULL OR ProductVariantId < 1)
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'One or more items are out of stock. Update the cart and try again.' AS ResponseMsg;
        DROP TABLE #Lines;
        RETURN;
    END

    DECLARE @Total DECIMAL(10, 2) = (SELECT SUM(LineTotal) FROM #Lines);
    DECLARE @OrderId INT;

    BEGIN TRAN;

    INSERT INTO [dbo].[Orders]
    (
        [UserId],
        [OrderDate],
        [TotalAmount],
        [OrderStatus],
        [ShippingAddress],
        [PaymentMethod],
        [Cancelled],
        [ReceiverName],
        [Phone],
        [AddressLine],
        [City],
        [Pincode]
    )
    VALUES
    (
        @UserId,
        GETDATE(),
        @Total,
        N'Placed',
        @FullAddress,
        N'COD',
        0,
        LTRIM(RTRIM(@ReceiverName)),
        LTRIM(RTRIM(@Phone)),
        LTRIM(RTRIM(@AddressLine)),
        LTRIM(RTRIM(@City)),
        LTRIM(RTRIM(@Pincode))
    );

    SET @OrderId = SCOPE_IDENTITY();

    UPDATE [dbo].[Orders]
    SET [OrderNumber] = N'SC' + RIGHT(N'000000' + CAST(@OrderId AS NVARCHAR(10)), 6)
    WHERE [OrderId] = @OrderId;

    INSERT INTO [dbo].[OrderItems]
    (
        [FK_Order],
        [FK_Product],
        [FK_ProductVariant],
        [ProductName],
        [VariantLabel],
        [SKU],
        [UnitPrice],
        [Quantity],
        [LineTotal]
    )
    SELECT
        @OrderId,
        ProductId,
        ProductVariantId,
        Name,
        Label,
        SKU,
        Price,
        Quantity,
        LineTotal
    FROM #Lines;

    DECLARE @VariantId INT;
    DECLARE @Need INT;
    DECLARE @StockId INT;
    DECLARE @Have INT;

    DECLARE line_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ProductVariantId, Quantity
        FROM #Lines;

    OPEN line_cursor;
    FETCH NEXT FROM line_cursor INTO @VariantId, @Need;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        WHILE @Need > 0
        BEGIN
            SELECT TOP (1)
                @StockId = S.ID_Stock,
                @Have = S.Quantity
            FROM [dbo].[Stock] AS S WITH (UPDLOCK, ROWLOCK)
            WHERE S.FK_ProductVariant = @VariantId
              AND ISNULL(S.Cancelled, 0) = 0
              AND S.Quantity > 0
            ORDER BY S.CreatedOn ASC, S.ID_Stock ASC;

            IF @StockId IS NULL
            BEGIN
                CLOSE line_cursor;
                DEALLOCATE line_cursor;
                ROLLBACK TRAN;
                SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Stock changed while placing the order. Please try again.' AS ResponseMsg;
                DROP TABLE #Lines;
                RETURN;
            END

            IF @Have >= @Need
            BEGIN
                UPDATE [dbo].[Stock]
                SET Quantity = Quantity - @Need
                WHERE ID_Stock = @StockId;
                SET @Need = 0;
            END
            ELSE
            BEGIN
                UPDATE [dbo].[Stock]
                SET Quantity = 0
                WHERE ID_Stock = @StockId;
                SET @Need = @Need - @Have;
            END

            SET @StockId = NULL;
            SET @Have = 0;
        END

        FETCH NEXT FROM line_cursor INTO @VariantId, @Need;
    END

    CLOSE line_cursor;
    DEALLOCATE line_cursor;

    INSERT INTO [dbo].[Payments]
    (
        [OrderId],
        [PaymentDate],
        [PaymentAmount],
        [PaymentStatus],
        [PaymentMethod],
        [Cancelled]
    )
    VALUES
    (
        @OrderId,
        GETDATE(),
        @Total,
        N'Pending',
        N'COD',
        0
    );

    INSERT INTO [dbo].[Shipping]
    (
        [OrderId],
        [ShippingAddress],
        [ShippingStatus],
        [Cancelled]
    )
    VALUES
    (
        @OrderId,
        @FullAddress,
        N'Pending',
        0
    );

    IF ISNULL(@ProductVariantId, 0) < 1
    BEGIN
        DELETE CI
        FROM [dbo].[CartItems] AS CI
        INNER JOIN [dbo].[Cart] AS C ON C.CartId = CI.CartId
        WHERE C.UserId = @UserId;
    END

    COMMIT TRAN;

    DROP TABLE #Lines;

    SELECT @OrderId AS ResponseCode, 1 AS StatusCode, N'Order placed. Pay cash on delivery.' AS ResponseMsg;
END
GO
