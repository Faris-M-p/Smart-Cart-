/****** Object:  StoredProcedure [dbo].[ProVariantValueSelectById]    Script Date: 15-01-2026 00:20:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  :  Muhammed Faris
Created On  : 15/01/2026
Purpose     : Select Variant Value by ID
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProVariantValueSelectById]
    @ID_VariantValue INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM VariantValue WHERE ID_VariantValue = @ID_VariantValue)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Variant Value ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM VariantValue WHERE ID_VariantValue = @ID_VariantValue AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Variant Value is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    SELECT 
        VV.ID_VariantValue,
        VV.FK_Variant,
        VV.ValueName,
        VV.Description,
        VV.ValueIcon,
        VV.DisplayOrder,
        VV.CreatedOn,
        VV.Cancelled,
        VV.CancelledOn,
        VV.CancelledReason,
        V.VariantName
    FROM VariantValue VV WITH(NOLOCK)
    LEFT JOIN Variant V ON V.ID_Variant = VV.FK_Variant
    WHERE VV.ID_VariantValue = @ID_VariantValue;
END;

