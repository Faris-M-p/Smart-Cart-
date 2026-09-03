/****** Object:  StoredProcedure [dbo].[ProProductVariantUpsert]    Script Date: 13-01-2026 22:05:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************
Stored Procedure : ProProductVariantUpsert
Created By       : Muhammed Faris
Created On       : 09/12/2025

====================  PROCEDURE PURPOSE  ====================
This procedure is responsible for managing individual Product Variant 
entries (SKUs) in the SmartCart Ecommerce System.

SmartCart supports multi-attribute products similar to:
  - Amazon
  - Shopify
  - Flipkart

Each SKU is uniquely identified by a combination of Variant + VariantValue.
Example SKU combinations:
   • Color = Black
   • Size = XL
   • Print = Spiderman
   • RAM = 8GB + Storage = 128GB + Color = Blue

====================  BUSINESS USE CASES  ====================
1) **Admin creates new product variant (SKU)**  
   Example: Product = Bag  
            Color = Black  
            Print = Spiderman  
            PriceAdjustment = +50  
            IsDefault = 0  

2) **Admin updates an existing SKU**
   - Change price adjustment  
   - Change default state  
   - Replace attributes (Color/Size/Print combinations)

3) **Admin soft deletes a SKU**
   - Marks the SKU as Cancelled, but keeps history  

====================  WHO USES THIS PROCEDURE?  ====================
✔ Admin Panel  
✔ Backend System  
❌ NOT used by normal customer (they only consume SELECT APIs)

====================  WHERE USED IN SMARTCART?  ====================
- Product Management → Variant Management section  
- SKU Creation Wizard  
- Admin’s Edit Product page  
- SKU-level Stock & Image management  

====================  INPUT PARAMETERS  ====================
@UserAction
    1 = Insert (Create SKU)
    2 = Update SKU
    3 = Delete SKU (soft delete)

@VariantAttributes (JSON)
    JSON array containing FK_Variant + FK_VariantValue pairs
    Example:
    [
        {"FK_Variant":1, "FK_VariantValue":21}, 
        {"FK_Variant":2, "FK_VariantValue":33}
    ]

====================  OUTPUT FORMAT  ====================
ResponseCode = SKU ID or -1  
ResponseMsg  = Success or error message  
StatusCode   = 1 for success, 0 for failure  

**********************************************************************/
CREATE OR ALTER PROCEDURE [dbo].[ProProductVariantUpsert]
(
    @UserAction INT,                         -- 1=Insert, 2=Update, 3=Delete (soft)
    @ID_ProductVariant INT = 0,              -- For update/delete
    @FK_Product INT = 0,                     -- Required for insert (and used for update validation)
    @PriceAdjustment DECIMAL(10,2) = 0,
    @IsDefault BIT = 0,
    @VariantAttributes NVARCHAR(MAX) = NULL, -- JSON: array of { FK_Variant, FK_VariantValue }
    @EnterBy INT = NULL,
    @CancelledReason NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @Now DATETIME = GETDATE(),
        @ResponseCode INT = 0,
        @ResponseMsg NVARCHAR(500) = '',
        @StatusCode BIT = 0;

    -- Table variable to hold parsed attributes
    DECLARE @attrs TABLE (
        FK_Variant INT NOT NULL,
        FK_VariantValue INT NOT NULL
    );

BEGIN TRY
    BEGIN TRANSACTION;

    -------------------------------------------------------------------
    -- BASIC VALIDATIONS (Product existence)
    -------------------------------------------------------------------
    IF (@UserAction IN (1,2))
    BEGIN
        IF (@UserAction = 1 AND @FK_Product = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'FK_Product is required for insert.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -- If update, validate productvariant exists & get FK_Product if not provided
        IF (@UserAction = 2)
        BEGIN
            IF (@ID_ProductVariant = 0)
            BEGIN
                SELECT -1 AS ResponseCode, 'ID_ProductVariant is required for update.' AS ResponseMsg, 0 AS StatusCode;
                ROLLBACK TRANSACTION; RETURN;
            END

            IF NOT EXISTS (SELECT 1 FROM ProductVariant pv WHERE pv.ID_ProductVariant = @ID_ProductVariant)
            BEGIN
                SELECT -1 AS ResponseCode, 'Invalid ProductVariant ID.' AS ResponseMsg, 0 AS StatusCode;
                ROLLBACK TRANSACTION; RETURN;
            END

            -- Ensure we have FK_Product (if caller didn't pass, fetch)
            IF (@FK_Product = 0)
                SELECT @FK_Product = FK_Product FROM ProductVariant WHERE ID_ProductVariant = @ID_ProductVariant;
        END

        -- Product must exist and be active
        IF NOT EXISTS (SELECT 1 FROM Product WHERE ID_Product = @FK_Product AND Cancelled = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid or deleted Product.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END

    -------------------------------------------------------------------
    -- Parse VariantAttributes JSON into @attrs if provided
    -------------------------------------------------------------------
    IF (@VariantAttributes IS NOT NULL AND LTRIM(RTRIM(@VariantAttributes)) <> '')
    BEGIN
        INSERT INTO @attrs (FK_Variant, FK_VariantValue)
        SELECT TRY_CAST([FK_Variant] AS INT), TRY_CAST([FK_VariantValue] AS INT)
        FROM OPENJSON(@VariantAttributes)
             WITH (FK_Variant INT '$.FK_Variant', FK_VariantValue INT '$.FK_VariantValue');

        -- Basic check: at least one attribute for insert/update mapping
        IF (@UserAction = 1 AND (SELECT COUNT(1) FROM @attrs) = 0)
        BEGIN
            SELECT -1 AS ResponseCode, 'At least one Variant attribute is required in VariantAttributes JSON for insert.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -- Validate each pair: variant exists and value exists and belongs to variant
        IF EXISTS (
            SELECT 1
            FROM @attrs a
            LEFT JOIN Variant v ON v.ID_Variant = a.FK_Variant
            LEFT JOIN VariantValue vv ON vv.ID_VariantValue = a.FK_VariantValue
            WHERE v.ID_Variant IS NULL OR vv.ID_VariantValue IS NULL OR vv.FK_Variant <> a.FK_Variant
        )
        BEGIN
            SELECT -1 AS ResponseCode, 'One or more Variant/Value pairs are invalid or mismatched.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END
    ELSE
    BEGIN
        -- If attributes not provided for insert, reject (changeable if you want zero-attribute SKUs)
        IF (@UserAction = 1)
        BEGIN
            SELECT -1 AS ResponseCode, 'VariantAttributes JSON is required for insert (at least one attribute).' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END
    END

    -------------------------------------------------------------------
    -- Helper: build incoming attribute signature (ordered string) to detect duplicates.
    -- Example signature: "1:21,2:33"
    -------------------------------------------------------------------
    DECLARE @IncomingSig NVARCHAR(MAX) = NULL;

    IF EXISTS (SELECT 1 FROM @attrs)
    BEGIN
        SELECT @IncomingSig = STUFF((
            SELECT ',' + CAST(FK_Variant AS VARCHAR(12)) + ':' + CAST(FK_VariantValue AS VARCHAR(12))
            FROM @attrs
            ORDER BY FK_Variant
            FOR XML PATH(''), TYPE
        ).value('.','nvarchar(max)'), 1, 1, '');
    END

    -------------------------------------------------------------------
    -- DUPLICATE CHECK (for the same product)
    -- We compare the signature with existing SKUs of the same product.
    -------------------------------------------------------------------
    DECLARE @DupExistingID INT = NULL;

    IF (@IncomingSig IS NOT NULL)
    BEGIN
        SELECT TOP 1 @DupExistingID = pv.ID_ProductVariant
        FROM ProductVariant pv
        WHERE pv.FK_Product = @FK_Product
          AND pv.Cancelled = 0
          AND (
                -- compute signature for each existing pv
                SELECT STUFF((
                    SELECT ',' + CAST(pva.FK_Variant AS VARCHAR(12)) + ':' + CAST(pva.FK_VariantValue AS VARCHAR(12))
                    FROM ProductVariantAttribute pva
                    WHERE pva.FK_ProductVariant = pv.ID_ProductVariant
                    ORDER BY pva.FK_Variant
                    FOR XML PATH(''), TYPE
                ).value('.','nvarchar(max)'),1,1,'')
              ) = @IncomingSig;
        -- If found, @DupExistingID holds that existing SKU id
    END

    -------------------------------------------------------------------
    -- INSERT (CREATE SKU)
    -------------------------------------------------------------------
    IF (@UserAction = 1)
    BEGIN
        IF (@DupExistingID IS NOT NULL)
        BEGIN
            SELECT -1 AS ResponseCode, CONCAT('Duplicate SKU exists (ProductVariant ID: ', @DupExistingID, ').') AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -- If IsDefault = 1 reset others
        IF (@IsDefault = 1)
            UPDATE ProductVariant SET IsDefault = 0 WHERE FK_Product = @FK_Product AND Cancelled = 0;

        INSERT INTO ProductVariant
        (
            FK_Product, PriceAdjustment, IsDefault,
            CreatedOn, EnterBy, Cancelled, CancelledOn, CancelledReason, CancelledBy
        )
        VALUES
        (
            @FK_Product, @PriceAdjustment, @IsDefault,
            @Now, @EnterBy, 0, NULL, NULL, NULL
        );

        SET @ID_ProductVariant = SCOPE_IDENTITY();

        -- Insert mapping rows
        INSERT INTO ProductVariantAttribute (FK_ProductVariant, FK_Variant, FK_VariantValue)
        SELECT @ID_ProductVariant, FK_Variant, FK_VariantValue
        FROM @attrs;

        SELECT @ID_ProductVariant AS ResponseCode, 'ProductVariant created successfully.' AS ResponseMsg, 1 AS StatusCode;

        COMMIT TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- UPDATE
    -------------------------------------------------------------------
    IF (@UserAction = 2)
    BEGIN
        -- Validate exists and not cancelled
        IF NOT EXISTS (SELECT 1 FROM ProductVariant WHERE ID_ProductVariant = @ID_ProductVariant)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid ProductVariant ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -- If incoming signature matches some other existing SKU for same product -> duplicate
        IF (@IncomingSig IS NOT NULL AND @DupExistingID IS NOT NULL AND @DupExistingID <> @ID_ProductVariant)
        BEGIN
            SELECT -1 AS ResponseCode, CONCAT('Duplicate SKU exists (ProductVariant ID: ', @DupExistingID, ').') AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        -- If IsDefault = 1 reset others
        IF (@IsDefault = 1)
            UPDATE ProductVariant SET IsDefault = 0 WHERE FK_Product = @FK_Product AND Cancelled = 0;

        UPDATE ProductVariant
        SET 
            PriceAdjustment = @PriceAdjustment,
            IsDefault = @IsDefault
        WHERE ID_ProductVariant = @ID_ProductVariant;

        -- If VariantAttributes provided => replace mapping rows
        IF (@IncomingSig IS NOT NULL)
        BEGIN
            -- delete existing mappings for this SKU
            DELETE FROM ProductVariantAttribute WHERE FK_ProductVariant = @ID_ProductVariant;

            -- insert new mapping rows
            INSERT INTO ProductVariantAttribute (FK_ProductVariant, FK_Variant, FK_VariantValue)
            SELECT @ID_ProductVariant, FK_Variant, FK_VariantValue
            FROM @attrs;
        END

        SELECT @ID_ProductVariant AS ResponseCode, 'ProductVariant updated successfully.' AS ResponseMsg, 1 AS StatusCode;
        COMMIT TRANSACTION; RETURN;
    END

    -------------------------------------------------------------------
    -- DELETE (SOFT)
    -------------------------------------------------------------------
    IF (@UserAction = 3)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductVariant WHERE ID_ProductVariant = @ID_ProductVariant)
        BEGIN
            SELECT -1 AS ResponseCode, 'Invalid ProductVariant ID.' AS ResponseMsg, 0 AS StatusCode;
            ROLLBACK TRANSACTION; RETURN;
        END

        UPDATE ProductVariant
        SET Cancelled = 1, CancelledOn = @Now, CancelledReason = @CancelledReason, CancelledBy = @EnterBy
        WHERE ID_ProductVariant = @ID_ProductVariant;

        SELECT @ID_ProductVariant AS ResponseCode, 'ProductVariant deleted (soft) successfully.' AS ResponseMsg, 1 AS StatusCode;
        COMMIT TRANSACTION; RETURN;
    END

    -- Default fallback
    SELECT -1 AS ResponseCode, 'Invalid UserAction.' AS ResponseMsg, 0 AS StatusCode;
    ROLLBACK TRANSACTION; RETURN;

END TRY
BEGIN CATCH
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    ROLLBACK TRANSACTION;
    SELECT -1 AS ResponseCode, @ErrMsg AS ResponseMsg, 0 AS StatusCode;
END CATCH;
END;

