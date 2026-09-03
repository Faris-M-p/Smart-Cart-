/****** Object:  StoredProcedure [dbo].[ProProductVariantStockUpsert]    Script Date: 13-01-2026 22:05:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantStockUpsert
Created By       : Muhammed Faris
Created On       : 11/12/2025

====================  PROCEDURE PURPOSE  =====================
This procedure manages STOCK ENTRIES at SKU level.

It is tightly linked with:
    → Purchases
    → PurchaseDetails
    → ProductVariant (SKU)

It is used to:
    • Add stock when purchase invoice is entered
    • Update stock quantity or price (if needed)
    • Soft delete stock in case of cancellation or correction

====================  BUSINESS USE CASES  =====================
1) Admin creates a purchase bill:
       Purchase → PurchaseDetails → Stock created automatically
2) Admin edits a purchase detail:
       Update the same stock row
3) Admin cancels a purchase detail:
       Soft delete the stock (Cancelled = 1)
4) Admin performs manual stock correction:
       Update stock quantity / expiry date

====================  WHO USES THIS PROCEDURE?  =====================
✔ Admin Panel  
✔ Inventory Team  
✔ Purchase Module  
❌ NOT used by normal customer  

====================  INPUT PARAMETERS  =====================

@UserAction  
    1 = Insert Stock  
    2 = Update Stock  
    3 = Delete Stock (Soft Delete)

@FK_PurchaseDetail  → Required for insert/update  
@FK_VariantProduct  → SKU ID  
@Quantity           → Incoming stock quantity  
@EnterBy            → User ID  
@CancelledReason    → Only for delete  

====================  OUTPUT FORMAT  =====================
ResponseCode = StockID or -1  
ResponseMsg  = Text message  
StatusCode   = 1 success / 0 fail  

**********************************************************************/

CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantStockUpsert]
(
    @UserAction INT,                    -- 1=Insert, 2=Update, 3=Delete (soft)
    @StockID INT = 0,                   -- For update/delete
    @FK_PurchaseDetail INT = 0,         -- Required
    @FK_VariantProduct INT = 0,         -- SKU ID
    @Quantity INT = 0,                  -- Stock quantity
    @EnterBy INT = NULL,
    @CancelledReason NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Now DATETIME = GETDATE(),
        @ResponseCode INT = 0,
        @ResponseMsg NVARCHAR(500) = '',
        @StatusCode BIT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    --------------------------------------------------------------------
    -- VALIDATION
    --------------------------------------------------------------------
    IF (@FK_PurchaseDetail = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid PurchaseDetail.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM PurchaseDetails WHERE PurchaseDetailID = @FK_PurchaseDetail AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'PurchaseDetail does not exist or deleted.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM ProductVariant WHERE ID_ProductVariant = @FK_VariantProduct AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid SKU (ProductVariant).' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    IF (@UserAction = 1 AND @Quantity <= 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Quantity must be greater than zero.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    --------------------------------------------------------------------
    -- PREVENT DUPLICATE STOCK ENTRY FOR SAME PURCHASE DETAIL
    --------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF EXISTS (SELECT 1 FROM Stock WHERE FK_PurchaseDetail = @FK_PurchaseDetail AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 
                   'Stock already exists for this Purchase Detail.' AS ResponseMsg, 
                   0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END

    --------------------------------------------------------------------
    -- 1) INSERT STOCK ENTRY
    --------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO Stock
        (
            FK_PurchaseDetail,
            FK_ProductVariant,
            Quantity,
            CreatedOn,
            EnterBy,
            Cancelled,
            CancelledOn,
            CancelledReason,
            CancelledBy
        )
        VALUES
        (
            @FK_PurchaseDetail,
            @FK_VariantProduct,
            @Quantity,
            @Now,
            @EnterBy,
            0,
            NULL,
            NULL,
            NULL
        );

        SET @StockID = SCOPE_IDENTITY();

        SELECT @StockID AS ResponseCode,
               'Stock added successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    --------------------------------------------------------------------
    -- 2) UPDATE STOCK ENTRY
    --------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Stock WHERE ID_Stock = @StockID AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid or deleted StockID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE Stock
        SET Quantity = @Quantity
        WHERE ID_Stock = @StockID;

        SELECT @StockID AS ResponseCode,
               'Stock updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    --------------------------------------------------------------------
    -- 3) DELETE STOCK ENTRY (SOFT DELETE)
    --------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Stock WHERE ID_Stock = @StockID)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid StockID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE Stock
        SET 
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy
        WHERE ID_Stock = @StockID;

        SELECT @StockID AS ResponseCode,
               'Stock deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
END CATCH;

END;

