/****** Object:  StoredProcedure [dbo].[ProProductDelete]    Script Date: 13-01-2026 22:02:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[ProProductDelete]
    @ID_Product INT,
    @CancelledReason NVARCHAR(500),
    @EnterBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserDate DATETIME = GETDATE();

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Product ID.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product AND Cancelled = 1)
    BEGIN
        SELECT -1 AS ResponseCode, 'Product already deleted.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    UPDATE Product
    SET Cancelled = 1,
        CancelledOn = @UserDate,
        CancelledReason = @CancelledReason,
        CancelledBy = @EnterBy
    WHERE ID_Product = @ID_Product;

    SELECT @ID_Product AS ResponseCode, 'Product deleted successfully.' AS ResponseMsg, 1 AS StatusCode;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
    ROLLBACK TRANSACTION;
END CATCH;

END;

