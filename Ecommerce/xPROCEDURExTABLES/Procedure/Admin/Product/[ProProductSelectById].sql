USE [SmartCart]
GO
/****** Object:  StoredProcedure [dbo].[ProProductSelectById]    Script Date: 13-01-2026 22:04:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Select Product By ID for Admin Edit
------------------------------------------------------------------------*/
ALTER PROCEDURE [dbo].[ProProductSelectById]
    @ID_Product INT
AS
BEGIN
    SET NOCOUNT ON;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Product ID.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Product is deleted.' AS ResponseMsg, 0 AS StatusCode;
        RETURN;
    END;

    -------------------------------------------------------------------
    -- RESULT: PRODUCT INFO
    -------------------------------------------------------------------
    SELECT 
        P.ID_Product,
        P.Name,
        P.Description,
        P.Price,
        P.MRP,
        P.FK_Category,
        P.FK_SubCategory,
        P.FK_Brand,
        P.Rating,
        P.Gender,
        P.FK_Status,
        P.CreatedOn,
        P.UpdatedOn,
        -- First non-cancelled image
        (SELECT TOP 1 PI.ImageData 
         FROM ProductImage PI 
         WHERE PI.FK_Product = P.ID_Product AND PI.Cancelled = 0 
         ORDER BY PI.ID_ProductImage ASC) AS ImageData,
        -- Is Base64 flag
        (SELECT TOP 1 PI.IsBase64 
         FROM ProductImage PI 
         WHERE PI.FK_Product = P.ID_Product AND PI.Cancelled = 0 
         ORDER BY PI.ID_ProductImage ASC) AS IsBase64
    FROM Product P
    WHERE P.ID_Product = @ID_Product;

END;
