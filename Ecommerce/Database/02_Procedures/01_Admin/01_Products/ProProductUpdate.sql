/****** Object:  StoredProcedure [dbo].[ProProductUpdate]    Script Date: 13-01-2026 22:04:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Created By  : Muhammed Faris
Purpose     : Insert / Update Product Master With Validation
------------------------------------------------------------------------*/
CREATE OR ALTER PROCEDURE [dbo].[ProProductUpdate]
    @UserAction INT,          -- 1 = Insert, 2 = Update
    @ID_Product INT = 0,
    @Name NVARCHAR(255),
    @Description NVARCHAR(MAX) = '',
    @Price DECIMAL(10,2),
    @MRP DECIMAL(10,2),
    @FK_Category INT,
    @FK_SubCategory INT,
    @FK_Brand INT = NULL,
    @Rating DECIMAL(3,1) = NULL,
    @Gender NVARCHAR(50) = NULL,
    @FK_Status INT = 1,
    @EnterBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @UserDate DATETIME = GETDATE(),
        @IsDuplicate INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- VALIDATION
    -------------------------------------------------------------------
    IF (LTRIM(RTRIM(ISNULL(@Name,''))) = '')
    BEGIN
        SELECT -1 AS ResponseCode, 'Product name is required.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    IF (@Price <= 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Price must be greater than zero.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    IF (@MRP < @Price)
    BEGIN
        SELECT -1 AS ResponseCode, 'MRP must be >= Price.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM Category WHERE ID_Category = @FK_Category AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid Category.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM SubCategory WHERE ID_SubCategory = @FK_SubCategory AND Cancelled = 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Invalid SubCategory.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- DUPLICATE CHECK: Same Name in same Category/SubCategory
    -------------------------------------------------------------------
    SELECT @IsDuplicate = COUNT(*)
    FROM Product WITH(NOLOCK)
    WHERE Name = @Name
      AND FK_Category = @FK_Category
      AND FK_SubCategory = @FK_SubCategory
      AND ID_Product <> @ID_Product
      AND Cancelled = 0;

    IF (@IsDuplicate > 0)
    BEGIN
        SELECT -1 AS ResponseCode, 'Duplicate product exists.' AS ResponseMsg, 0 AS StatusCode;
        ROLLBACK TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- INSERT
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        INSERT INTO Product
        (
            Name, Description, Price, MRP,
            FK_Category, FK_SubCategory, FK_Brand, Rating, Gender, FK_Status,
            CreatedOn, EnterBy,
            Cancelled, CancelledOn, CancelledReason, CancelledBy
        )
        VALUES
        (
            @Name, @Description, @Price, @MRP,
            @FK_Category, @FK_SubCategory, @FK_Brand, @Rating, @Gender, @FK_Status,
            @UserDate, @EnterBy,
            0, NULL, NULL, NULL
        );

        SET @ID_Product = SCOPE_IDENTITY();

        SELECT @ID_Product AS ResponseCode, 'Product created successfully.' AS ResponseMsg, 1 AS StatusCode;
        COMMIT TRANSACTION; RETURN;
    END;

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid Product ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        IF EXISTS (SELECT 1 FROM Product WHERE ID_Product = @ID_Product AND Cancelled = 1)
        BEGIN
            SELECT -1 AS ResponseCode, 'Product is deleted.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END;

        UPDATE Product
        SET 
            Name = @Name,
            Description = @Description,
            Price = @Price,
            MRP = @MRP,
            FK_Category = @FK_Category,
            FK_SubCategory = @FK_SubCategory,
            FK_Brand = @FK_Brand,
            Rating = @Rating,
            Gender = @Gender,
            FK_Status = @FK_Status,
            UpdatedOn = @UserDate
        WHERE ID_Product = @ID_Product;

        SELECT @ID_Product AS ResponseCode, 'Product updated successfully.' AS ResponseMsg, 1 AS StatusCode;
        COMMIT TRANSACTION; RETURN;
    END;

END TRY
BEGIN CATCH
    SELECT -1 AS ResponseCode, ERROR_MESSAGE() AS ResponseMsg, 0 AS StatusCode;
    ROLLBACK TRANSACTION;
END CATCH;

END;

