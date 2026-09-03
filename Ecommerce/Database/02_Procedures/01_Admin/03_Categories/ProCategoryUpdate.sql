/****** Object:  StoredProcedure [dbo].[ProCategoryUpdate]    Script Date: 13-01-2026 21:57:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 07/12/2025
Purpose     : To Insert / Update Category Master with Validation
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProCategoryUpdate]
    @UserAction INT,                  -- 1 = Add, 2 = Edit
    @CategoryID INT = 0,
    @CategoryName NVARCHAR(255),
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
    IF (LTRIM(RTRIM(ISNULL(@CategoryName, ''))) = '')
    BEGIN
        SELECT -1 AS ResponseCode, 'Please enter category name.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (Only against active categories)
    -------------------------------------------------------------------
    SELECT @IsDuplicate = COUNT(*)
    FROM Category WITH(NOLOCK)
    WHERE Name = @CategoryName
      AND ID_Category <> @CategoryID
      AND Cancelled = 0;

    IF (@IsDuplicate > 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'Category name "' + @CategoryName + '" already exists.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END


    -------------------------------------------------------------------
    -- 1. ADD NEW CATEGORY
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO Category
        (
            Name,
            Description,
            IsActive,
            Cancelled,
            CancelledOn,
            CancelledReason
        )
        VALUES
        (
            @CategoryName,
            @Description,
            1,
            0,
            NULL,
            NULL
        );

        SET @CategoryID = SCOPE_IDENTITY();

        SELECT @CategoryID AS ResponseCode,
               'Category created successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END


    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING CATEGORY
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN

        -------------------------------------------------------------------
        -- BASIC EDIT VALIDATION
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM Category WHERE ID_Category = @CategoryID)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Category ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        IF EXISTS (SELECT 1 FROM Category WHERE ID_Category = @CategoryID AND Cancelled = 1)
        BEGIN
            SELECT -1 AS ResponseCode,
                   'This category is deleted and cannot be edited.' AS ResponseMsg,
                   0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -------------------------------------------------------------------
        -- UPDATE OPERATION
        -------------------------------------------------------------------
        UPDATE Category
        SET 
            Name = @CategoryName,
            Description = @Description
        WHERE ID_Category = @CategoryID;

        SELECT @CategoryID AS ResponseCode,
               'Category updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

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

