SET NOCOUNT ON;
GO

/**********************************************************************
  Development sample catalog — LocalDB / SmartCart only

  Purpose
    Realistic catalog + purchase + stock rows so admin listing screens
    can be verified when those tables are empty.

  Safety
    • Idempotent: inserts only when a unique business key is missing.
    • Does not UPDATE or DELETE existing rows.
    • Do not run against production.

  Unique keys used for skip checks
    Category.Name, SubCategory (Name + FK_Category), Brand.BrandName,
    Supplier.Email, Variants.Name, VariantValues (FK_Variant + Name),
    Products.Slug, ProductVariants.SKU, Purchase.InvoiceNumber
**********************************************************************/

PRINT 'Seeding development sample catalog (idempotent, LocalDB only)...';

/* -------------------------------------------------------------------------
   Independent masters
   ------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Electronics')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Electronics', N'Smartphones, audio, and consumer electronics', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Home & Kitchen')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Home & Kitchen', N'Kitchen appliances and household essentials', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [Name] = N'Personal Care')
    INSERT INTO [dbo].[Category] ([Name], [Description], [IsActive], [Cancelled])
    VALUES (N'Personal Care', N'Skincare, haircare, and daily wellness products', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Apple')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Apple', N'Apple consumer electronics', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Samsung')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Samsung', N'Samsung mobile and consumer electronics', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'boAt')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'boAt', N'boAt audio and wearables', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Prestige')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Prestige', N'TTK Prestige kitchen appliances', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brand] WHERE [BrandName] = N'Himalaya')
    INSERT INTO [dbo].[Brand] ([BrandName], [Description], [IsActive], [Cancelled])
    VALUES (N'Himalaya', N'Himalaya Wellness personal care', 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Supplier] WHERE [Email] = N'orders@malabartech.in')
    INSERT INTO [dbo].[Supplier] (
        [Name], [CompanyName], [Email], [Phone],
        [State], [District], [City], [Address], [Pincode],
        [Description], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        N'Malabar Tech Distributors',
        N'Malabar Tech Distributors Pvt Ltd',
        N'orders@malabartech.in',
        N'9847012456',
        N'Kerala', N'Ernakulam', N'Kochi',
        N'3rd Floor, Infopark Annex, Kakkanad',
        N'682030',
        N'Authorised electronics wholesaler for smartphones and accessories.',
        1, GETDATE(), 0
    );

IF NOT EXISTS (SELECT 1 FROM [dbo].[Supplier] WHERE [Email] = N'purchase@deccanconsumer.in')
    INSERT INTO [dbo].[Supplier] (
        [Name], [CompanyName], [Email], [Phone],
        [State], [District], [City], [Address], [Pincode],
        [Description], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        N'Deccan Consumer Supplies',
        N'Deccan Consumer Supplies LLP',
        N'purchase@deccanconsumer.in',
        N'9895123780',
        N'Kerala', N'Kozhikode', N'Kozhikode',
        N'12/88, Mavoor Road, near Focus Mall',
        N'673004',
        N'Regional distributor for audio, wearables, and lifestyle electronics.',
        1, GETDATE(), 0
    );

IF NOT EXISTS (SELECT 1 FROM [dbo].[Supplier] WHERE [Email] = N'accounts@coastalhome.in')
    INSERT INTO [dbo].[Supplier] (
        [Name], [CompanyName], [Email], [Phone],
        [State], [District], [City], [Address], [Pincode],
        [Description], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        N'Coastal Home Essentials',
        N'Coastal Home Essentials Pvt Ltd',
        N'accounts@coastalhome.in',
        N'9746058123',
        N'Kerala', N'Thrissur', N'Thrissur',
        N'Warehouse 4, Kanimangalam Industrial Estate',
        N'680027',
        N'Kitchen appliances and personal-care wholesaler for Kerala retail.',
        1, GETDATE(), 0
    );

IF NOT EXISTS (SELECT 1 FROM [dbo].[Variants] WHERE [Name] = N'Color')
    INSERT INTO [dbo].[Variants] ([Name], [Description], [DisplayOrder], [IsActive], [Cancelled])
    VALUES (N'Color', N'Product colour', 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Variants] WHERE [Name] = N'Storage')
    INSERT INTO [dbo].[Variants] ([Name], [Description], [DisplayOrder], [IsActive], [Cancelled])
    VALUES (N'Storage', N'On-device storage capacity', 2, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Variants] WHERE [Name] = N'Capacity')
    INSERT INTO [dbo].[Variants] ([Name], [Description], [DisplayOrder], [IsActive], [Cancelled])
    VALUES (N'Capacity', N'Pack size or wattage', 3, 1, 0);

DECLARE
    @Electronics INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Electronics' AND [Cancelled] = 0),
    @HomeKitchen INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Home & Kitchen' AND [Cancelled] = 0),
    @PersonalCare INT = (SELECT TOP (1) [ID_Category] FROM [dbo].[Category] WHERE [Name] = N'Personal Care' AND [Cancelled] = 0),
    @Apple INT = (SELECT TOP (1) [ID_Brand] FROM [dbo].[Brand] WHERE [BrandName] = N'Apple' AND [Cancelled] = 0),
    @Samsung INT = (SELECT TOP (1) [ID_Brand] FROM [dbo].[Brand] WHERE [BrandName] = N'Samsung' AND [Cancelled] = 0),
    @Boat INT = (SELECT TOP (1) [ID_Brand] FROM [dbo].[Brand] WHERE [BrandName] = N'boAt' AND [Cancelled] = 0),
    @Prestige INT = (SELECT TOP (1) [ID_Brand] FROM [dbo].[Brand] WHERE [BrandName] = N'Prestige' AND [Cancelled] = 0),
    @Himalaya INT = (SELECT TOP (1) [ID_Brand] FROM [dbo].[Brand] WHERE [BrandName] = N'Himalaya' AND [Cancelled] = 0),
    @Malabar INT = (SELECT TOP (1) [ID_Supplier] FROM [dbo].[Supplier] WHERE [Email] = N'orders@malabartech.in' AND [Cancelled] = 0),
    @Deccan INT = (SELECT TOP (1) [ID_Supplier] FROM [dbo].[Supplier] WHERE [Email] = N'purchase@deccanconsumer.in' AND [Cancelled] = 0),
    @Coastal INT = (SELECT TOP (1) [ID_Supplier] FROM [dbo].[Supplier] WHERE [Email] = N'accounts@coastalhome.in' AND [Cancelled] = 0),
    @Color INT = (SELECT TOP (1) [ID_Variant] FROM [dbo].[Variants] WHERE [Name] = N'Color' AND ISNULL([Cancelled], 0) = 0),
    @Storage INT = (SELECT TOP (1) [ID_Variant] FROM [dbo].[Variants] WHERE [Name] = N'Storage' AND ISNULL([Cancelled], 0) = 0),
    @Capacity INT = (SELECT TOP (1) [ID_Variant] FROM [dbo].[Variants] WHERE [Name] = N'Capacity' AND ISNULL([Cancelled], 0) = 0);

IF @Electronics IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Smartphones' AND [FK_Category] = @Electronics)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled])
    VALUES (N'Smartphones', N'Mobile phones and related handsets', @Electronics, 1, 0);

IF @Electronics IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Audio' AND [FK_Category] = @Electronics)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled])
    VALUES (N'Audio', N'Earphones, earbuds, and portable speakers', @Electronics, 1, 0);

IF @HomeKitchen IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Kitchen Appliances' AND [FK_Category] = @HomeKitchen)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled])
    VALUES (N'Kitchen Appliances', N'Cooktops, mixers, and small kitchen appliances', @HomeKitchen, 1, 0);

IF @PersonalCare IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[SubCategory] WHERE [Name] = N'Skincare' AND [FK_Category] = @PersonalCare)
    INSERT INTO [dbo].[SubCategory] ([Name], [Description], [FK_Category], [IsActive], [Cancelled])
    VALUES (N'Skincare', N'Face wash, moisturisers, and daily skincare', @PersonalCare, 1, 0);

IF @Color IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Black')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Color, N'Black', N'Black finish', 1, 0);

IF @Color IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Blue')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Color, N'Blue', N'Blue finish', 2, 0);

IF @Color IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Awesome Navy')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Color, N'Awesome Navy', N'Samsung Awesome Navy', 3, 0);

IF @Storage IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Storage AND [Name] = N'128GB')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Storage, N'128GB', N'128 GB storage', 1, 0);

IF @Storage IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Storage AND [Name] = N'256GB')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Storage, N'256GB', N'256 GB storage', 2, 0);

IF @Capacity IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Capacity AND [Name] = N'2000W')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Capacity, N'2000W', N'2000 watt cooktop', 1, 0);

IF @Capacity IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Capacity AND [Name] = N'150ml')
    INSERT INTO [dbo].[VariantValues] ([FK_Variant], [Name], [Description], [DisplayOrder], [Cancelled])
    VALUES (@Capacity, N'150ml', N'150 ml pack', 2, 0);

DECLARE
    @Smartphones INT = (SELECT TOP (1) [ID_SubCategory] FROM [dbo].[SubCategory] WHERE [Name] = N'Smartphones' AND [FK_Category] = @Electronics AND [Cancelled] = 0),
    @Audio INT = (SELECT TOP (1) [ID_SubCategory] FROM [dbo].[SubCategory] WHERE [Name] = N'Audio' AND [FK_Category] = @Electronics AND [Cancelled] = 0),
    @KitchenAppliances INT = (SELECT TOP (1) [ID_SubCategory] FROM [dbo].[SubCategory] WHERE [Name] = N'Kitchen Appliances' AND [FK_Category] = @HomeKitchen AND [Cancelled] = 0),
    @Skincare INT = (SELECT TOP (1) [ID_SubCategory] FROM [dbo].[SubCategory] WHERE [Name] = N'Skincare' AND [FK_Category] = @PersonalCare AND [Cancelled] = 0),
    @ColorBlack INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Black' AND ISNULL([Cancelled], 0) = 0),
    @ColorBlue INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Blue' AND ISNULL([Cancelled], 0) = 0),
    @ColorNavy INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Color AND [Name] = N'Awesome Navy' AND ISNULL([Cancelled], 0) = 0),
    @Storage128 INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Storage AND [Name] = N'128GB' AND ISNULL([Cancelled], 0) = 0),
    @Storage256 INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Storage AND [Name] = N'256GB' AND ISNULL([Cancelled], 0) = 0),
    @Cap2000W INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Capacity AND [Name] = N'2000W' AND ISNULL([Cancelled], 0) = 0),
    @Cap150ml INT = (SELECT TOP (1) [ID_VariantValue] FROM [dbo].[VariantValues] WHERE [FK_Variant] = @Capacity AND [Name] = N'150ml' AND ISNULL([Cancelled], 0) = 0);

IF @Smartphones IS NOT NULL AND @Apple IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Slug] = N'iphone-15')
    INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
    VALUES (
        @Smartphones, @Apple, N'iPhone 15', N'iphone-15',
        N'6.1-inch Super Retina XDR display, A16 Bionic, Dual 48MP camera system, USB-C.',
        1, GETDATE(), 0
    );

IF @Smartphones IS NOT NULL AND @Samsung IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Slug] = N'samsung-galaxy-a35')
    INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
    VALUES (
        @Smartphones, @Samsung, N'Samsung Galaxy A35', N'samsung-galaxy-a35',
        N'6.6-inch Super AMOLED, 120 Hz, 50MP OIS camera, 5000 mAh battery.',
        1, GETDATE(), 0
    );

IF @Audio IS NOT NULL AND @Boat IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Slug] = N'boat-airdopes-141')
    INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
    VALUES (
        @Audio, @Boat, N'boAt Airdopes 141', N'boat-airdopes-141',
        N'True wireless earbuds with 42-hour playback, ASAP charge, and IPX4 sweat resistance.',
        1, GETDATE(), 0
    );

IF @KitchenAppliances IS NOT NULL AND @Prestige IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Slug] = N'prestige-pic-20-induction-cooktop')
    INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
    VALUES (
        @KitchenAppliances, @Prestige, N'Prestige PIC 20 Induction Cooktop', N'prestige-pic-20-induction-cooktop',
        N'2000W induction cooktop with Indian menu options, push-button controls, and automatic shut-off.',
        1, GETDATE(), 0
    );

IF @Skincare IS NOT NULL AND @Himalaya IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Slug] = N'himalaya-purifying-neem-face-wash-150ml')
    INSERT INTO [dbo].[Products] ([FK_SubCategory], [FK_Brand], [Name], [Slug], [Description], [IsActive], [CreatedAt], [Cancelled])
    VALUES (
        @Skincare, @Himalaya, N'Himalaya Purifying Neem Face Wash', N'himalaya-purifying-neem-face-wash-150ml',
        N'Neem and turmeric face wash that helps cleanse excess oil and keep skin fresh.',
        1, GETDATE(), 0
    );

DECLARE
    @Iphone15 INT = (SELECT TOP (1) [ID_Product] FROM [dbo].[Products] WHERE [Slug] = N'iphone-15' AND [Cancelled] = 0),
    @GalaxyA35 INT = (SELECT TOP (1) [ID_Product] FROM [dbo].[Products] WHERE [Slug] = N'samsung-galaxy-a35' AND [Cancelled] = 0),
    @Airdopes141 INT = (SELECT TOP (1) [ID_Product] FROM [dbo].[Products] WHERE [Slug] = N'boat-airdopes-141' AND [Cancelled] = 0),
    @Pic20 INT = (SELECT TOP (1) [ID_Product] FROM [dbo].[Products] WHERE [Slug] = N'prestige-pic-20-induction-cooktop' AND [Cancelled] = 0),
    @NeemWash INT = (SELECT TOP (1) [ID_Product] FROM [dbo].[Products] WHERE [Slug] = N'himalaya-purifying-neem-face-wash-150ml' AND [Cancelled] = 0);

IF @Iphone15 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'IP15-BLK-128')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @Iphone15, N'IP15-BLK-128', N'8901234500017', N'Black / 128GB',
        N'iPhone 15, Black, 128 GB',
        79900.00, 74900.00, N'pcs', 1, 1, 5, 1, GETDATE(), 0
    );

IF @Iphone15 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'IP15-BLU-256')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @Iphone15, N'IP15-BLU-256', N'8901234500024', N'Blue / 256GB',
        N'iPhone 15, Blue, 256 GB',
        89900.00, 84900.00, N'pcs', 1, 0, 5, 1, GETDATE(), 0
    );

IF @GalaxyA35 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'SMA35-NVY-128')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @GalaxyA35, N'SMA35-NVY-128', N'8901234500031', N'Awesome Navy / 128GB',
        N'Samsung Galaxy A35, Awesome Navy, 128 GB',
        33999.00, 30999.00, N'pcs', 1, 1, 8, 1, GETDATE(), 0
    );

IF @Airdopes141 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'BOAT-AD141-BLK')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @Airdopes141, N'BOAT-AD141-BLK', N'8901234500048', N'Black',
        N'boAt Airdopes 141, Black',
        4490.00, 1299.00, N'pcs', 1, 1, 10, 1, GETDATE(), 0
    );

IF @Pic20 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'PRE-PIC20-2000W')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @Pic20, N'PRE-PIC20-2000W', N'8901234500055', N'2000W',
        N'Prestige PIC 20, 2000W',
        3495.00, 2499.00, N'pcs', 1, 1, 6, 1, GETDATE(), 0
    );

IF @NeemWash IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariants] WHERE [SKU] = N'HIM-NEEM-FW-150')
    INSERT INTO [dbo].[ProductVariants] (
        [FK_Product], [SKU], [Barcode], [VariantLabel], [Description],
        [MRP], [SellingPrice], [UnitOfMeasure], [UnitValue],
        [IsDefault], [MaxOrderQty], [IsActive], [CreatedAt], [Cancelled]
    )
    VALUES (
        @NeemWash, N'HIM-NEEM-FW-150', N'8901234500062', N'150ml',
        N'Himalaya Purifying Neem Face Wash, 150 ml',
        170.00, 145.00, N'bottle', 150, 1, 20, 1, GETDATE(), 0
    );

DECLARE
    @SkuIphoneBlack INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'IP15-BLK-128' AND ISNULL([Cancelled], 0) = 0),
    @SkuIphoneBlue INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'IP15-BLU-256' AND ISNULL([Cancelled], 0) = 0),
    @SkuGalaxyNavy INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'SMA35-NVY-128' AND ISNULL([Cancelled], 0) = 0),
    @SkuAirdopes INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'BOAT-AD141-BLK' AND ISNULL([Cancelled], 0) = 0),
    @SkuPic20 INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'PRE-PIC20-2000W' AND ISNULL([Cancelled], 0) = 0),
    @SkuNeem INT = (SELECT TOP (1) [ID_ProductVariant] FROM [dbo].[ProductVariants] WHERE [SKU] = N'HIM-NEEM-FW-150' AND ISNULL([Cancelled], 0) = 0);

IF @SkuIphoneBlack IS NOT NULL AND @Color IS NOT NULL AND @ColorBlack IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuIphoneBlack AND [FK_Variant] = @Color)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuIphoneBlack, @Color, @ColorBlack, N'Color');

IF @SkuIphoneBlack IS NOT NULL AND @Storage IS NOT NULL AND @Storage128 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuIphoneBlack AND [FK_Variant] = @Storage)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuIphoneBlack, @Storage, @Storage128, N'Storage');

IF @SkuIphoneBlue IS NOT NULL AND @Color IS NOT NULL AND @ColorBlue IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuIphoneBlue AND [FK_Variant] = @Color)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuIphoneBlue, @Color, @ColorBlue, N'Color');

IF @SkuIphoneBlue IS NOT NULL AND @Storage IS NOT NULL AND @Storage256 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuIphoneBlue AND [FK_Variant] = @Storage)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuIphoneBlue, @Storage, @Storage256, N'Storage');

IF @SkuGalaxyNavy IS NOT NULL AND @Color IS NOT NULL AND @ColorNavy IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuGalaxyNavy AND [FK_Variant] = @Color)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuGalaxyNavy, @Color, @ColorNavy, N'Color');

IF @SkuGalaxyNavy IS NOT NULL AND @Storage IS NOT NULL AND @Storage128 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuGalaxyNavy AND [FK_Variant] = @Storage)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuGalaxyNavy, @Storage, @Storage128, N'Storage');

IF @SkuAirdopes IS NOT NULL AND @Color IS NOT NULL AND @ColorBlack IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuAirdopes AND [FK_Variant] = @Color)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuAirdopes, @Color, @ColorBlack, N'Color');

IF @SkuPic20 IS NOT NULL AND @Capacity IS NOT NULL AND @Cap2000W IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuPic20 AND [FK_Variant] = @Capacity)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuPic20, @Capacity, @Cap2000W, N'Capacity');

IF @SkuNeem IS NOT NULL AND @Capacity IS NOT NULL AND @Cap150ml IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[ProductVariantAttributes] WHERE [FK_ProductVariant] = @SkuNeem AND [FK_Variant] = @Capacity)
    INSERT INTO [dbo].[ProductVariantAttributes] ([FK_ProductVariant], [FK_Variant], [FK_VariantValue], [Description])
    VALUES (@SkuNeem, @Capacity, @Cap150ml, N'Capacity');

/* -------------------------------------------------------------------------
   Purchases + line items + stock batches
   ------------------------------------------------------------------------- */
