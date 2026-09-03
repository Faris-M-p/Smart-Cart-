/****** Object:  StoredProcedure [dbo].[ProSupplierSelectById]    Script Date: 14-01-2026 00:00:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select Supplier By ID for Admin Edit
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProSupplierSelectById]
    @ID_Supplier BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Supplier WHERE ID_Supplier = @ID_Supplier)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Supplier ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Supplier WHERE ID_Supplier = @ID_Supplier AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Supplier is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- RESULT: SUPPLIER INFO
    -------------------------------------------------------------------
    SELECT 
        S.ID_Supplier,
        S.Name AS SupplierName,
        CAST(NULL AS NVARCHAR(100)) AS ContactPerson,
        S.Phone,
        S.Email,
        CAST(NULL AS NVARCHAR(50)) AS GSTNumber,
        S.Address,
        S.CreatedAt AS CreatedOn,
        S.Cancelled,
        S.CancelledOn,
        S.CancelledReason
    FROM Supplier S
    WHERE S.ID_Supplier = @ID_Supplier;

END;

