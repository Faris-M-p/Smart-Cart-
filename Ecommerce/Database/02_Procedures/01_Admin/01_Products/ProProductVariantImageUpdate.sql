/****** Object:  StoredProcedure [dbo].[ProProductVariantImageUpdate]    Script Date: 13-01-2026 22:04:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantImageUpdate
Created By       : Muhammed Faris
Created On       : 10/12/2025

====================  PROCEDURE PURPOSE  =====================
This procedure handles all CRUD operations for SKU-level images 
(ProductVariantImage) for SmartCart.

Each SKU (ProductVariant) can have:
    • Multiple images
    • One default display image
    • Soft delete support

====================  BUSINESS USE CASES  =====================
1) Admin uploads images for a product variant (SKU)
2) Admin updates the image (replace Base64 / URL)
3) Admin marks an image as default → all others become non-default
4) Admin deletes an image (soft delete)

====================  WHO USES THIS PROCEDURE?  =====================
✔ Admin Panel  
✔ SmartCart Backoffice API  
❌ NOT used by customers  

====================  WHERE USED?  =====================
✔ Product → Variant → Image Manager  
✔ SKU-level Media Management  
✔ Image sorting & prioritization  

====================  INPUT PARAMETERS  =====================
@UserAction  
    1 = Add Image  
    2 = Update Image  
    3 = Delete Image (soft delete)  
    4 = Set as Default  

@ImageURL  
    Base64 OR URL string  

====================  OUTPUT  =====================
ResponseCode = ImageID or -1  
ResponseMsg  = Success / Error  
StatusCode   = 1 = success, 0 = failed  

**********************************************************************/

CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantImageUpdate]
(
    @UserAction INT,                        -- 1=Add, 2=Update, 3=Delete, 4=SetDefault
    @ID_ProductVariantImage INT = 0,        -- For update/delete/default-set
    @FK_ProductVariant INT = 0,             -- Only for insert
    @ImageURL NVARCHAR(MAX) = NULL,         -- Base64 or URL
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

    -------------------------------------------------------------------
    -- VALIDATION: SKU must exist
    -------------------------------------------------------------------
    IF (@UserAction IN (1))
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductVariant WHERE ID_ProductVariant = @FK_ProductVariant AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid FK_ProductVariant.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END

    IF (@UserAction IN (2,3,4))
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductVariantImage WHERE ID_ProductVariantImage = @ID_ProductVariantImage)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid ProductVariantImage ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END

    -------------------------------------------------------------------
    -- INSERT (ADD NEW IMAGE)
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF (ISNULL(@ImageURL, '') = '')
        BEGIN
            SELECT -1 AS ResponseCode, 'ImageURL cannot be empty.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        INSERT INTO ProductVariantImage
        (
            FK_ProductVariant, ImageURL, IsDefault,
            CreatedOn, EnterBy, Cancelled
        )
        VALUES
        (
            @FK_ProductVariant, @ImageURL, 0,
            @Now, @EnterBy, 0
        );

        SET @ID_ProductVariantImage = SCOPE_IDENTITY();

        SELECT @ID_ProductVariantImage AS ResponseCode,
               'Variant image added successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- UPDATE EXISTING IMAGE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF (ISNULL(@ImageURL, '') = '')
        BEGIN
            SELECT -1 AS ResponseCode, 'ImageURL cannot be empty for update.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE ProductVariantImage
        SET ImageURL = @ImageURL
        WHERE ID_ProductVariantImage = @ID_ProductVariantImage;

        SELECT @ID_ProductVariantImage AS ResponseCode,
               'Variant image updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- DELETE IMAGE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        UPDATE ProductVariantImage
        SET 
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy,
            IsDefault = 0
        WHERE ID_ProductVariantImage = @ID_ProductVariantImage;

        SELECT @ID_ProductVariantImage AS ResponseCode,
               'Variant image deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- SET DEFAULT IMAGE
    -------------------------------------------------------------------
    IF (@UserAction = 4)
    BEGIN
        DECLARE @SKU INT;

        SELECT @SKU = FK_ProductVariant
        FROM ProductVariantImage
        WHERE ID_ProductVariantImage = @ID_ProductVariantImage;

        -- reset others
        UPDATE ProductVariantImage
        SET IsDefault = 0
        WHERE FK_ProductVariant = @SKU
          AND Cancelled = 0;

        -- set new default
        UPDATE ProductVariantImage
        SET IsDefault = 1
        WHERE ID_ProductVariantImage = @ID_ProductVariantImage;

        SELECT @ID_ProductVariantImage AS ResponseCode,
               'Variant default image updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
END CATCH;

END;

