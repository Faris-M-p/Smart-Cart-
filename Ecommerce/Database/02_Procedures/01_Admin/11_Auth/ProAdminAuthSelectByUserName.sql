SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProAdminAuthSelectByUserName
Created By       : Muhammed Faris
Created On       : 06/09/2026
Purpose          : Admin login lookup — employee + role by username
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProAdminAuthSelectByUserName]
(
    @UserName NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.ID_AdminUser,
        e.UserName,
        e.PasswordHash,
        e.FullName,
        e.IsActive,
        e.Cancelled,
        e.FK_UserRole,
        r.RoleName,
        r.IsActive AS RoleIsActive,
        r.Cancelled AS RoleCancelled
    FROM [dbo].[AdminUsers] e
    LEFT JOIN [dbo].[UserRoles] r ON r.ID_UserRole = e.FK_UserRole
    WHERE e.UserName = @UserName;
END
GO
