SET NOCOUNT ON;
GO

/**********************************************************************
  Supermarket catalog seed — LocalDB / SmartCart only

  Adds grocery-style categories, brands, products, SKUs, public image
  URLs, opening purchase, and stock. Images are remote HTTPS URLs only;
  nothing is written to disk.

  Idempotent: inserts only when a unique business key is missing.
**********************************************************************/

PRINT 'Seeding supermarket catalog (idempotent, LocalDB only)...';

/* -------------------------------------------------------------------------
   Categories
   ------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Fresh Produce')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Fresh Produce', N'Fruits and vegetables', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Dairy & Eggs')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Dairy & Eggs', N'Milk, curd, butter, cheese, and eggs', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Bakery')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Bakery', N'Bread, buns, and biscuits', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Grocery Staples')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Grocery Staples', N'Rice, flour, oil, salt, spices, and noodles', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Snacks & Beverages')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Snacks & Beverages', N'Chips, soft drinks, tea, coffee, and water', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Frozen Foods')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Frozen Foods', N'Ice cream and frozen snacks', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Household')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Household', N'Laundry and kitchen cleaning', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Baby Care')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Baby Care', N'Diapers and baby essentials', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Personal Care')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Personal Care', N'Skincare, haircare, and daily wellness products', 1, 0);

/* -------------------------------------------------------------------------
   Brands
   ------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'SmartCart Fresh')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'SmartCart Fresh', N'Store-brand fresh produce', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Amul')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Amul', N'GCMMF dairy', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Nestle')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Nestle', N'Nestle dairy and beverages', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Britannia')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Britannia', N'Britannia bakery', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Parle')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Parle', N'Parle biscuits and snacks', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'India Gate')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'India Gate', N'India Gate rice', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Aashirvaad')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Aashirvaad', N'ITC Aashirvaad atta', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Fortune')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Fortune', N'Adani Wilmar edible oil', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Tata')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Tata', N'Tata salt and tea', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Everest')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Everest', N'Everest spices', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Maggi')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Maggi', N'Nestle Maggi noodles', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Lays')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Lays', N'PepsiCo Lays chips', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Coca-Cola')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Coca-Cola', N'Coca-Cola beverages', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Nescafe')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Nescafe', N'Nestle Nescafe coffee', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Bisleri')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Bisleri', N'Bisleri packaged water', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Kwality Walls')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Kwality Walls', N'HUL ice cream', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'McCain')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'McCain', N'McCain frozen snacks', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Colgate')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Colgate', N'Colgate oral care', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Dove')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Dove', N'HUL Dove personal care', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Surf Excel')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Surf Excel', N'HUL laundry detergent', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Vim')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Vim', N'HUL dishwash', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Pampers')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Pampers', N'P&G Pampers', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Himalaya')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Himalaya', N'Himalaya Wellness personal care', 1, 0);

/* -------------------------------------------------------------------------
   Supplier
   ------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Supplier] WHERE [Email] = N'wholesale@smartcart.local')
    INSERT INTO [dbo].[Supplier] (
        [Name], [CompanyName], [Email], [Phone],
        [State], [District], [City], [Address], [Pincode],
        [Description], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        N'SmartCart Wholesale',
        N'SmartCart Wholesale Pvt Ltd',
        N'wholesale@smartcart.local',
        N'9876501122',
        N'Kerala', N'Ernakulam', N'Kochi',
        N'Warehouse 2, Kalamassery Industrial Estate',
        N'683104',
        N'Primary grocery wholesaler for supermarket opening stock.',
        1, GETDATE(), 0
    );

/* -------------------------------------------------------------------------
   Variant: Pack Size
   ------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Variants] WHERE [Name] = N'Pack Size')
    INSERT INTO [dbo].[Variants] ([Name], [Description], [DisplayOrder], [IsActive], [Cancelled])
    VALUES (N'Pack Size', N'Grocery pack or weight', 10, 1, 0);

DECLARE @PackSize INT = (SELECT TOP (1) [ID_Variant] FROM [dbo].[Variants] WHERE [Name] = N'Pack Size' AND ISNULL([Cancelled], 0) = 0);

IF @PackSize IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'1 kg')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'1 kg', N'1 kilogram', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'5 kg')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'5 kg', N'5 kilograms', 2, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'500 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'500 g', N'500 grams', 3, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'200 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'200 g', N'200 grams', 4, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'100 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'100 g', N'100 grams', 5, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'400 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'400 g', N'400 grams', 6, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'800 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'800 g', N'800 grams', 7, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'1 L')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'1 L', N'1 litre', 8, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'750 ml')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'750 ml', N'750 millilitres', 9, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'340 ml')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'340 ml', N'340 millilitres', 10, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'1 dozen')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'1 dozen', N'12 pieces', 11, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'12 pcs')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'12 pcs', N'Pack of 12', 12, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'20 pcs')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'20 pcs', N'Pack of 20', 13, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'52 g')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'52 g', N'52 grams', 14, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @PackSize AND [Name] = N'1 piece')
        INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled]) VALUES (@PackSize, N'1 piece', N'Single unit', 15, 0);
END;

/* -------------------------------------------------------------------------
   Subcategories
   ------------------------------------------------------------------------- */
