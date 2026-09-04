SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : List active (non-cancelled) User Roles
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProUserRoleListSelect]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.ID_UserRole,
        r.RoleName,
        r.Description,
        r.IsSystemRole,
        r.IsActive
    FROM [dbo].[UserRoles] r
    WHERE r.Cancelled = 0
    ORDER BY r.IsSystemRole DESC, r.RoleName ASC;
END
GO
