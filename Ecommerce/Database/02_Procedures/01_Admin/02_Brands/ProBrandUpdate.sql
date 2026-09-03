/****** Object:  StoredProcedure [dbo].[ProBrandUpdate]    Script Date: 14-01-2026 00:00:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 14/01/2026
Purpose     : To Insert / Update Brand Master with Validation
------------------------------------------------------------------------
Modification
On          By                  TaskID/Remarks
------------------------------------------------------------------------
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProBrandUpdate]
    @UserAction INT,                  -- 1 = Add, 2 = Edit
    @BrandID INT = 0,
    @BrandName NVARCHAR(100),
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
    IF (LTRIM(RTRIM(ISNULL(@BrandName, ''))) = '')
    BEGIN
        SELECT -1 AS ResponseCode, 'Please enter brand name.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (Only against active brands)
    -------------------------------------------------------------------
    SELECT @IsDuplicate = COUNT(*)
    FROM Brands WITH(NOLOCK)
    WHERE BrandName = @BrandName
      AND BrandId <> @BrandID
      AND Cancelled = 0;

    IF (@IsDuplicate > 0)
    BEGIN
        SELECT -1 AS ResponseCode,
               'Brand name "' + @BrandName + '" already exists.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END


    -------------------------------------------------------------------
    -- 1. ADD NEW BRAND
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO Brands
        (
            BrandName,
            Cancelled,
            CancelledOn,
            CancelledReason
        )
        VALUES
        (
            @BrandName,
            0,
            NULL,
            NULL
        );

        SET @BrandID = SCOPE_IDENTITY();

        SELECT @BrandID AS ResponseCode,
               'Brand created successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END


    -------------------------------------------------------------------
    -- 2. UPDATE EXISTING BRAND
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN

        -------------------------------------------------------------------
        -- BASIC EDIT VALIDATION
        -------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Brand ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        IF EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID AND Cancelled = 1)
        BEGIN
            SELECT -1 AS ResponseCode,
                   'This brand is deleted and cannot be edited.' AS ResponseMsg,
                   0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -------------------------------------------------------------------
        -- UPDATE OPERATION
        -------------------------------------------------------------------
        UPDATE Brands
        SET 
            BrandName = @BrandName
        WHERE BrandId = @BrandID;

        SELECT @BrandID AS ResponseCode,
               'Brand updated successfully.' AS ResponseMsg,
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