DECLARE
    @FreshProduce INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Fresh Produce' AND [Cancelled] = 0),
    @DairyEggs INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Dairy & Eggs' AND [Cancelled] = 0),
    @Bakery INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Bakery' AND [Cancelled] = 0),
    @Staples INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Grocery Staples' AND [Cancelled] = 0),
    @Snacks INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Snacks & Beverages' AND [Cancelled] = 0),
    @Frozen INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Frozen Foods' AND [Cancelled] = 0),
    @Household INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Household' AND [Cancelled] = 0),
    @BabyCare INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Baby Care' AND [Cancelled] = 0),
    @PersonalCare INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Personal Care' AND [Cancelled] = 0);

IF @FreshProduce IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Fruits' AND [FK_Category] = @FreshProduce)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Fruits', N'Fresh fruits', @FreshProduce, 1, 0);
IF @FreshProduce IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Vegetables' AND [FK_Category] = @FreshProduce)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Vegetables', N'Fresh vegetables', @FreshProduce, 1, 0);
IF @DairyEggs IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Milk' AND [FK_Category] = @DairyEggs)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Milk', N'Packaged milk', @DairyEggs, 1, 0);
IF @DairyEggs IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Butter & Cheese' AND [FK_Category] = @DairyEggs)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Butter & Cheese', N'Butter, cheese, and spreads', @DairyEggs, 1, 0);
IF @DairyEggs IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Curd & Yogurt' AND [FK_Category] = @DairyEggs)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Curd & Yogurt', N'Dahi and yogurt', @DairyEggs, 1, 0);
IF @DairyEggs IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Eggs' AND [FK_Category] = @DairyEggs)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Eggs', N'Farm eggs', @DairyEggs, 1, 0);
IF @Bakery IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Bread' AND [FK_Category] = @Bakery)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Bread', N'Fresh bread', @Bakery, 1, 0);
IF @Bakery IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Biscuits' AND [FK_Category] = @Bakery)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Biscuits', N'Packed biscuits', @Bakery, 1, 0);
IF @Staples IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Rice & Grains' AND [FK_Category] = @Staples)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Rice & Grains', N'Rice and grains', @Staples, 1, 0);
IF @Staples IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Flour & Sugar' AND [FK_Category] = @Staples)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Flour & Sugar', N'Atta, maida, and sugar', @Staples, 1, 0);
IF @Staples IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Oils' AND [FK_Category] = @Staples)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Oils', N'Cooking oils', @Staples, 1, 0);
IF @Staples IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Spices' AND [FK_Category] = @Staples)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Spices', N'Masala and spices', @Staples, 1, 0);
IF @Staples IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Noodles' AND [FK_Category] = @Staples)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Noodles', N'Instant noodles', @Staples, 1, 0);
IF @Snacks IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Chips' AND [FK_Category] = @Snacks)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Chips', N'Potato chips', @Snacks, 1, 0);
IF @Snacks IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Soft Drinks' AND [FK_Category] = @Snacks)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Soft Drinks', N'Carbonated drinks', @Snacks, 1, 0);
IF @Snacks IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Tea & Coffee' AND [FK_Category] = @Snacks)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Tea & Coffee', N'Tea and coffee', @Snacks, 1, 0);
IF @Snacks IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Water' AND [FK_Category] = @Snacks)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Water', N'Packaged drinking water', @Snacks, 1, 0);
IF @Frozen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Ice Cream' AND [FK_Category] = @Frozen)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Ice Cream', N'Ice cream and cones', @Frozen, 1, 0);
IF @Frozen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Frozen Snacks' AND [FK_Category] = @Frozen)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Frozen Snacks', N'Ready-to-cook frozen food', @Frozen, 1, 0);
IF @PersonalCare IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Oral Care' AND [FK_Category] = @PersonalCare)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Oral Care', N'Toothpaste and oral care', @PersonalCare, 1, 0);
IF @PersonalCare IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Hair Care' AND [FK_Category] = @PersonalCare)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Hair Care', N'Shampoo and hair care', @PersonalCare, 1, 0);
IF @Household IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Laundry' AND [FK_Category] = @Household)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Laundry', N'Detergent and fabric care', @Household, 1, 0);
IF @Household IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Cleaning' AND [FK_Category] = @Household)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Cleaning', N'Dishwash and surface cleaners', @Household, 1, 0);
IF @BabyCare IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Diapers' AND [FK_Category] = @BabyCare)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled]) VALUES (N'Diapers', N'Baby diapers', @BabyCare, 1, 0);

