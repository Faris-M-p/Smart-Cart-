/****** Object:  StoredProcedure [dbo].[ProBrandDelete]    Script Date: 14-01-2026 00:00:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Delete Brand with Validation and Soft Deletion
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProBrandDelete]
    @BrandID INT,
    @CancelledReason NVARCHAR(255) = '',
    @EnterBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @ResponseCode INT = 0,
        @ResponseMsg NVARCHAR(MAX) = '',
        @StatusCode BIT = 0,
        @UserDate DATETIME = GETDATE();

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- 1. CHECK IF BRAND EXISTS
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'Invalid Brand ID.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 2. ALREADY CANCELLED CHECK
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'This brand is already deleted.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 3. CHECK PRODUCT DEPENDENCY
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM Product WITH(NOLOCK) 
               WHERE FK_Brand = @BrandID AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'Cannot delete this brand because active products exist.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 4. SOFT DELETE BRAND
    -------------------------------------------------------------------
    UPDATE Brands
    SET 
        Cancelled = 1,
        CancelledOn = @UserDate,
        CancelledReason = @CancelledReason
    WHERE BrandId = @BrandID;

    SELECT @BrandID AS ResponseCode,
           'Brand deleted successfully.' AS ResponseMsg,
           1 AS StatusCode;

    COMMIT TRANSACTION;
    RETURN;

END TRY
BEGIN CATCH

    SET @ResponseCode = -1;
    SET @ResponseMsg = ERROR_MESSAGE();
    SET @StatusCode = 0;

    ROLLBACK TRANSACTION;

    SELECT @ResponseCode AS ResponseCode, 
           @ResponseMsg AS ResponseMsg, 
           @StatusCode AS StatusCode;

END CATCH

END

