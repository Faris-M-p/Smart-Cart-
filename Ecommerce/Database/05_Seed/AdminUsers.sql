SET NOCOUNT ON;
GO

PRINT 'Seeding initial Admin user...';
GO

/*
    Temporary development credentials (replace after first login in a later auth task):
      UserName : admin
      Password : Admin@123

    PasswordHash is ASP.NET Core Identity V3 (PBKDF2-HMAC-SHA256, 100000 iterations).
    Verify later with Microsoft.AspNetCore.Identity.PasswordHasher<TUser>.
*/
IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[AdminUsers]
    WHERE [UserName] = N'admin'
)
BEGIN
    INSERT INTO [dbo].[AdminUsers] (
        [FK_UserRole],
        [UserName],
        [PasswordHash],
        [FullName],
        [Email],
        [PhoneNumber],
        [IsActive],
        [CreatedAt],
        [Cancelled]
    )
    SELECT
        r.ID_UserRole,
        N'admin',
        N'AQAAAAEAAYagAAAAEPGBDPr1vrIh1zlYcblDhIRjePewKQpikuv9yxb2oVFsGwoDVTS9GhHDPlcaMiPGEA==',
        N'System Administrator',
        N'admin@smartcart.local',
        NULL,
        1,
        GETDATE(),
        0
    FROM [dbo].[UserRoles] r
    WHERE r.RoleName = N'Admin'
      AND r.Cancelled = 0;
END
GO
