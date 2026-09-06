SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProAdminAuthPermissionSelect
Created By       : Muhammed Faris
Created On       : 06/09/2026
Purpose          : Active permission codes for an Employee User Role
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProAdminAuthPermissionSelect]
(
    @ID_UserRole INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PermissionCode
    FROM [dbo].[UserRolePermissions] urp
    INNER JOIN [dbo].[Permissions] p ON p.ID_Permission = urp.FK_Permission
    WHERE urp.FK_UserRole = @ID_UserRole
      AND urp.Cancelled = 0
      AND p.Cancelled = 0
      AND p.IsActive = 1
    ORDER BY p.DisplayOrder ASC;
END
GO
