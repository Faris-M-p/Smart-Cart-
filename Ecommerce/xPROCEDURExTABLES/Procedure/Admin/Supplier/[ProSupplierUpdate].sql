USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProSupplierUpdate]    Script Date: 13-01-2026 22:07:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProSupplierUpdate
Created By       : Muhammed Faris
Created On       : 12/12/2025

PURPOSE
  Insert / Update / Soft Delete Supplier.
  EXACTLY matches your current Supplier table structure:
      ID_Supplier, SupplierName, ContactPerson, Phone, Email,
      GSTNumber, Address, CreatedOn, EnterBy,
      Cancelled, CancelledOn, CancelledReason, CancelledBy

USED BY
  ✔ Admin Panel → Supplier Master (Add / Edit)
  ✔ Purchase Module → Select Supplier
  ✔ Inventory Team → Maintain Supplier List

ACTIONS
  1 → Insert
  2 → Update
  3 → Delete (soft delete)

**********************************************************************/
ALTER   PROCEDURE [dbo].[ProSupplierUpdate]
(
    @UserAction INT,                -- 1=Insert, 2=Update, 3=Delete
    @ID_Supplier BIGINT = 0,

    @SupplierName NVARCHAR(255),
    @ContactPerson NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(255) = NULL,
    @GSTNumber NVARCHAR(50) = NULL,
    @Address NVARCHAR(500) = NULL,

    @EnterBy INT,
    @CancelledReason NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Now DATETIME = GETDATE(),
        @IsDuplicate INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    --------------------------------------------------------------------
    -- VALIDATION
    --------------------------------------------------------------------
    IF LTRIM(RTRIM(ISNULL(@SupplierName,''))) = ''
    BEGIN
        SELECT -1 AS ResponseCode, 'Supplier Name is required.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END

    --------------------------------------------------------------------
    -- DUPLICATE CHECK
    --------------------------------------------------------------------
    SELECT @IsDuplicate = COUNT(*)
    FROM Supplier
    WHERE SupplierName = @SupplierName
      AND Cancelled = 0
      AND ID_Supplier <> @ID_Supplier;

    IF @IsDuplicate > 0
    BEGIN
        SELECT -1 AS ResponseCode, 'Duplicate supplier exists.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END


    --------------------------------------------------------------------
    -- INSERT
    --------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO Supplier
        (
            SupplierName, ContactPerson, Phone, Email, GSTNumber,
            Address,
            CreatedOn, EnterBy, Cancelled
        )
        VALUES
        (
            @SupplierName, @ContactPerson, @Phone, @Email, @GSTNumber,
            @Address,
            @Now, @EnterBy, 0
        );

        SET @ID_Supplier = SCOPE_IDENTITY();

        SELECT @ID_Supplier AS ResponseCode,
               'Supplier created successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END


    --------------------------------------------------------------------
    -- UPDATE
    --------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Supplier WHERE ID_Supplier = @ID_Supplier)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Supplier ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE Supplier
        SET
            SupplierName = @SupplierName,
            ContactPerson = @ContactPerson,
            Phone = @Phone,
            Email = @Email,
            GSTNumber = @GSTNumber,
            Address = @Address
        WHERE ID_Supplier = @ID_Supplier;

        SELECT @ID_Supplier AS ResponseCode,
               'Supplier updated successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END


    --------------------------------------------------------------------
    -- SOFT DELETE
    --------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Supplier WHERE ID_Supplier = @ID_Supplier)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Supplier ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE Supplier
        SET
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = @CancelledReason,
            CancelledBy = @EnterBy
        WHERE ID_Supplier = @ID_Supplier;

        SELECT @ID_Supplier AS ResponseCode,
               'Supplier deleted successfully.' AS ResponseMsg,
               1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END


    --------------------------------------------------------------------
    -- INVALID ACTION
    --------------------------------------------------------------------
    SELECT -1 AS ResponseCode, 'Invalid UserAction.' AS ResponseMsg, 0 AS StatusCode;
    ROLLBACK TRANSACTION; RETURN;

END TRY
BEGIN CATCH
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    ROLLBACK TRANSACTION;

    SELECT -1 AS ResponseCode,
           @Err AS ResponseMsg,
           0 AS StatusCode;
END CATCH;

END
