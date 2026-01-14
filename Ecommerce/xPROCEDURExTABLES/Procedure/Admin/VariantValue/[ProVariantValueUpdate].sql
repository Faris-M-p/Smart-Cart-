USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProVariantValueUpdate]    Script Date: 13-01-2026 22:08:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 08/12/2025
Purpose     : Insert / Update / Delete Variant Value
------------------------------------------------------------------------
@UserAction = 1 → INSERT
@UserAction = 2 → UPDATE
@UserAction = 3 → DELETE (Soft Delete)
------------------------------------------------------------------------*/
ALTER   PROCEDURE [dbo].[ProVariantValueUpdate]
(
    @UserAction INT,                          -- 1=Insert, 2=Update, 3=Delete
    @ID_VariantValue INT = 0,                 -- For update/delete
    @FK_Variant INT = 0,                      -- Parent Variant ID
    @ValueName NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @ValueIcon NVARCHAR(MAX) = NULL,          -- color dot, print icon
    @DisplayOrder INT = 1,
    @EnterBy INT,
    @CancelledReason NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Now DATETIME = GETDATE();

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (@UserAction IN (1,2))
    BEGIN
        IF (@FK_Variant = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Variant ID is required.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        IF (ISNULL(@ValueName,'') = '')
        BEGIN
            SELECT -1 AS ResponseCode, 'ValueName is required.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;  
    END;

    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF EXISTS (
            SELECT 1 FROM VariantValue 
            WHERE FK_Variant = @FK_Variant 
              AND ValueName = @ValueName 
              AND Cancelled = 0
        )
        BEGIN
            SELECT -1 AS ResponseCode, 'Duplicate Variant Value exists.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        INSERT INTO VariantValue
        (
            FK_Variant, ValueName, Description, image, DisplayOrder,
            CreatedOn, EnterBy,
            Cancelled, CancelledOn, CancelledReason, CancelledBy
        )
        VALUES
        (
            @FK_Variant, @ValueName, @Description, @ValueIcon, @DisplayOrder,
            @Now, @EnterBy,
            0, NULL, NULL, NULL
        );

        SET @ID_VariantValue = SCOPE_IDENTITY();

        SELECT @ID_VariantValue AS ResponseCode,
               'Variant Value added successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM VariantValue WHERE ID_VariantValue = @ID_VariantValue AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Variant Value ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        UPDATE VariantValue
        SET 
            ValueName = @ValueName,
            Description = @Description,
            image = @ValueIcon,
            DisplayOrder = @DisplayOrder
        WHERE ID_VariantValue = @ID_VariantValue;

        SELECT @ID_VariantValue AS ResponseCode,
               'Variant Value updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- DELETE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM VariantValue WHERE ID_VariantValue = @ID_VariantValue)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Variant Value ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        UPDATE VariantValue
        SET 
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy
        WHERE ID_VariantValue = @ID_VariantValue;

        SELECT @ID_VariantValue AS ResponseCode,
               'Variant Value deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END;

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    SELECT -1 AS ResponseCode,
           ERROR_MESSAGE() AS ResponseMsg,
           0 AS StatusCode;
END CATCH;

END;
