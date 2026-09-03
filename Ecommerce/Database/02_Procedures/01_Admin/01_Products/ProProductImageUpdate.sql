/****** Object:  StoredProcedure [dbo].[ProProductImageUpdate]    Script Date: 13-01-2026 22:03:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 09/12/2025
Purpose     : Insert / Update / Delete Product Image
------------------------------------------------------------------------
@UserAction = 1 → Add
@UserAction = 2 → Update
@UserAction = 3 → Delete (soft delete)
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProProductImageUpdate]
(
    @UserAction INT,                         -- 1 = Add, 2 = Update, 3 = Delete
    @ID_ProductImage INT = 0,                -- For update/delete
    @FK_Product INT = 0,                     -- For add
    @Image NVARCHAR(MAX) = NULL,             -- Base64 or URL
    @CancelledReason NVARCHAR(500) = NULL,
    @EnterBy INT
)
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
    -- 1. ADD NEW IMAGE
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF (@FK_Product = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Product ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        IF (ISNULL(@Image, '') = '')
        BEGIN
            SELECT -1 AS ResponseCode, 'Image cannot be empty.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        INSERT INTO ProductImage
        (
            FK_Product,
            Image,
            CreatedOn,
            EnterBy,
            Cancelled,
            CancelledOn,
            CancelledReason,
            CancelledBy
        )
        VALUES
        (
            @FK_Product,
            @Image,
            @UserDate,
            @EnterBy,
            0,          -- Active
            NULL,
            NULL,
            NULL
        );

        SET @ID_ProductImage = SCOPE_IDENTITY();

        SELECT @ID_ProductImage AS ResponseCode,
               'Product image added successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
    END


    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING IMAGE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductImage WHERE ID_ProductImage = @ID_ProductImage AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid or deleted Product Image ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE ProductImage
        SET Image = @Image
        WHERE ID_ProductImage = @ID_ProductImage;

        SELECT @ID_ProductImage AS ResponseCode,
               'Product image updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
    END


    -------------------------------------------------------------------
    -- 3. DELETE IMAGE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductImage WHERE ID_ProductImage = @ID_ProductImage)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Product Image ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        IF EXISTS (SELECT 1 FROM ProductImage WHERE ID_ProductImage = @ID_ProductImage AND Cancelled = 1)
        BEGIN
            SELECT -1 AS ResponseCode, 'Image already deleted.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE ProductImage
        SET 
            Cancelled = 1,
            CancelledOn = @UserDate,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy
        WHERE ID_ProductImage = @ID_ProductImage;

        SELECT @ID_ProductImage AS ResponseCode,
               'Product image deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
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

END CATCH;

END