/* -------------------------------------------------------------------------
   Catalog rows
   ------------------------------------------------------------------------- */
DECLARE @Catalog TABLE (
    SubCategory NVARCHAR(100),
    CategoryName NVARCHAR(100),
    BrandName NVARCHAR(100),
    ProductName NVARCHAR(255),
    Slug NVARCHAR(255),
    ProductDesc NVARCHAR(MAX),
    SKU NVARCHAR(100),
    Barcode NVARCHAR(100),
    PackSize NVARCHAR(100),
    MRP DECIMAL(10,2),
    SellingPrice DECIMAL(10,2),
    Cost DECIMAL(10,2),
    Uom NVARCHAR(20),
    UnitValue DECIMAL(10,3),
    StockQty INT,
    ImageUrl NVARCHAR(500)
);

INSERT INTO @Catalog
VALUES
(N'Fruits', N'Fresh Produce', N'SmartCart Fresh', N'Shimla Apple', N'shimla-apple', N'Crisp red apples, sold loose by weight.', N'SM-APPLE-1KG', N'8902000000011', N'1 kg', 180.00, 149.00, 110.00, N'kg', 1, 48, N'https://images.unsplash.com/photo-1560806887-1e4cd0b21054?auto=format&fit=crop&w=800&q=80'),
(N'Fruits', N'Fresh Produce', N'SmartCart Fresh', N'Robusta Banana', N'robusta-banana', N'Everyday robusta bananas, one dozen.', N'SM-BANANA-12', N'8902000000028', N'1 dozen', 70.00, 54.00, 38.00, N'dozen', 1, 60, N'https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?auto=format&fit=crop&w=800&q=80'),
(N'Vegetables', N'Fresh Produce', N'SmartCart Fresh', N'Farm Fresh Tomato', N'farm-fresh-tomato', N'Ripe cooking tomatoes.', N'SM-TOMATO-1KG', N'8902000000035', N'1 kg', 50.00, 36.00, 24.00, N'kg', 1, 72, N'https://images.unsplash.com/photo-1546470427-e26264be0f40?auto=format&fit=crop&w=800&q=80'),
(N'Vegetables', N'Fresh Produce', N'SmartCart Fresh', N'Farm Fresh Onion', N'farm-fresh-onion', N'Nashik red onions.', N'SM-ONION-1KG', N'8902000000042', N'1 kg', 45.00, 32.00, 22.00, N'kg', 1, 80, N'https://images.unsplash.com/photo-1508747703725-719777637510?auto=format&fit=crop&w=800&q=80'),
(N'Vegetables', N'Fresh Produce', N'SmartCart Fresh', N'Farm Fresh Potato', N'farm-fresh-potato', N'Table potatoes for daily cooking.', N'SM-POTATO-1KG', N'8902000000059', N'1 kg', 40.00, 28.00, 18.00, N'kg', 1, 90, N'https://images.unsplash.com/photo-1518977676601-b53f82aba655?auto=format&fit=crop&w=800&q=80'),
(N'Milk', N'Dairy & Eggs', N'Amul', N'Amul Taaza Toned Milk', N'amul-taaza-toned-milk', N'Toned milk, tetra pack, 1 litre.', N'SM-AMUL-MILK-1L', N'8902000000066', N'1 L', 74.00, 68.00, 58.00, N'L', 1, 120, N'https://images.unsplash.com/photo-1563636619-e9143da7973b?auto=format&fit=crop&w=800&q=80'),
(N'Butter & Cheese', N'Dairy & Eggs', N'Amul', N'Amul Butter', N'amul-butter', N'Salted table butter.', N'SM-AMUL-BUTTER-500', N'8902000000073', N'500 g', 295.00, 275.00, 230.00, N'g', 500, 36, N'https://images.unsplash.com/photo-1589985270826-4b7bb135bc9d?auto=format&fit=crop&w=800&q=80'),
(N'Curd & Yogurt', N'Dairy & Eggs', N'Nestle', N'Nestle A+ Dahi', N'nestle-a-plus-dahi', N'Thick set curd, 400 g cup.', N'SM-NESTLE-DAHI-400', N'8902000000080', N'400 g', 55.00, 48.00, 36.00, N'g', 400, 40, N'https://images.unsplash.com/photo-1488477181946-6428a0291777?auto=format&fit=crop&w=800&q=80'),
(N'Eggs', N'Dairy & Eggs', N'SmartCart Fresh', N'Farm Fresh Eggs', N'farm-fresh-eggs', N'White eggs, tray of 12.', N'SM-EGGS-12', N'8902000000097', N'12 pcs', 96.00, 84.00, 66.00, N'pcs', 12, 55, N'https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?auto=format&fit=crop&w=800&q=80'),
(N'Bread', N'Bakery', N'Britannia', N'Britannia Whole Wheat Bread', N'britannia-whole-wheat-bread', N'Soft whole wheat sandwich bread.', N'SM-BRIT-BREAD-400', N'8902000000103', N'400 g', 55.00, 48.00, 34.00, N'g', 400, 28, N'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=800&q=80'),
(N'Biscuits', N'Bakery', N'Parle', N'Parle-G Original Glucose', N'parle-g-original', N'Classic glucose biscuits, family pack.', N'SM-PARLE-G-800', N'8902000000110', N'800 g', 90.00, 80.00, 62.00, N'g', 800, 44, N'https://images.unsplash.com/photo-1558961363-fa8fdf82db35?auto=format&fit=crop&w=800&q=80'),
(N'Rice & Grains', N'Grocery Staples', N'India Gate', N'India Gate Basmati Rice', N'india-gate-basmati-rice', N'Aged basmati rice for daily cooking.', N'SM-IG-RICE-1KG', N'8902000000127', N'1 kg', 185.00, 159.00, 128.00, N'kg', 1, 32, N'https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=800&q=80'),
(N'Rice & Grains', N'Grocery Staples', N'India Gate', N'India Gate Basmati Rice', N'india-gate-basmati-rice', N'Aged basmati rice for daily cooking.', N'SM-IG-RICE-5KG', N'8902000000134', N'5 kg', 890.00, 749.00, 610.00, N'kg', 5, 18, N'https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=800&q=80'),
(N'Flour & Sugar', N'Grocery Staples', N'Aashirvaad', N'Aashirvaad Superior MP Atta', N'aashirvaad-atta', N'Whole wheat atta for rotis.', N'SM-AASH-ATTA-5KG', N'8902000000141', N'5 kg', 310.00, 275.00, 230.00, N'kg', 5, 24, N'https://images.unsplash.com/photo-1574323347407-f5e1ad6d020b?auto=format&fit=crop&w=800&q=80'),
(N'Oils', N'Grocery Staples', N'Fortune', N'Fortune Sunflower Oil', N'fortune-sunflower-oil', N'Refined sunflower oil, 1 litre.', N'SM-FORT-OIL-1L', N'8902000000158', N'1 L', 175.00, 152.00, 128.00, N'L', 1, 30, N'https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?auto=format&fit=crop&w=800&q=80'),
(N'Flour & Sugar', N'Grocery Staples', N'Tata', N'Tata Salt', N'tata-salt', N'Iodised vacuum-evaporated salt.', N'SM-TATA-SALT-1KG', N'8902000000165', N'1 kg', 32.00, 28.00, 20.00, N'kg', 1, 70, N'https://images.unsplash.com/photo-1518110925495-5fe2ade03c8b?auto=format&fit=crop&w=800&q=80'),
(N'Spices', N'Grocery Staples', N'Everest', N'Everest Turmeric Powder', N'everest-turmeric', N'Pure haldi powder.', N'SM-EVE-HALDI-200', N'8902000000172', N'200 g', 72.00, 62.00, 48.00, N'g', 200, 38, N'https://images.unsplash.com/photo-1615485290382-441e4d049cbd?auto=format&fit=crop&w=800&q=80'),
(N'Noodles', N'Grocery Staples', N'Maggi', N'Maggi 2-Minute Noodles', N'maggi-2-minute-noodles', N'Masala instant noodles, pack of 12.', N'SM-MAGGI-12', N'8902000000189', N'12 pcs', 168.00, 144.00, 118.00, N'pcs', 12, 50, N'https://images.unsplash.com/photo-1612929633738-8fe44f4ec141?auto=format&fit=crop&w=800&q=80'),
(N'Chips', N'Snacks & Beverages', N'Lays', N'Lays Classic Salted', N'lays-classic-salted', N'Classic salted potato chips.', N'SM-LAYS-52', N'8902000000196', N'52 g', 20.00, 18.00, 13.00, N'g', 52, 96, N'https://images.unsplash.com/photo-1566478989037-eec170784d0b?auto=format&fit=crop&w=800&q=80'),
(N'Soft Drinks', N'Snacks & Beverages', N'Coca-Cola', N'Coca-Cola', N'coca-cola-750ml', N'Chilled cola, 750 ml PET.', N'SM-COKE-750', N'8902000000202', N'750 ml', 45.00, 40.00, 30.00, N'ml', 750, 64, N'https://images.unsplash.com/photo-1629203851122-3726ecdf080e?auto=format&fit=crop&w=800&q=80'),
(N'Tea & Coffee', N'Snacks & Beverages', N'Tata', N'Tata Tea Gold', N'tata-tea-gold', N'Rich leaf tea, 500 g.', N'SM-TATA-TEA-500', N'8902000000219', N'500 g', 310.00, 279.00, 230.00, N'g', 500, 26, N'https://images.unsplash.com/photo-1564890369478-c89ca4d9f3f9?auto=format&fit=crop&w=800&q=80'),
(N'Tea & Coffee', N'Snacks & Beverages', N'Nescafe', N'Nescafe Classic', N'nescafe-classic', N'Soluble coffee, glass jar.', N'SM-NESCAFE-100', N'8902000000226', N'100 g', 385.00, 345.00, 290.00, N'g', 100, 22, N'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?auto=format&fit=crop&w=800&q=80'),
(N'Water', N'Snacks & Beverages', N'Bisleri', N'Bisleri Packaged Water', N'bisleri-water-1l', N'Packaged drinking water, 1 litre.', N'SM-BISLERI-1L', N'8902000000233', N'1 L', 20.00, 18.00, 12.00, N'L', 1, 140, N'https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=800&q=80'),
(N'Ice Cream', N'Frozen Foods', N'Kwality Walls', N'Kwality Walls Cornetto', N'kwality-walls-cornetto', N'Chocolate cone ice cream.', N'SM-CORNETTO-1', N'8902000000240', N'1 piece', 50.00, 40.00, 28.00, N'pcs', 1, 8, N'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=800&q=80'),
(N'Frozen Snacks', N'Frozen Foods', N'McCain', N'McCain French Fries', N'mccain-french-fries', N'Shoestring fries, 750 g.', N'SM-MCCAIN-FRIES-750', N'8902000000257', N'800 g', 210.00, 189.00, 150.00, N'g', 750, 16, N'https://images.unsplash.com/photo-1573080496219-bb080318a755?auto=format&fit=crop&w=800&q=80'),
(N'Oral Care', N'Personal Care', N'Colgate', N'Colgate Strong Teeth', N'colgate-strong-teeth', N'Family toothpaste, 200 g.', N'SM-COLGATE-200', N'8902000000264', N'200 g', 135.00, 118.00, 92.00, N'g', 200, 34, N'https://images.unsplash.com/photo-1559591937-abc3a5d02748?auto=format&fit=crop&w=800&q=80'),
(N'Hair Care', N'Personal Care', N'Dove', N'Dove Daily Shine Shampoo', N'dove-daily-shine-shampoo', N'Daily shine shampoo, 340 ml.', N'SM-DOVE-SHAMP-340', N'8902000000271', N'340 ml', 285.00, 249.00, 198.00, N'ml', 340, 20, N'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?auto=format&fit=crop&w=800&q=80'),
(N'Laundry', N'Household', N'Surf Excel', N'Surf Excel Easy Wash', N'surf-excel-easy-wash', N'Detergent powder, 1 kg.', N'SM-SURF-1KG', N'8902000000288', N'1 kg', 168.00, 149.00, 118.00, N'kg', 1, 27, N'https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?auto=format&fit=crop&w=800&q=80'),
(N'Cleaning', N'Household', N'Vim', N'Vim Dishwash Liquid', N'vim-dishwash-liquid', N'Lemon dishwash liquid, 500 ml.', N'SM-VIM-500', N'8902000000295', N'500 g', 125.00, 109.00, 84.00, N'ml', 500, 31, N'https://images.unsplash.com/photo-1563453392212-326f5e854473?auto=format&fit=crop&w=800&q=80'),
(N'Diapers', N'Baby Care', N'Pampers', N'Pampers Baby Dry M', N'pampers-baby-dry-m', N'Mid-size baby diapers, pack of 20.', N'SM-PAMPERS-M20', N'8902000000301', N'20 pcs', 399.00, 349.00, 280.00, N'pcs', 20, 14, N'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=800&q=80');

