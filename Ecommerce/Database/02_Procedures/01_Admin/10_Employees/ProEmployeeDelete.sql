SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProEmployeeDelete
Created By       : Muhammed Faris
Created On       : 06/09/2026

PURPOSE
  Soft-delete an employee (Cancelled = 1). Does not remove the row.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProEmployeeDelete]
(
    @ID_AdminUser INT,
    @CancelledReason NVARCHAR(255) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[AdminUsers] WHERE ID_AdminUser = @ID_AdminUser)
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Employee not found.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM [dbo].[AdminUsers] WHERE ID_AdminUser = @ID_AdminUser AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This employee is already deleted.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[AdminUsers]
        WHERE ID_AdminUser = @ID_AdminUser AND LOWER(UserName) = N'admin'
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'The system administrator cannot be deleted.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[AdminUsers]
    SET
        Cancelled = 1,
        CancelledOn = GETDATE(),
        CancelledReason = @CancelledReason,
        IsActive = 0,
        UpdatedAt = GETDATE()
    WHERE ID_AdminUser = @ID_AdminUser;

    SELECT @ID_AdminUser AS ResponseCode, 1 AS StatusCode, N'Employee deleted successfully.' AS ResponseMsg;
END
GO
