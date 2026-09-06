SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProEmployeeUpdate
Created By       : Muhammed Faris
Created On       : 06/09/2026

INPUT
  @PasswordHash NULL/empty = keep existing hash.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProEmployeeUpdate]
(
    @ID_AdminUser INT,
    @FullName NVARCHAR(150),
    @UserName NVARCHAR(50),
    @PasswordHash NVARCHAR(255) = NULL,
    @FK_UserRole INT,
    @IsActive BIT = 1
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
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'This employee is deleted and cannot be edited.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[AdminUsers]
        WHERE ID_AdminUser = @ID_AdminUser AND LOWER(UserName) = N'admin'
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'The system administrator cannot be modified.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[AdminUsers]
        WHERE ID_AdminUser <> @ID_AdminUser
          AND LOWER(UserName) = LOWER(LTRIM(RTRIM(@UserName)))
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Username already exists.' AS ResponseMsg;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[UserRoles]
        WHERE ID_UserRole = @FK_UserRole AND Cancelled = 0 AND IsActive = 1
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please select a valid active user role.' AS ResponseMsg;
        RETURN;
    END

    UPDATE [dbo].[AdminUsers]
    SET
        FullName = LTRIM(RTRIM(@FullName)),
        UserName = LTRIM(RTRIM(@UserName)),
        FK_UserRole = @FK_UserRole,
        IsActive = ISNULL(@IsActive, 1),
        PasswordHash = CASE
            WHEN LTRIM(RTRIM(ISNULL(@PasswordHash, N''))) = N'' THEN PasswordHash
            ELSE @PasswordHash
        END,
        UpdatedAt = GETDATE()
    WHERE ID_AdminUser = @ID_AdminUser;

    SELECT @ID_AdminUser AS ResponseCode, 1 AS StatusCode, N'Employee updated successfully.' AS ResponseMsg;
END
GO