/* -------------------------------------------------------------------------
   Products
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
SELECT
    sc.[ID_SubCategory],
    b.[ID_Brand],
    MIN(p.[ProductName]),
    p.[Slug],
    MIN(p.[ProductDesc]),
    1,
    GETDATE(),
    0
FROM @Catalog p
INNER JOIN [dbo].[Category] c ON c.[Name] = p.[CategoryName] AND c.[Cancelled] = 0
INNER JOIN [dbo].[SubCategory] sc ON sc.[Name] = p.[SubCategory] AND sc.[FK_Category] = c.[ID_Category] AND sc.[Cancelled] = 0
INNER JOIN [dbo].[Brand] b ON b.[BrandName] = p.[BrandName] AND b.[Cancelled] = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[Products] x WHERE x.[Slug] = p.[Slug]
)
GROUP BY p.[Slug], sc.[ID_SubCategory], b.[ID_Brand];

/* -------------------------------------------------------------------------
   SKUs
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ProductVariants] (
    [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
    [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
    [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
)
SELECT
    pr.[ID_Product],
    c.[SKU],
    c.[Barcode],
    c.[PackSize],
    c.[ProductName] + N', ' + c.[PackSize],
    c.[MRP],
    c.[SellingPrice],
    c.[Uom],
    c.[UnitValue],
    CASE WHEN c.[SKU] = N'SM-IG-RICE-5KG' THEN 0 ELSE 1 END,
    20,
    1,
    GETDATE(),
    0
FROM @Catalog c
INNER JOIN [dbo].[Products] pr ON pr.[Slug] = c.[Slug] AND pr.[Cancelled] = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[ProductVariants] v WHERE v.[SKU] = c.[SKU]
);

/* -------------------------------------------------------------------------
   Pack Size attributes
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
SELECT
    pv.[ID_ProductVariant],
    @PackSize,
    vv.[ID_VariantValue],
    N'Pack Size'
FROM @Catalog c
INNER JOIN [dbo].[ProductVariants] pv ON pv.[SKU] = c.[SKU] AND ISNULL(pv.[Cancelled], 0) = 0
INNER JOIN [dbo].[VariantValues] vv ON vv.[FK_Variant] = @PackSize AND vv.[Name] = c.[PackSize] AND ISNULL(vv.[Cancelled], 0) = 0
WHERE @PackSize IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[ProductVariantAttributes] a
      WHERE a.[FK_ProductVariant] = pv.[ID_ProductVariant] AND a.[FK_Variant] = @PackSize
  );

/* -------------------------------------------------------------------------
   Public image URLs (product + SKU)
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ProductMedia] ([FK_Product], [MediaType], [MediaUrl], [DisplayOrder], [IsPrimary], [CreatedAt])
SELECT
    pr.[ID_Product],
    N'Image',
    MIN(c.[ImageUrl]),
    0,
    1,
    GETDATE()
FROM @Catalog c
INNER JOIN [dbo].[Products] pr ON pr.[Slug] = c.[Slug] AND pr.[Cancelled] = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[ProductMedia] m WHERE m.[FK_Product] = pr.[ID_Product]
)
GROUP BY pr.[ID_Product];

INSERT INTO [dbo].[SkuMedia] ([FK_ProductSKU], [MediaType], [MediaUrl], [DisplayOrder], [IsPrimary], [CreatedAt])
SELECT
    pv.[ID_ProductVariant],
    N'Image',
    c.[ImageUrl],
    0,
    1,
    GETDATE()
FROM @Catalog c
INNER JOIN [dbo].[ProductVariants] pv ON pv.[SKU] = c.[SKU] AND ISNULL(pv.[Cancelled], 0) = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[SkuMedia] m WHERE m.[FK_ProductSKU] = pv.[ID_ProductVariant]
);

/* Existing electronics products — public photos only if they have no media yet */
INSERT INTO [dbo].[ProductMedia] ([FK_Product], [MediaType], [MediaUrl], [DisplayOrder], [IsPrimary], [CreatedAt])
SELECT p.[ID_Product], N'Image', x.[ImageUrl], 0, 1, GETDATE()
FROM (VALUES
    (N'iphone-15', N'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    (N'samsung-galaxy-a35', N'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80'),
    (N'boat-airdopes-141', N'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=800&q=80'),
    (N'prestige-pic-20-induction-cooktop', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=800&q=80'),
    (N'himalaya-purifying-neem-face-wash-150ml', N'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=800&q=80')
) x ([Slug], [ImageUrl])
INNER JOIN [dbo].[Products] p ON p.[Slug] = x.[Slug] AND p.[Cancelled] = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[ProductMedia] m WHERE m.[FK_Product] = p.[ID_Product]
);

INSERT INTO [dbo].[SkuMedia] ([FK_ProductSKU], [MediaType], [MediaUrl], [DisplayOrder], [IsPrimary], [CreatedAt])
SELECT pv.[ID_ProductVariant], N'Image', x.[ImageUrl], 0, 1, GETDATE()
FROM (VALUES
    (N'IP15-BLK-128', N'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    (N'IP15-BLU-256', N'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    (N'SMA35-NVY-128', N'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80'),
    (N'BOAT-AD141-BLK', N'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=800&q=80'),
    (N'PRE-PIC20-2000W', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=800&q=80'),
    (N'HIM-NEEM-FW-150', N'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=800&q=80')
) x ([SKU], [ImageUrl])
INNER JOIN [dbo].[ProductVariants] pv ON pv.[SKU] = x.[SKU] AND ISNULL(pv.[Cancelled], 0) = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[SkuMedia] m WHERE m.[FK_ProductSKU] = pv.[ID_ProductVariant]
);

/* -------------------------------------------------------------------------
   Opening purchase + stock
   ------------------------------------------------------------------------- */
DECLARE
    @SupplierId INT = (SELECT TOP (1) [ID_Supplier] FROM [dbo].[Supplier] WHERE [Email] = N'wholesale@smartcart.local' AND [Cancelled] = 0),
    @PurchaseId INT;

IF @SupplierId IS NOT NULL
BEGIN
    SET @PurchaseId = (SELECT TOP (1) [ID_Purchase] FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-SMKT-26001');

    IF @PurchaseId IS NULL
    BEGIN
        INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
        SELECT
            @SupplierId,
            '2026-08-20',
            N'INV-SMKT-26001',
            ISNULL(SUM(c.[Cost] * c.[StockQty]), 0),
            N'Supermarket opening stock — grocery aisles',
            '2026-08-20T09:00:00',
            0
        FROM @Catalog c;

        SET @PurchaseId = SCOPE_IDENTITY();
    END;

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    SELECT
        @PurchaseId,
        pv.[ID_ProductVariant],
        c.[StockQty],
        c.[Cost],
        c.[MRP],
        '2026-08-20T09:00:00',
        0
    FROM @Catalog c
    INNER JOIN [dbo].[ProductVariants] pv ON pv.[SKU] = c.[SKU] AND ISNULL(pv.[Cancelled], 0) = 0
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[PurchaseDetail] pd
        WHERE pd.[FK_Purchase] = @PurchaseId AND pd.[FK_ProductVariant] = pv.[ID_ProductVariant]
    );

    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    SELECT
        pd.[ID_PurchaseDetail],
        pd.[FK_ProductVariant],
        pd.[Quantity],
        '2026-08-20T09:00:00',
        0
    FROM [dbo].[PurchaseDetail] pd
    WHERE pd.[FK_Purchase] = @PurchaseId
      AND NOT EXISTS (
          SELECT 1 FROM [dbo].[Stock] s WHERE s.[FK_PurchaseDetail] = pd.[ID_PurchaseDetail]
      );

    UPDATE p
    SET p.[TotalAmount] = x.[TotalAmount]
    FROM [dbo].[Purchase] p
    INNER JOIN (
        SELECT [FK_Purchase], SUM([PurchasePrice] * [Quantity]) AS [TotalAmount]
        FROM [dbo].[PurchaseDetail]
        WHERE [FK_Purchase] = @PurchaseId AND [Cancelled] = 0
        GROUP BY [FK_Purchase]
    ) x ON x.[FK_Purchase] = p.[ID_Purchase];
END;

PRINT 'Supermarket catalog seed complete.';
GO
