SET NOCOUNT ON;
GO

PRINT 'Seeding Admin role permission mappings...';
GO

INSERT INTO [dbo].[UserRolePermissions] (
    [FK_UserRole],
    [FK_Permission],
    [CreatedAt],
    [Cancelled]
)
SELECT
    r.ID_UserRole,
    p.ID_Permission,
    GETDATE(),
    0
FROM [dbo].[UserRoles] r
CROSS JOIN [dbo].[Permissions] p
WHERE r.RoleName = N'Admin'
  AND r.Cancelled = 0
  AND p.Cancelled = 0
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[UserRolePermissions] urp
      WHERE urp.FK_UserRole = r.ID_UserRole
        AND urp.FK_Permission = p.ID_Permission
  );
GO
