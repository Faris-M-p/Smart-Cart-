SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProEmployeeListSelect
Created By       : Muhammed Faris
Created On       : 06/09/2026

PURPOSE
  Admin Employee listing from AdminUsers with User Role name.
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProEmployeeListSelect]
(
    @SearchText NVARCHAR(255) = '',
    @PageIndex INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@PageIndex < 1) SET @PageIndex = 1;
    IF (@PageSize < 1) SET @PageSize = 10;

    DECLARE @Search NVARCHAR(255) = LOWER(LTRIM(RTRIM(ISNULL(@SearchText, N''))));

    ;WITH Filtered AS
    (
        SELECT
            e.ID_AdminUser,
            e.FullName,
            e.UserName,
            e.FK_UserRole,
            r.RoleName,
            e.IsActive,
            e.CreatedAt
        FROM [dbo].[AdminUsers] e
        INNER JOIN [dbo].[UserRoles] r ON r.ID_UserRole = e.FK_UserRole
        WHERE e.Cancelled = 0
          AND (
                @Search = N''
                OR LOWER(e.FullName) LIKE N'%' + @Search + N'%'
                OR LOWER(e.UserName) LIKE N'%' + @Search + N'%'
                OR LOWER(r.RoleName) LIKE N'%' + @Search + N'%'
              )
    )
    SELECT
        COUNT(1) OVER() AS TotalCount,
        ID_AdminUser AS EmployeeID,
        FullName AS EmployeeName,
        UserName,
        FK_UserRole,
        RoleName AS UserRoleName,
        IsActive,
        CreatedAt
    FROM Filtered
    ORDER BY ID_AdminUser DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