DECLARE
    @PurchaseId INT,
    @DetailId INT;

IF @Malabar IS NOT NULL AND @SkuIphoneBlack IS NOT NULL AND @SkuIphoneBlue IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-MLB-26031')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Malabar, '2026-03-01', N'INV-MLB-26031', 775880.00, N'Opening smartphone stock for Kochi warehouse', '2026-03-01T10:15:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuIphoneBlack, 8, 61990.00, 79900.00, '2026-03-01T10:15:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuIphoneBlack, 8, '2026-03-01T10:15:00', 0);

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuIphoneBlue, 4, 69990.00, 89900.00, '2026-03-01T10:15:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuIphoneBlue, 4, '2026-03-01T10:15:00', 0);
END

IF @Malabar IS NOT NULL AND @SkuGalaxyNavy IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-MLB-26048')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Malabar, '2026-04-18', N'INV-MLB-26048', 299880.00, N'Galaxy A35 mid-range restock', '2026-04-18T11:40:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuGalaxyNavy, 12, 24990.00, 33999.00, '2026-04-18T11:40:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuGalaxyNavy, 12, '2026-04-18T11:40:00', 0);
END

IF @Deccan IS NOT NULL AND @SkuAirdopes IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-DCN-26056')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Deccan, '2026-05-06', N'INV-DCN-26056', 35600.00, N'Festival audio assortment — Airdopes 141', '2026-05-06T09:20:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuAirdopes, 40, 890.00, 4490.00, '2026-05-06T09:20:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuAirdopes, 40, '2026-05-06T09:20:00', 0);
END

