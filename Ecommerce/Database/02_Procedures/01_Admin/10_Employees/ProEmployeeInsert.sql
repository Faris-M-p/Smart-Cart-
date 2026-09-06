SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProEmployeeInsert
Created By       : Muhammed Faris
Created On       : 06/09/2026

INPUT
  @PasswordHash must already be hashed by the application.
  Do not pass a plain-text password.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProEmployeeInsert]
(
    @FullName NVARCHAR(150),
    @UserName NVARCHAR(50),
    @PasswordHash NVARCHAR(255),
    @FK_UserRole INT,
    @IsActive BIT = 1,
    @Email NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (LTRIM(RTRIM(ISNULL(@FullName, N''))) = N'')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter employee name.' AS ResponseMsg;
        RETURN;
    END

    IF (LTRIM(RTRIM(ISNULL(@UserName, N''))) = N'')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter username.' AS ResponseMsg;
        RETURN;
    END

    IF (LTRIM(RTRIM(ISNULL(@PasswordHash, N''))) = N'')
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter password.' AS ResponseMsg;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[UserRoles]
        WHERE ID_UserRole = @FK_UserRole AND Cancelled = 0 AND IsActive = 1
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please select a valid active user role.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[AdminUsers]
        WHERE LOWER(UserName) = LOWER(LTRIM(RTRIM(@UserName)))
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Username already exists.' AS ResponseMsg;
        RETURN;
    END

    INSERT INTO [dbo].[AdminUsers] (
        [FK_UserRole], [UserName], [PasswordHash], [FullName], [Email],
        [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @FK_UserRole,
        LTRIM(RTRIM(@UserName)),
        @PasswordHash,
        LTRIM(RTRIM(@FullName)),
        ISNULL(@Email, LOWER(LTRIM(RTRIM(@UserName))) + N'@smartcart.local'),
        ISNULL(@IsActive, 1),
        GETDATE(),
        0
    );

    SELECT SCOPE_IDENTITY() AS ResponseCode, 1 AS StatusCode, N'Employee created successfully.' AS ResponseMsg;
END
GO
