SET NOCOUNT ON;
GO

PRINT 'Seeding UserRoles...';
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[UserRoles]
    WHERE [RoleName] = N'Admin'
)
BEGIN
    INSERT INTO [dbo].[UserRoles] (
        [RoleName],
        [Description],
        [IsSystemRole],
        [IsActive],
        [CreatedAt],
        [Cancelled]
    )
    VALUES (
        N'Admin',
        N'System Administrator with complete access',
        1,
        1,
        GETDATE(),
        0
    );
END
GO
