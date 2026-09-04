SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Insert / Update User Role and replace permission mappings
              in one transaction.
  @UserAction = 1 Insert, 2 Update
  @SelectedPermissionIds = JSON array of permission IDs, e.g. [14,15,16]
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProUserRoleSave]
    @UserAction INT,
    @ID_UserRole INT = 0,
    @RoleName NVARCHAR(100),
    @Description NVARCHAR(1000) = NULL,
    @IsActive BIT = 1,
    @SelectedPermissionIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @Now DATETIME = GETDATE(),
        @NormalizedName NVARCHAR(100),
        @PermissionCount INT = 0;

    SET @NormalizedName = LTRIM(RTRIM(ISNULL(@RoleName, N'')));

BEGIN TRY
    BEGIN TRANSACTION;

    IF (@NormalizedName = N'')
    BEGIN
        SELECT -1 AS ResponseCode, 'Role name is required.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF (@SelectedPermissionIds IS NULL OR LTRIM(RTRIM(@SelectedPermissionIds)) IN (N'', N'[]'))
    BEGIN
        SELECT -1 AS ResponseCode, 'At least one permission must be selected.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    DECLARE @PermIds TABLE (ID_Permission INT NOT NULL PRIMARY KEY);

    INSERT INTO @PermIds (ID_Permission)
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM OPENJSON(@SelectedPermissionIds)
    WHERE TRY_CAST(value AS INT) IS NOT NULL
      AND TRY_CAST(value AS INT) > 0;

    SELECT @PermissionCount = COUNT(*) FROM @PermIds;

    IF (@PermissionCount = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'At least one permission must be selected.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM @PermIds p
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dbo].[Permissions] x
            WHERE x.ID_Permission = p.ID_Permission
              AND x.Cancelled = 0
        )
    )
    BEGIN
        SELECT -1 AS ResponseCode, 'One or more selected permissions are invalid.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF (@UserAction = 1)
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM [dbo].[UserRoles]
            WHERE LOWER(RoleName) = LOWER(@NormalizedName)
              AND Cancelled = 0
        )
        BEGIN
            SELECT -1 AS ResponseCode, 'Role name already exists.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO [dbo].[UserRoles] (
            RoleName, Description, IsSystemRole, IsActive, CreatedAt, Cancelled
        )
        VALUES (
            @NormalizedName, @Description, 0, @IsActive, @Now, 0
        );

        SET @ID_UserRole = SCOPE_IDENTITY();
    END
    ELSE IF (@UserAction = 2)
    BEGIN
        IF (@ID_UserRole <= 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid user role.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE ID_UserRole = @ID_UserRole AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'User role not found.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[UserRoles] WHERE ID_UserRole = @ID_UserRole AND IsSystemRole = 1)
        BEGIN
            SELECT -1 AS ResponseCode, 'System roles cannot be modified.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM [dbo].[UserRoles]
            WHERE LOWER(RoleName) = LOWER(@NormalizedName)
              AND Cancelled = 0
              AND ID_UserRole <> @ID_UserRole
        )
        BEGIN
            SELECT -1 AS ResponseCode, 'Role name already exists.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE [dbo].[UserRoles]
        SET
            RoleName = @NormalizedName,
            Description = @Description,
            IsActive = @IsActive,
            UpdatedAt = @Now
        WHERE ID_UserRole = @ID_UserRole;

        UPDATE urp
        SET
            urp.Cancelled = 0,
            urp.CancelledOn = NULL,
            urp.CancelledReason = NULL
        FROM [dbo].[UserRolePermissions] urp
        INNER JOIN @PermIds p ON p.ID_Permission = urp.FK_Permission
        WHERE urp.FK_UserRole = @ID_UserRole;

        UPDATE [dbo].[UserRolePermissions]
        SET
            Cancelled = 1,
            CancelledOn = @Now,
            CancelledReason = N'Replaced during role update'
        WHERE FK_UserRole = @ID_UserRole
          AND Cancelled = 0
          AND FK_Permission NOT IN (SELECT ID_Permission FROM @PermIds);
    END
    ELSE
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid user action.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION;
        RETURN;
    END

    INSERT INTO [dbo].[UserRolePermissions] (
        FK_UserRole, FK_Permission, CreatedAt, Cancelled
    )
    SELECT
        @ID_UserRole,
        p.ID_Permission,
        @Now,
        0
    FROM @PermIds p
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[UserRolePermissions] urp
        WHERE urp.FK_UserRole = @ID_UserRole
          AND urp.FK_Permission = p.ID_Permission
    );

    SELECT @ID_UserRole AS ResponseCode,
           CASE WHEN @UserAction = 1 THEN 'User role created successfully.' ELSE 'User role updated successfully.' END AS ResponseMsg,
           1 AS StatusCode;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
END CATCH
END
GO
