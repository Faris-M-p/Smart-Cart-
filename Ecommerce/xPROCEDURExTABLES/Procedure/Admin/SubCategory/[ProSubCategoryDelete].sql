USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProSubCategoryDelete]    Script Date: 13-01-2026 22:06:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Delete SubCategory with Validation and Soft Deletion
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProSubCategoryDelete]
    @SubCategoryID INT,
    @CancelledReason NVARCHAR(500) = '',
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
    -- 1. CHECK IF SUBCATEGORY EXISTS
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM SubCategory WHERE ID_SubCategory = @SubCategoryID)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'Invalid SubCategory ID.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 2. ALREADY DELETED CHECK
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM SubCategory WHERE ID_SubCategory = @SubCategoryID AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'This subcategory is already deleted.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 3. CHECK PRODUCT DEPENDENCY (OPTIONAL BUT RECOMMENDED)
    -------------------------------------------------------------------
    /*
    IF EXISTS (SELECT 1 FROM Products WITH(NOLOCK)
               WHERE FK_SubCategory = @SubCategoryID AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'Cannot delete this subcategory because products are linked to it.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;
    */

    -------------------------------------------------------------------
    -- 4. SOFT DELETE SUBCATEGORY
    -------------------------------------------------------------------
    UPDATE SubCategory
    SET 
        Cancelled       = 1,
        CancelledOn     = @UserDate,
        CancelledBy     = @EnterBy,
        CancelledReason = @CancelledReason
    WHERE ID_SubCategory = @SubCategoryID;

    -------------------------------------------------------------------
    -- SUCCESS RESPONSE
    -------------------------------------------------------------------
    SELECT @SubCategoryID AS ResponseCode,
           'SubCategory deleted successfully.' AS ResponseMsg,
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
END CATCH;

END;
