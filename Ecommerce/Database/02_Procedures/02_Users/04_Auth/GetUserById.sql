SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : GetUserById
Created By       : Muhammed Faris
Created On       : 07/09/2026
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[GetUserById]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.UserId,
        ISNULL(U.FullName, U.UserName) AS FullName,
        U.Email,
        CAST(ISNULL(U.Cancelled, 0) AS BIT) AS Cancelled
    FROM [dbo].[Users] AS U
    WHERE U.UserId = @UserId
      AND ISNULL(U.Cancelled, 0) = 0;
END
GO