IF @Coastal IS NOT NULL AND @SkuPic20 IS NOT NULL AND @SkuNeem IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-CHE-26062')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Coastal, '2026-06-12', N'INV-CHE-26062', 32310.00, N'Mixed kitchen and personal-care inbound', '2026-06-12T14:05:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuPic20, 15, 1850.00, 3495.00, '2026-06-12T14:05:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuPic20, 15, '2026-06-12T14:05:00', 0);

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [ExpiryDate], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuNeem, 48, 95.00, 170.00, '2028-01-31', '2026-06-12T14:05:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuNeem, 48, '2026-06-12T14:05:00', 0);
END

IF @Malabar IS NOT NULL AND @SkuIphoneBlack IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-MLB-26078')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Malabar, '2026-07-08', N'INV-MLB-26078', 365940.00, N'iPhone 15 Black 128GB refill after weekend sale', '2026-07-08T16:30:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuIphoneBlack, 6, 60990.00, 79900.00, '2026-07-08T16:30:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuIphoneBlack, 6, '2026-07-08T16:30:00', 0);
END

IF @Deccan IS NOT NULL AND @SkuAirdopes IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-DCN-26085')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Deccan, '2026-07-25', N'INV-DCN-26085', 21500.00, N'Second Airdopes lot at revised wholesale rate', '2026-07-25T10:50:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuAirdopes, 25, 860.00, 4490.00, '2026-07-25T10:50:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuAirdopes, 25, '2026-07-25T10:50:00', 0);
END

