SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Created On  : 04/09/2026
Purpose     : Flat module + permission list for the Admin permission tree
**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProPermissionTreeSelect]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.ID_Module,
        m.ModuleName,
        m.DisplayName,
        m.DisplayOrder AS ModuleDisplayOrder,
        p.ID_Permission,
        p.PermissionName,
        p.PermissionCode,
        p.DisplayOrder AS PermissionDisplayOrder
    FROM [dbo].[Modules] m
    INNER JOIN [dbo].[Permissions] p ON p.FK_Module = m.ID_Module
    WHERE m.Cancelled = 0
      AND m.IsActive = 1
      AND p.Cancelled = 0
      AND p.IsActive = 1
    ORDER BY m.DisplayOrder ASC, p.DisplayOrder ASC, p.PermissionName ASC;
END
GO
