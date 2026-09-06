SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProEmployeeSelectById
Created By       : Muhammed Faris
Created On       : 06/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProEmployeeSelectById]
(
    @ID_AdminUser INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.ID_AdminUser AS EmployeeID,
        e.FullName AS EmployeeName,
        e.UserName,
        e.FK_UserRole,
        r.RoleName AS UserRoleName,
        e.IsActive,
        e.CreatedAt
    FROM [dbo].[AdminUsers] e
    INNER JOIN [dbo].[UserRoles] r ON r.ID_UserRole = e.FK_UserRole
    WHERE e.ID_AdminUser = @ID_AdminUser
      AND e.Cancelled = 0;
END
GO
