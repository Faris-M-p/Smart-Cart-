/****** Object:  StoredProcedure [dbo].[ProBrandSelectById]    Script Date: 14-01-2026 00:00:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select Brand By ID for Admin Edit
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProBrandSelectById]
    @BrandID INT
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Brand ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Brands WHERE BrandId = @BrandID AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Brand is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- RESULT: BRAND INFO
    -------------------------------------------------------------------
    SELECT 
        B.BrandId AS BrandID,
        B.BrandName,
        B.Cancelled,
        B.CancelledOn,
        B.CancelledReason
    FROM Brands B
    WHERE B.BrandId = @BrandID;

END;

