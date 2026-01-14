USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProSubCategoryUpdate]    Script Date: 13-01-2026 22:06:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Insert / Update SubCategory Master with Validation
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProSubCategoryUpdate]
    @UserAction INT,                  -- 1 = Add, 2 = Edit
    @SubCategoryID INT = 0,
    @SubCategoryName NVARCHAR(255),
    @FK_Category INT,
    @Description NVARCHAR(500) = '',
    @EnterBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @ResponseCode INT = 0,
        @ResponseMsg NVARCHAR(MAX) = '',
        @StatusCode BIT = 0,
        @UserDate DATETIME = GETDATE(),
        @IsDuplicate INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- REQUIRED FIELD VALIDATION
    -------------------------------------------------------------------
    IF (LTRIM(RTRIM(ISNULL(@SubCategoryName, ''))) = '')
    BEGIN
        SELECT -1 AS ResponseCode, 'Please enter SubCategory name.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- CATEGORY MUST EXIST
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Category WHERE ID_Category = @FK_Category AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid or deleted Category.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (Only active rows)
    -------------------------------------------------------------------
    SELECT @IsDuplicate = COUNT(*)
    FROM SubCategory WITH(NOLOCK)
    WHERE SubCategoryName = @SubCategoryName
      AND ID_SubCategory <> @SubCategoryID
      AND Cancelled = 0;

    IF (@IsDuplicate > 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'SubCategory "' + @SubCategoryName + '" already exists.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- 1. INSERT NEW SUBCATEGORY
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO SubCategory
        (
            SubCategoryName,
            FK_Category,
            Description,
            CreatedDate,
            EnterBy,
            Cancelled,
            CancelledOn,
            CancelledReason,
            CancelledBy
        )
        VALUES
        (
            @SubCategoryName,
            @FK_Category,
            @Description,
            @UserDate,
            @EnterBy,
            0,
            NULL,
            NULL,
            NULL
        );

        SET @SubCategoryID = SCOPE_IDENTITY();

        SELECT @SubCategoryID AS ResponseCode,
               'SubCategory created successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING SUBCATEGORY
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN

        -------------------------------------------------------------------
        -- BASIC EDIT VALIDATION
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM SubCategory WHERE ID_SubCategory = @SubCategoryID)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid SubCategory ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        IF EXISTS (SELECT 1 FROM SubCategory WHERE ID_SubCategory = @SubCategoryID AND Cancelled = 1)
        BEGIN
            SELECT -1 AS ResponseCode,
                   'This SubCategory is deleted and cannot be edited.' AS ResponseMsg,
                   0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        -------------------------------------------------------------------
        -- PERFORM UPDATE
        -------------------------------------------------------------------
        UPDATE SubCategory
        SET 
            SubCategoryName = @SubCategoryName,
            FK_Category     = @FK_Category,
            Description     = @Description
        WHERE ID_SubCategory = @SubCategoryID;

        SELECT @SubCategoryID AS ResponseCode,
               'SubCategory updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END;

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

END
