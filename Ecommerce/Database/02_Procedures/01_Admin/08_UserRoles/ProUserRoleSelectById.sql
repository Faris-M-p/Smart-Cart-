SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Select one User Role and its assigned permission IDs
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProUserRoleSelectById]
    @ID_UserRole INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[UserRoles]
        WHERE ID_UserRole = @ID_UserRole
          AND Cancelled = 0
    )
    BEGIN
        SELECT -1 AS ResponseCode, 'User role not found.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END

    SELECT
        r.ID_UserRole,
        r.RoleName,
        r.Description,
        r.IsSystemRole,
        r.IsActive
    FROM [dbo].[UserRoles] r
    WHERE r.ID_UserRole = @ID_UserRole
      AND r.Cancelled = 0;

    SELECT urp.FK_Permission AS ID_Permission
    FROM [dbo].[UserRolePermissions] urp
    WHERE urp.FK_UserRole = @ID_UserRole
      AND urp.Cancelled = 0;
END
GO
