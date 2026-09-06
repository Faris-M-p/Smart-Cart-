SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : RegisterUser
Created By       : Muhammed Faris
Created On       : 07/09/2026

INPUT
  @PasswordHash must already be hashed by the application.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[RegisterUser]
(
    @FullName NVARCHAR(150),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @FullName = LTRIM(RTRIM(ISNULL(@FullName, N'')));
    SET @Email = LTRIM(RTRIM(ISNULL(@Email, N'')));

    IF @FullName = N''
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter your name.' AS ResponseMsg;
        RETURN;
    END

    IF @Email = N''
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter your email.' AS ResponseMsg;
        RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@PasswordHash, N''))) = N''
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'Please enter a password.' AS ResponseMsg;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM [dbo].[Users]
        WHERE LOWER(Email) = LOWER(@Email)
          AND ISNULL(Cancelled, 0) = 0
    )
    BEGIN
        SELECT -1 AS ResponseCode, 0 AS StatusCode, N'An account with this email already exists.' AS ResponseMsg;
        RETURN;
    END

    INSERT INTO [dbo].[Users] (
        [UserName],
        [FullName],
        [PasswordHash],
        [Email],
        [IsAdmin],
        [CreatedAt],
        [Cancelled]
    )
    VALUES (
        @Email,
        @FullName,
        @PasswordHash,
        @Email,
        0,
        GETDATE(),
        0
    );

    SELECT SCOPE_IDENTITY() AS ResponseCode, 1 AS StatusCode, N'Account created.' AS ResponseMsg;
END
GO
