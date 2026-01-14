USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProVariantUpdate]    Script Date: 13-01-2026 22:07:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Purpose     :  Insert / Update / Delete Variant Master
-----------------------------------------------------------------------
@UserAction = 1 → INSERT
@UserAction = 2 → UPDATE
@UserAction = 3 → DELETE (Soft Delete)
**********************************************************************/
ALTER   PROCEDURE [dbo].[ProVariantUpdate]
(
    @UserAction INT,                          -- 1=Insert, 2=Update, 3=Delete
    @ID_Variant INT = 0,                      -- For update/delete
    @VariantName NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @DisplayOrder INT = 1,
    @EnterBy INT,
    @CancelledReason NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Now DATETIME = GETDATE(),
        @ResponseCode INT,
        @ResponseMsg NVARCHAR(500),
        @StatusCode BIT;

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF (ISNULL(@VariantName, '') = '')
        BEGIN
            SELECT -1 AS ResponseCode, 'VariantName is required.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        -- Duplicate Check
        IF EXISTS (SELECT 1 FROM Variant WHERE VariantName = @VariantName AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Variant already exists.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        INSERT INTO Variant
        (
            VariantName, Description, DisplayOrder,
            CreatedOn, EnterBy,
            Cancelled, CancelledOn, CancelledReason, CancelledBy
        )
        VALUES
        (
            @VariantName, @Description, @DisplayOrder,
            @Now, @EnterBy,
            0, NULL, NULL, NULL
        );

        SET @ID_Variant = SCOPE_IDENTITY();

        SELECT @ID_Variant AS ResponseCode,
               'Variant created successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Variant WHERE ID_Variant = @ID_Variant AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Variant ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        UPDATE Variant
        SET 
            VariantName = @VariantName,
            Description = @Description,
            DisplayOrder = @DisplayOrder
        WHERE ID_Variant = @ID_Variant;

        SELECT @ID_Variant AS ResponseCode,
               'Variant updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- DELETE (SOFT DELETE)
    -------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Variant WHERE ID_Variant = @ID_Variant)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Variant ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        UPDATE Variant
        SET 
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy
        WHERE ID_Variant = @ID_Variant;

        SELECT @ID_Variant AS ResponseCode,
               'Variant deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION;
        RETURN;
    END;

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    SELECT -1 AS ResponseCode,
           ERROR_MESSAGE() AS ResponseMsg,
           0 AS StatusCode;
END CATCH;

END;
