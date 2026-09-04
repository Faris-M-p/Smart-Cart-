SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Selected permission IDs for a User Role
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProUserRolePermissionSelect]
    @ID_UserRole INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT urp.FK_Permission AS ID_Permission
    FROM [dbo].[UserRolePermissions] urp
    INNER JOIN [dbo].[Permissions] p ON p.ID_Permission = urp.FK_Permission
    WHERE urp.FK_UserRole = @ID_UserRole
      AND urp.Cancelled = 0
      AND p.Cancelled = 0
    ORDER BY p.DisplayOrder ASC;
END
GO
