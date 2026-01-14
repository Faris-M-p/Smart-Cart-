USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProVariantSelectById]    Script Date: 15-01-2026 00:20:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 15/01/2026
Purpose     : Select Variant by ID
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProVariantSelectById]
    @ID_Variant INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Variant WHERE ID_Variant = @ID_Variant)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Variant ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Variant WHERE ID_Variant = @ID_Variant AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Variant is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    SELECT 
        ID_Variant,
        VariantName,
        Description,
        DisplayOrder,
        CreatedOn,
        Cancelled,
        CancelledOn,
        CancelledReason
    FROM Variant
    WHERE ID_Variant = @ID_Variant;
END;