IF @Coastal IS NOT NULL AND @SkuPic20 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-CHE-26094')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Coastal, '2026-08-14', N'INV-CHE-26094', 18500.00, N'Prestige PIC 20 top-up for Onam season', '2026-08-14T13:10:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuPic20, 10, 1850.00, 3495.00, '2026-08-14T13:10:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuPic20, 10, '2026-08-14T13:10:00', 0);
END

IF @Malabar IS NOT NULL AND @SkuGalaxyNavy IS NOT NULL AND @SkuIphoneBlue IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[Purchase] WHERE [InvoiceNumber] = N'INV-MLB-26108')
BEGIN
    INSERT INTO [dbo].[Purchase] ([FK_Supplier], [PurchaseDate], [InvoiceNumber], [TotalAmount], [Notes], [CreatedOn], [Cancelled])
    VALUES (@Malabar, '2026-08-28', N'INV-MLB-26108', 329420.00, N'Mixed smartphone inbound before weekend promotions', '2026-08-28T09:45:00', 0);
    SET @PurchaseId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuGalaxyNavy, 5, 24490.00, 33999.00, '2026-08-28T09:45:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuGalaxyNavy, 5, '2026-08-28T09:45:00', 0);

    INSERT INTO [dbo].[PurchaseDetail] ([FK_Purchase], [FK_ProductVariant], [Quantity], [PurchasePrice], [MRP], [CreatedOn], [Cancelled])
    VALUES (@PurchaseId, @SkuIphoneBlue, 3, 68990.00, 89900.00, '2026-08-28T09:45:00', 0);
    SET @DetailId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[Stock] ([FK_PurchaseDetail], [FK_ProductVariant], [Quantity], [CreatedOn], [Cancelled])
    VALUES (@DetailId, @SkuIphoneBlue, 3, '2026-08-28T09:45:00', 0);
END

PRINT 'Development sample catalog seed completed.';
GO
