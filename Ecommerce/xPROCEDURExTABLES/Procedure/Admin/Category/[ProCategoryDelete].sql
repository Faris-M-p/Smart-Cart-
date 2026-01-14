USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProCategoryDelete]    Script Date: 13-01-2026 21:55:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Delete Category with Validation and Soft Deletion
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProCategoryDelete]
    @CategoryID INT,
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
    -- 1. CHECK IF CATEGORY EXISTS
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Category WHERE ID_Category = @CategoryID)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'Invalid Category ID.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 2. ALREADY CANCELLED CHECK
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM Category WHERE ID_Category = @CategoryID AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 
               'This category is already deleted.' AS ResponseMsg, 
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 3. CHECK SUBCATEGORY DEPENDENCY
    -------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM SubCategory WITH(NOLOCK) 
               WHERE FK_Category = @CategoryID AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'Cannot delete this category because active subcategories exist.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- 4. SOFT DELETE CATEGORY
    -------------------------------------------------------------------
    UPDATE Category
    SET 
        Cancelled = 1,
        CancelledOn = @UserDate,
        CancelledReason = @CancelledReason,
        CancelledBy = @EnterBy
    WHERE ID_Category = @CategoryID;

    SELECT @CategoryID AS ResponseCode,
           'Category deleted successfully.' AS ResponseMsg,
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
