SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Soft-delete a User Role when no employees are assigned
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProUserRoleCancel]
    @ID_UserRole INT,
    @CancelledReason NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME = GETDATE();

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE ID_UserRole = @ID_UserRole)
    BEGIN
        SELECT -1 AS ResponseCode, 'User role not found.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE ID_UserRole = @ID_UserRole AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'This user role is already deleted.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE ID_UserRole = @ID_UserRole AND IsSystemRole = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'System roles cannot be deleted.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM [dbo].[AdminUsers]
        WHERE FK_UserRole = @ID_UserRole
          AND Cancelled = 0
    )
    BEGIN
        SELECT -1 AS ResponseCode,
               'Cannot delete this role because employees are assigned to it.' AS ResponseMsg,
               0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE [dbo].[UserRoles]
    SET
        Cancelled = 1,
        CancelledOn = @Now,
        CancelledReason = @CancelledReason,
        UpdatedAt = @Now
    WHERE ID_UserRole = @ID_UserRole;

    UPDATE [dbo].[UserRolePermissions]
    SET
        Cancelled = 1,
        CancelledOn = @Now,
        CancelledReason = N'Role cancelled'
    WHERE FK_UserRole = @ID_UserRole
      AND Cancelled = 0;

    SELECT @ID_UserRole AS ResponseCode, 'User role deleted successfully.' AS ResponseMsg, 1 AS StatusCode;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
END CATCH
END
GO
