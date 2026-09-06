SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[GetShopFilters]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.ID_Category AS Id,
        C.Name
    FROM Category AS C WITH (NOLOCK)
    WHERE ISNULL(C.Cancelled, 0) = 0
      AND C.IsActive = 1
    ORDER BY C.Name;

    SELECT
        SC.ID_SubCategory AS Id,
        SC.Name,
        SC.FK_Category AS CategoryId
    FROM SubCategory AS SC WITH (NOLOCK)
    WHERE ISNULL(SC.Cancelled, 0) = 0
      AND SC.IsActive = 1
    ORDER BY SC.Name;

    SELECT
        B.ID_Brand AS Id,
        B.BrandName AS Name
    FROM Brand AS B WITH (NOLOCK)
    WHERE ISNULL(B.Cancelled, 0) = 0
      AND B.IsActive = 1
    ORDER BY B.BrandName;
END;
GO
