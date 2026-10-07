\echo 'Seeding complete SmartCart dummy data (storefront + admin)...'

/*
  Run after super_market_catalog.sql (and roles/admin seed).

  Storefront login (all demo shoppers):
    Email    : demo1@smartcart.local  …  demo18@smartcart.local
    Password : Admin@123

  Extra staff (same password):
    manager / Admin@123
    cashier / Admin@123

  Idempotent: safe to re-run. Demo orders use ordernumber DM###### .
*/

-- =============================================================================
-- Extra pack sizes
-- =============================================================================
INSERT INTO variantvalues (fk_variant, name, description, displayorder, cancelled)
SELECT v.id_variant, s.name, s.description, s.displayorder, FALSE
FROM variants v
CROSS JOIN (VALUES
    ('90 g', '90 grams', 16),
    ('125 g', '125 grams', 17),
    ('150 g', '150 grams', 18),
    ('250 g', '250 grams', 19),
    ('2 L', '2 litres', 20)
) AS s(name, description, displayorder)
WHERE v.name = 'Pack Size'
  AND COALESCE(v.cancelled, FALSE) = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM variantvalues vv
      WHERE vv.fk_variant = v.id_variant AND vv.name = s.name
  );

-- =============================================================================
-- Extra brands
-- =============================================================================
INSERT INTO brand (brandname, description, isactive, cancelled)
SELECT s.brandname, s.description, TRUE, FALSE
FROM (VALUES
    ('Oreo', 'Mondelez Oreo biscuits'),
    ('Kellogg''s', 'Breakfast cereals'),
    ('Horlicks', 'Malted health drink'),
    ('Kurkure', 'PepsiCo Kurkure'),
    ('Pepsi', 'PepsiCo cola'),
    ('Sprite', 'Coca-Cola Sprite'),
    ('Tropicana', 'PepsiCo juices'),
    ('Real', 'Dabur Real juices'),
    ('Dettol', 'Reckitt hygiene'),
    ('Lifebuoy', 'HUL Lifebuoy'),
    ('Harpic', 'Reckitt toilet care'),
    ('Lizol', 'Reckitt floor cleaner'),
    ('Johnson''s', 'Johnson baby care'),
    ('Kissan', 'HUL Kissan'),
    ('MTR', 'MTR foods'),
    ('Catch', 'Catch spices'),
    ('Madhur', 'Madhur sugar'),
    ('Harvest Gold', 'Harvest Gold bakery'),
    ('Bru', 'HUL Bru coffee'),
    ('Mother Dairy', 'Mother Dairy milk and curd'),
    ('Sunfeast', 'ITC Sunfeast'),
    ('Good Day', 'Britannia Good Day'),
    ('Magnum', 'Kwality Walls Magnum'),
    ('Boost', 'GSK Boost')
) AS s(brandname, description)
WHERE NOT EXISTS (
    SELECT 1 FROM brand b WHERE b.brandname = s.brandname
);

-- =============================================================================
-- Extra subcategories
-- =============================================================================
INSERT INTO subcategory (name, description, fk_category, isactive, cancelled)
SELECT s.name, s.description, c.id_category, TRUE, FALSE
FROM (VALUES
    ('Juices', 'Packaged fruit juices', 'Snacks & Beverages'),
    ('Spreads', 'Jam, ketchup, and sauces', 'Grocery Staples'),
    ('Breakfast', 'Cereals and health drinks', 'Grocery Staples'),
    ('Skin Care', 'Soaps and face care', 'Personal Care'),
    ('Home Hygiene', 'Toilet and floor cleaners', 'Household')
) AS s(name, description, categoryname)
INNER JOIN category c ON c.name = s.categoryname AND c.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM subcategory sc
    WHERE sc.name = s.name AND sc.fk_category = c.id_category
);

-- =============================================================================
-- Extra catalog rows
-- =============================================================================
CREATE TEMP TABLE IF NOT EXISTS catalog_demo (
    subcategory TEXT,
    categoryname TEXT,
    brandname TEXT,
    productname TEXT,
    slug TEXT,
    productdesc TEXT,
    sku TEXT,
    barcode TEXT,
    packsize TEXT,
    mrp NUMERIC(10,2),
    sellingprice NUMERIC(10,2),
    cost NUMERIC(10,2),
    uom TEXT,
    unitvalue NUMERIC(10,3),
    stockqty INT,
    imageurl TEXT
);
TRUNCATE catalog_demo;

INSERT INTO catalog_demo VALUES
('Fruits', 'Fresh Produce', 'SmartCart Fresh', 'Alphonso Mango', 'alphonso-mango', 'Seasonal Alphonso mangoes, sold by weight.', 'DEMO-MANGO-1KG', '8903000000014', '1 kg', 220.00, 189.00, 140.00, 'kg', 1, 36, 'https://images.unsplash.com/photo-1553279768-865429fa0078?auto=format&fit=crop&w=800&q=80'),
('Fruits', 'Fresh Produce', 'SmartCart Fresh', 'Mosambi', 'mosambi', 'Sweet mosambi, juicy and fresh.', 'DEMO-MOSAMBI-1KG', '8903000000021', '1 kg', 90.00, 72.00, 50.00, 'kg', 1, 40, 'https://images.unsplash.com/photo-1580052614034-c55d20bfee3b?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Green Capsicum', 'green-capsicum', 'Firm green capsicum for stir fry.', 'DEMO-CAPSI-500', '8903000000038', '500 g', 60.00, 48.00, 32.00, 'g', 500, 34, 'https://images.unsplash.com/photo-1563565375-f3fdfdbefa83?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Cucumber', 'farm-cucumber', 'Salad cucumbers.', 'DEMO-CUCUM-1KG', '8903000000045', '1 kg', 40.00, 32.00, 20.00, 'kg', 1, 50, 'https://images.unsplash.com/photo-1449300079323-02e209d9d3a6?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Carrot', 'farm-carrot', 'Orange table carrots.', 'DEMO-CARROT-1KG', '8903000000052', '1 kg', 55.00, 42.00, 28.00, 'kg', 1, 46, 'https://images.unsplash.com/photo-1598170845058-32b9d6a5da37?auto=format&fit=crop&w=800&q=80'),
('Milk', 'Dairy & Eggs', 'Amul', 'Amul Gold Full Cream Milk', 'amul-gold-milk', 'Full cream milk, 1 litre tetra.', 'DEMO-AMUL-GOLD-1L', '8903000000069', '1 L', 82.00, 74.00, 64.00, 'L', 1, 90, 'https://images.unsplash.com/photo-1550583724-b2692b85b150?auto=format&fit=crop&w=800&q=80'),
('Butter & Cheese', 'Dairy & Eggs', 'Amul', 'Amul Cheese Slices', 'amul-cheese-slices', 'Processed cheese slices, 200 g.', 'DEMO-AMUL-CHEESE-200', '8903000000076', '200 g', 145.00, 129.00, 105.00, 'g', 200, 28, 'https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?auto=format&fit=crop&w=800&q=80'),
('Curd & Yogurt', 'Dairy & Eggs', 'Mother Dairy', 'Mother Dairy Classic Dahi', 'mother-dairy-dahi', 'Set curd, 400 g.', 'DEMO-MD-DAHI-400', '8903000000083', '400 g', 50.00, 42.00, 32.00, 'g', 400, 38, 'https://images.unsplash.com/photo-1571212515416-fef01fc43622?auto=format&fit=crop&w=800&q=80'),
('Bread', 'Bakery', 'Harvest Gold', 'Harvest Gold White Bread', 'harvest-gold-white-bread', 'Soft sandwich bread.', 'DEMO-HG-BREAD-400', '8903000000090', '400 g', 45.00, 40.00, 28.00, 'g', 400, 32, 'https://images.unsplash.com/photo-1549931319-a545dcf3bc73?auto=format&fit=crop&w=800&q=80'),
('Biscuits', 'Bakery', 'Good Day', 'Britannia Good Day Cashew', 'good-day-cashew', 'Cashew cookies, 200 g.', 'DEMO-GD-CASHEW-200', '8903000000106', '200 g', 45.00, 38.00, 28.00, 'g', 200, 48, 'https://images.unsplash.com/photo-1499636136210-6f4ee915583e?auto=format&fit=crop&w=800&q=80'),
('Biscuits', 'Bakery', 'Oreo', 'Oreo Original Vanilla', 'oreo-original', 'Vanilla cream biscuits.', 'DEMO-OREO-150', '8903000000113', '150 g', 40.00, 35.00, 26.00, 'g', 150, 55, 'https://images.unsplash.com/photo-1621939514649-280e2ee25f60?auto=format&fit=crop&w=800&q=80'),
('Biscuits', 'Bakery', 'Sunfeast', 'Sunfeast Dark Fantasy Choco Fills', 'dark-fantasy-choco', 'Choco-filled cookies.', 'DEMO-DF-250', '8903000000120', '250 g', 85.00, 72.00, 55.00, 'g', 250, 30, 'https://images.unsplash.com/photo-1558961363-fa8fdf82db35?auto=format&fit=crop&w=800&q=80'),
('Flour & Sugar', 'Grocery Staples', 'Madhur', 'Madhur Pure Sugar', 'madhur-sugar', 'Crystal white sugar, 1 kg.', 'DEMO-MADHUR-SUGAR-1KG', '8903000000137', '1 kg', 52.00, 46.00, 36.00, 'kg', 1, 60, 'https://images.unsplash.com/photo-1587049352846-4a222e784d38?auto=format&fit=crop&w=800&q=80'),
('Oils', 'Grocery Staples', 'Fortune', 'Fortune Mustard Oil', 'fortune-mustard-oil', 'Kachi ghani mustard oil, 1 litre.', 'DEMO-FORT-MUST-1L', '8903000000144', '1 L', 195.00, 169.00, 140.00, 'L', 1, 24, 'https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?auto=format&fit=crop&w=800&q=80'),
('Spices', 'Grocery Staples', 'Catch', 'Catch Red Chilli Powder', 'catch-chilli', 'Hot red chilli powder, 100 g.', 'DEMO-CATCH-CHILLI-100', '8903000000151', '100 g', 68.00, 58.00, 42.00, 'g', 100, 40, 'https://images.unsplash.com/photo-1596040033229-a9821ebd058d?auto=format&fit=crop&w=800&q=80'),
('Spices', 'Grocery Staples', 'MTR', 'MTR Rasam Powder', 'mtr-rasam-powder', 'South Indian rasam masala, 200 g.', 'DEMO-MTR-RASAM-200', '8903000000168', '200 g', 92.00, 79.00, 58.00, 'g', 200, 26, 'https://images.unsplash.com/photo-1596040033229-a9821ebd058d?auto=format&fit=crop&w=800&q=80'),
('Spreads', 'Grocery Staples', 'Kissan', 'Kissan Mixed Fruit Jam', 'kissan-mixed-fruit-jam', 'Mixed fruit jam, 500 g.', 'DEMO-KISSAN-JAM-500', '8903000000175', '500 g', 155.00, 135.00, 108.00, 'g', 500, 22, 'https://images.unsplash.com/photo-1526318472351-c75fcf070305?auto=format&fit=crop&w=800&q=80'),
('Spreads', 'Grocery Staples', 'Maggi', 'Maggi Tomato Ketchup', 'maggi-ketchup', 'Tomato ketchup, 500 g.', 'DEMO-MAGGI-KETCH-500', '8903000000182', '500 g', 125.00, 109.00, 82.00, 'g', 500, 28, 'https://images.unsplash.com/photo-1472476443507-c7a9ba75bb12?auto=format&fit=crop&w=800&q=80'),
('Breakfast', 'Grocery Staples', 'Kellogg''s', 'Kellogg''s Corn Flakes', 'kelloggs-corn-flakes', 'Breakfast corn flakes, 250 g.', 'DEMO-KELLOGG-CF-250', '8903000000199', '250 g', 145.00, 125.00, 98.00, 'g', 250, 20, 'https://images.unsplash.com/photo-1525351484163-7529414344d8?auto=format&fit=crop&w=800&q=80'),
('Breakfast', 'Grocery Staples', 'Horlicks', 'Horlicks Classic Malt', 'horlicks-classic', 'Malted health drink, 500 g.', 'DEMO-HORLICKS-500', '8903000000205', '500 g', 289.00, 259.00, 210.00, 'g', 500, 18, 'https://images.unsplash.com/photo-1564890369478-c89ca4d9f3f9?auto=format&fit=crop&w=800&q=80'),
('Breakfast', 'Grocery Staples', 'Boost', 'Boost Health Drink', 'boost-health-drink', 'Chocolate malt drink, 500 g.', 'DEMO-BOOST-500', '8903000000212', '500 g', 275.00, 245.00, 198.00, 'g', 500, 16, 'https://images.unsplash.com/photo-1511920170033-f8396924c348?auto=format&fit=crop&w=800&q=80'),
('Chips', 'Snacks & Beverages', 'Kurkure', 'Kurkure Masala Munch', 'kurkure-masala-munch', 'Masala corn puffs, 90 g.', 'DEMO-KURKURE-90', '8903000000229', '90 g', 20.00, 18.00, 12.00, 'g', 90, 80, 'https://images.unsplash.com/photo-1613919113641-5aa046527deb?auto=format&fit=crop&w=800&q=80'),
('Soft Drinks', 'Snacks & Beverages', 'Pepsi', 'Pepsi', 'pepsi-750ml', 'Pepsi, 750 ml PET.', 'DEMO-PEPSI-750', '8903000000236', '750 ml', 45.00, 40.00, 30.00, 'ml', 750, 70, 'https://images.unsplash.com/photo-1629203851122-3726ecdf080e?auto=format&fit=crop&w=800&q=80'),
('Soft Drinks', 'Snacks & Beverages', 'Sprite', 'Sprite', 'sprite-750ml', 'Lemon-lime soda, 750 ml.', 'DEMO-SPRITE-750', '8903000000243', '750 ml', 45.00, 40.00, 30.00, 'ml', 750, 64, 'https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?auto=format&fit=crop&w=800&q=80'),
('Juices', 'Snacks & Beverages', 'Tropicana', 'Tropicana Orange Delight', 'tropicana-orange', 'Orange juice, 1 litre.', 'DEMO-TROP-OJ-1L', '8903000000250', '1 L', 135.00, 119.00, 92.00, 'L', 1, 26, 'https://images.unsplash.com/photo-1600271886742-f049cd451bba?auto=format&fit=crop&w=800&q=80'),
('Juices', 'Snacks & Beverages', 'Real', 'Real Mixed Fruit Juice', 'real-mixed-fruit', 'Mixed fruit nectar, 1 litre.', 'DEMO-REAL-MF-1L', '8903000000267', '1 L', 125.00, 109.00, 84.00, 'L', 1, 24, 'https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?auto=format&fit=crop&w=800&q=80'),
('Tea & Coffee', 'Snacks & Beverages', 'Tata', 'Tata Tea Premium', 'tata-tea-premium', 'Everyday leaf tea, 250 g.', 'DEMO-TATA-TEA-250', '8903000000274', '250 g', 165.00, 149.00, 118.00, 'g', 250, 30, 'https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=80'),
('Tea & Coffee', 'Snacks & Beverages', 'Bru', 'Bru Instant Coffee', 'bru-instant', 'Instant coffee, 100 g.', 'DEMO-BRU-100', '8903000000281', '100 g', 210.00, 185.00, 150.00, 'g', 100, 22, 'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=800&q=80'),
('Water', 'Snacks & Beverages', 'Bisleri', 'Bisleri Packaged Water 2L', 'bisleri-water-2l', 'Family pack drinking water.', 'DEMO-BISLERI-2L', '8903000000298', '2 L', 30.00, 26.00, 16.00, 'L', 2, 80, 'https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=800&q=80'),
('Ice Cream', 'Frozen Foods', 'Magnum', 'Magnum Classic Almond', 'magnum-classic-almond', 'Almond ice cream stick.', 'DEMO-MAGNUM-1', '8903000000304', '1 piece', 80.00, 70.00, 48.00, 'pcs', 1, 12, 'https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=80'),
('Frozen Snacks', 'Frozen Foods', 'McCain', 'McCain Aloo Tikki', 'mccain-aloo-tikki', 'Ready-to-fry aloo tikki, 400 g.', 'DEMO-MCCAIN-TIKKI-400', '8903000000311', '400 g', 165.00, 149.00, 118.00, 'g', 400, 18, 'https://images.unsplash.com/photo-1601050690597-df0568f70950?auto=format&fit=crop&w=800&q=80'),
('Skin Care', 'Personal Care', 'Dettol', 'Dettol Original Soap', 'dettol-original-soap', 'Antibacterial soap, 125 g.', 'DEMO-DETTOL-SOAP-125', '8903000000328', '125 g', 48.00, 42.00, 30.00, 'g', 125, 44, 'https://images.unsplash.com/photo-1584305574647-0cc949a2e1d0?auto=format&fit=crop&w=800&q=80'),
('Skin Care', 'Personal Care', 'Lifebuoy', 'Lifebuoy Total 10 Soap', 'lifebuoy-total-soap', 'Germ protection soap, 125 g.', 'DEMO-LIFEBUOY-125', '8903000000335', '125 g', 38.00, 32.00, 22.00, 'g', 125, 50, 'https://images.unsplash.com/photo-1600857544200-b2f666a9a2ec?auto=format&fit=crop&w=800&q=80'),
('Hair Care', 'Personal Care', 'Himalaya', 'Himalaya Gentle Baby Shampoo', 'himalaya-baby-shampoo', 'No-tears baby shampoo, 200 g.', 'DEMO-HIM-SHAMP-200', '8903000000342', '200 g', 175.00, 155.00, 120.00, 'g', 200, 16, 'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?auto=format&fit=crop&w=800&q=80'),
('Oral Care', 'Personal Care', 'Colgate', 'Colgate MaxFresh', 'colgate-maxfresh', 'Cooling crystal toothpaste, 150 g.', 'DEMO-COLGATE-MAX-150', '8903000000359', '150 g', 118.00, 99.00, 76.00, 'g', 150, 30, 'https://images.unsplash.com/photo-1559591937-abc3a5d02748?auto=format&fit=crop&w=800&q=80'),
('Laundry', 'Household', 'Surf Excel', 'Surf Excel Liquid', 'surf-excel-liquid', 'Liquid detergent, 500 g.', 'DEMO-SURF-LIQ-500', '8903000000366', '500 g', 149.00, 129.00, 98.00, 'g', 500, 22, 'https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?auto=format&fit=crop&w=800&q=80'),
('Home Hygiene', 'Household', 'Harpic', 'Harpic Power Plus', 'harpic-power-plus', 'Toilet cleaner, 500 g.', 'DEMO-HARPIC-500', '8903000000373', '500 g', 109.00, 95.00, 72.00, 'g', 500, 20, 'https://images.unsplash.com/photo-1585421514738-01798e348b17?auto=format&fit=crop&w=800&q=80'),
('Home Hygiene', 'Household', 'Lizol', 'Lizol Floral Disinfectant', 'lizol-floral', 'Floor cleaner, 500 g.', 'DEMO-LIZOL-500', '8903000000380', '500 g', 115.00, 99.00, 76.00, 'g', 500, 20, 'https://images.unsplash.com/photo-1563453392212-326f5e854473?auto=format&fit=crop&w=800&q=80'),
('Diapers', 'Baby Care', 'Pampers', 'Pampers Pants L', 'pampers-pants-l', 'Pants-style diapers, pack of 20.', 'DEMO-PAMPERS-L20', '8903000000397', '20 pcs', 449.00, 399.00, 320.00, 'pcs', 20, 12, 'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=800&q=80'),
('Diapers', 'Baby Care', 'Johnson''s', 'Johnson''s Baby Soap', 'johnsons-baby-soap', 'Mild baby soap, 100 g.', 'DEMO-JOHNSON-SOAP-100', '8903000000403', '100 g', 65.00, 55.00, 40.00, 'g', 100, 24, 'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=800&q=80');

INSERT INTO products (fk_subcategory, fk_brand, name, slug, description, isactive, sellonline, createdat, cancelled)
SELECT
    sc.id_subcategory,
    b.id_brand,
    MIN(p.productname),
    p.slug,
    MIN(p.productdesc),
    TRUE, TRUE, NOW(), FALSE
FROM catalog_demo p
INNER JOIN category c ON c.name = p.categoryname AND c.cancelled = FALSE
INNER JOIN subcategory sc ON sc.name = p.subcategory AND sc.fk_category = c.id_category AND COALESCE(sc.cancelled, FALSE) = FALSE
INNER JOIN brand b ON b.brandname = p.brandname AND b.cancelled = FALSE
WHERE NOT EXISTS (SELECT 1 FROM products x WHERE x.slug = p.slug)
GROUP BY p.slug, sc.id_subcategory, b.id_brand;

INSERT INTO productvariants (
    fk_product, sku, barcode, variantlabel, description,
    mrp, sellingprice, unitofmeasure, unitvalue,
    isdefault, maxorderqty, isactive, sellonline, createdat, cancelled
)
SELECT
    pr.id_product, c.sku, c.barcode, c.packsize,
    c.productname || ', ' || c.packsize,
    c.mrp, c.sellingprice, c.uom, c.unitvalue,
    TRUE, 20, TRUE, TRUE, NOW(), FALSE
FROM catalog_demo c
INNER JOIN products pr ON pr.slug = c.slug AND pr.cancelled = FALSE
WHERE NOT EXISTS (SELECT 1 FROM productvariants v WHERE v.sku = c.sku);

INSERT INTO productvariantattributes (fk_productvariant, fk_variant, fk_variantvalue, description)
SELECT pv.id_productvariant, v.id_variant, vv.id_variantvalue, 'Pack Size'
FROM catalog_demo c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
INNER JOIN variants v ON v.name = 'Pack Size' AND COALESCE(v.cancelled, FALSE) = FALSE
INNER JOIN variantvalues vv ON vv.fk_variant = v.id_variant AND vv.name = c.packsize AND COALESCE(vv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM productvariantattributes a
    WHERE a.fk_productvariant = pv.id_productvariant AND a.fk_variant = v.id_variant
);

INSERT INTO productmedia (fk_product, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT pr.id_product, 'Image', MIN(c.imageurl), 0, TRUE, NOW()
FROM catalog_demo c
INNER JOIN products pr ON pr.slug = c.slug AND pr.cancelled = FALSE
WHERE NOT EXISTS (SELECT 1 FROM productmedia m WHERE m.fk_product = pr.id_product)
GROUP BY pr.id_product;

INSERT INTO skumedia (fk_productsku, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT pv.id_productvariant, 'Image', c.imageurl, 0, TRUE, NOW()
FROM catalog_demo c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (SELECT 1 FROM skumedia m WHERE m.fk_productsku = pv.id_productvariant);

INSERT INTO productimages (fk_product, imageurl, cancelled)
SELECT pr.id_product, MIN(c.imageurl), FALSE
FROM catalog_demo c
INNER JOIN products pr ON pr.slug = c.slug AND pr.cancelled = FALSE
WHERE NOT EXISTS (SELECT 1 FROM productimages i WHERE i.fk_product = pr.id_product)
GROUP BY pr.id_product;

INSERT INTO productimages (fk_product, imageurl, cancelled)
SELECT p.id_product, m.mediaurl, FALSE
FROM productmedia m
INNER JOIN products p ON p.id_product = m.fk_product
WHERE m.mediatype = 'Image'
  AND NOT EXISTS (SELECT 1 FROM productimages i WHERE i.fk_product = p.id_product);

INSERT INTO productvariantimages (fk_productvariant, imageurl, isprimary, displayorder, createdat)
SELECT pv.id_productvariant, s.mediaurl, TRUE, 0, NOW()
FROM skumedia s
INNER JOIN productvariants pv ON pv.id_productvariant = s.fk_productsku
WHERE s.mediatype = 'Image'
  AND NOT EXISTS (
      SELECT 1 FROM productvariantimages i WHERE i.fk_productvariant = pv.id_productvariant
  );

-- =============================================================================
-- Category / subcategory / brand images (homepage tiles)
-- =============================================================================
UPDATE category c
SET imageurl = s.imageurl
FROM (VALUES
    ('Fresh Produce', 'https://images.unsplash.com/photo-1610348725531-843dff563e2c?auto=format&fit=crop&w=800&q=80'),
    ('Dairy & Eggs', 'https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=800&q=80'),
    ('Bakery', 'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=800&q=80'),
    ('Grocery Staples', 'https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=800&q=80'),
    ('Snacks & Beverages', 'https://images.unsplash.com/photo-1566478989037-eec170784d0b?auto=format&fit=crop&w=800&q=80'),
    ('Frozen Foods', 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=800&q=80'),
    ('Household', 'https://images.unsplash.com/photo-1585421514738-01798e348b17?auto=format&fit=crop&w=800&q=80'),
    ('Baby Care', 'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=800&q=80'),
    ('Personal Care', 'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=800&q=80')
) AS s(name, imageurl)
WHERE c.name = s.name AND c.cancelled = FALSE;

UPDATE subcategory sc
SET imageurl = s.imageurl
FROM (VALUES
    ('Fruits', 'https://images.unsplash.com/photo-1619566636858-adf3ef46400b?auto=format&fit=crop&w=600&q=80'),
    ('Vegetables', 'https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=600&q=80'),
    ('Milk', 'https://images.unsplash.com/photo-1563636619-e9143da7973b?auto=format&fit=crop&w=600&q=80'),
    ('Bread', 'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=600&q=80'),
    ('Chips', 'https://images.unsplash.com/photo-1566478989037-eec170784d0b?auto=format&fit=crop&w=600&q=80'),
    ('Tea & Coffee', 'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=600&q=80'),
    ('Diapers', 'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=600&q=80')
) AS s(name, imageurl)
WHERE sc.name = s.name AND COALESCE(sc.cancelled, FALSE) = FALSE;

UPDATE brand b
SET imageurl = 'https://images.unsplash.com/photo-1580913428023-02c695666330?auto=format&fit=crop&w=400&q=80'
WHERE b.imageurl IS NULL AND b.cancelled = FALSE;

-- =============================================================================
-- Extra suppliers + purchase (stock for new SKUs)
-- =============================================================================
INSERT INTO supplier (
    name, companyname, email, phone,
    state, district, city, address, pincode,
    description, isactive, createdat, cancelled
)
SELECT s.name, s.companyname, s.email, s.phone, s.state, s.district, s.city, s.address, s.pincode, s.description, TRUE, NOW(), FALSE
FROM (VALUES
    ('Kerala Fresh Farms', 'Kerala Fresh Farms LLP', 'fresh@smartcart.local', '9876502201',
     'Kerala', 'Thrissur', 'Thrissur', 'NH 544, Mannuthy Agri Hub', '680651',
     'Fruits and vegetables aggregator.'),
    ('FMCG Distro South', 'FMCG Distro South Pvt Ltd', 'fmcg@smartcart.local', '9876502202',
     'Kerala', 'Ernakulam', 'Kochi', 'Edapally Bypass, Warehouse 7', '682024',
     'Packed FMCG, beverages, and personal care.'),
    ('Cold Chain Foods', 'Cold Chain Foods India', 'cold@smartcart.local', '9876502203',
     'Kerala', 'Kozhikode', 'Kozhikode', 'Industrial Estate, West Hill', '673005',
     'Frozen and dairy cold-chain supplier.')
) AS s(name, companyname, email, phone, state, district, city, address, pincode, description)
WHERE NOT EXISTS (SELECT 1 FROM supplier x WHERE x.email = s.email);

INSERT INTO purchase (fk_supplier, purchasedate, invoicenumber, totalamount, notes, createdon, cancelled)
SELECT s.id_supplier, DATE '2026-09-12', 'INV-DEMO-26002', 0,
       'Dummy restock — extra catalog and seasonal SKUs', TIMESTAMP '2026-09-12 11:30:00', FALSE
FROM supplier s
WHERE s.email = 'fmcg@smartcart.local' AND s.cancelled = FALSE
  AND NOT EXISTS (SELECT 1 FROM purchase p WHERE p.invoicenumber = 'INV-DEMO-26002');

INSERT INTO purchasedetail (fk_purchase, fk_productvariant, quantity, purchaseprice, mrp, createdon, cancelled)
SELECT p.id_purchase, pv.id_productvariant, c.stockqty, c.cost, c.mrp, TIMESTAMP '2026-09-12 11:30:00', FALSE
FROM catalog_demo c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
INNER JOIN purchase p ON p.invoicenumber = 'INV-DEMO-26002'
WHERE NOT EXISTS (
    SELECT 1 FROM purchasedetail pd
    WHERE pd.fk_purchase = p.id_purchase AND pd.fk_productvariant = pv.id_productvariant
);

INSERT INTO stock (fk_purchasedetail, fk_productvariant, quantity, createdon, cancelled)
SELECT pd.id_purchasedetail, pd.fk_productvariant, pd.quantity, TIMESTAMP '2026-09-12 11:30:00', FALSE
FROM purchasedetail pd
INNER JOIN purchase p ON p.id_purchase = pd.fk_purchase
WHERE p.invoicenumber = 'INV-DEMO-26002'
  AND NOT EXISTS (SELECT 1 FROM stock s WHERE s.fk_purchasedetail = pd.id_purchasedetail);

UPDATE purchase p
SET totalamount = x.totalamount
FROM (
    SELECT fk_purchase, SUM(purchaseprice * quantity) AS totalamount
    FROM purchasedetail WHERE cancelled = FALSE
    GROUP BY fk_purchase
) x
WHERE x.fk_purchase = p.id_purchase AND p.invoicenumber = 'INV-DEMO-26002';

INSERT INTO purchase (fk_supplier, purchasedate, invoicenumber, totalamount, notes, createdon, cancelled)
SELECT s.id_supplier, DATE '2026-09-28', 'INV-DEMO-26003', 0,
       'Fresh produce weekly load', TIMESTAMP '2026-09-28 08:15:00', FALSE
FROM supplier s
WHERE s.email = 'fresh@smartcart.local' AND s.cancelled = FALSE
  AND NOT EXISTS (SELECT 1 FROM purchase p WHERE p.invoicenumber = 'INV-DEMO-26003');

INSERT INTO purchasedetail (fk_purchase, fk_productvariant, quantity, purchaseprice, mrp, expirydate, createdon, cancelled)
SELECT p.id_purchase, pv.id_productvariant, 40, 20.00, pv.mrp, DATE '2026-10-12', TIMESTAMP '2026-09-28 08:15:00', FALSE
FROM purchase p
CROSS JOIN productvariants pv
INNER JOIN products pr ON pr.id_product = pv.fk_product
INNER JOIN subcategory sc ON sc.id_subcategory = pr.fk_subcategory
INNER JOIN category c ON c.id_category = sc.fk_category
WHERE p.invoicenumber = 'INV-DEMO-26003'
  AND c.name = 'Fresh Produce'
  AND COALESCE(pv.cancelled, FALSE) = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM purchasedetail pd
      WHERE pd.fk_purchase = p.id_purchase AND pd.fk_productvariant = pv.id_productvariant
  );

INSERT INTO stock (fk_purchasedetail, fk_productvariant, quantity, createdon, cancelled)
SELECT pd.id_purchasedetail, pd.fk_productvariant, pd.quantity, TIMESTAMP '2026-09-28 08:15:00', FALSE
FROM purchasedetail pd
INNER JOIN purchase p ON p.id_purchase = pd.fk_purchase
WHERE p.invoicenumber = 'INV-DEMO-26003'
  AND NOT EXISTS (SELECT 1 FROM stock s WHERE s.fk_purchasedetail = pd.id_purchasedetail);

UPDATE purchase p
SET totalamount = x.totalamount
FROM (
    SELECT fk_purchase, SUM(purchaseprice * quantity) AS totalamount
    FROM purchasedetail WHERE cancelled = FALSE GROUP BY fk_purchase
) x
WHERE x.fk_purchase = p.id_purchase AND p.invoicenumber = 'INV-DEMO-26003';

-- =============================================================================
-- Homepage banners, featured categories, featured products
-- =============================================================================
INSERT INTO homepage_banners (title, imageurl, isactive, displayorder, createdat, cancelled)
SELECT s.title, s.imageurl, TRUE, s.displayorder, NOW(), FALSE
FROM (VALUES
    ('Harvest Fresh This Week', 'https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=1600&q=80', 10),
    ('Dairy & Breakfast Essentials', 'https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=1600&q=80', 20),
    ('Snack Time Combos', 'https://images.unsplash.com/photo-1621939514649-280e2ee25f60?auto=format&fit=crop&w=1600&q=80', 30),
    ('Home Care Mega Save', 'https://images.unsplash.com/photo-1581578731117-4d571e4bfe5a?auto=format&fit=crop&w=1600&q=80', 40),
    ('Baby & Personal Care', 'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=1600&q=80', 50),
    ('Frozen Treats & Ready Meals', 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=1600&q=80', 60)
) AS s(title, imageurl, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM homepage_banners b WHERE b.title = s.title AND b.cancelled = FALSE
);

INSERT INTO homepage_banner_categories (fk_banner, fk_category)
SELECT b.id_banner, c.id_category
FROM (VALUES
    ('Harvest Fresh This Week', 'Fresh Produce'),
    ('Harvest Fresh This Week', 'Grocery Staples'),
    ('Dairy & Breakfast Essentials', 'Dairy & Eggs'),
    ('Dairy & Breakfast Essentials', 'Bakery'),
    ('Snack Time Combos', 'Snacks & Beverages'),
    ('Home Care Mega Save', 'Household'),
    ('Baby & Personal Care', 'Baby Care'),
    ('Baby & Personal Care', 'Personal Care'),
    ('Frozen Treats & Ready Meals', 'Frozen Foods')
) AS s(bannertitle, categoryname)
INNER JOIN homepage_banners b ON b.title = s.bannertitle AND b.cancelled = FALSE
INNER JOIN category c ON c.name = s.categoryname AND c.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM homepage_banner_categories x
    WHERE x.fk_banner = b.id_banner AND x.fk_category = c.id_category
);

INSERT INTO homepage_categories (fk_category, isactive, displayorder, createdat, cancelled)
SELECT c.id_category, TRUE, s.displayorder, NOW(), FALSE
FROM (VALUES
    ('Fresh Produce', 10),
    ('Dairy & Eggs', 20),
    ('Bakery', 30),
    ('Grocery Staples', 40),
    ('Snacks & Beverages', 50),
    ('Frozen Foods', 60),
    ('Household', 70),
    ('Personal Care', 80),
    ('Baby Care', 90)
) AS s(categoryname, displayorder)
INNER JOIN category c ON c.name = s.categoryname AND c.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM homepage_categories hc WHERE hc.fk_category = c.id_category
);

INSERT INTO homepage_products (fk_product, isactive, displayorder, createdat, cancelled)
SELECT p.id_product, TRUE, s.displayorder, NOW(), FALSE
FROM (VALUES
    ('shimla-apple', 10),
    ('amul-taaza-toned-milk', 20),
    ('tata-tea-gold', 30),
    ('india-gate-basmati-rice', 40),
    ('lays-classic-salted', 50),
    ('maggi-2-minute-noodles', 60),
    ('dove-daily-shine-shampoo', 70),
    ('pampers-baby-dry-m', 80),
    ('kwality-walls-cornetto', 90),
    ('oreo-original', 100),
    ('horlicks-classic', 110),
    ('surf-excel-easy-wash', 120)
) AS s(slug, displayorder)
INNER JOIN products p ON p.slug = s.slug AND p.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM homepage_products hp WHERE hp.fk_product = p.id_product
);

-- =============================================================================
-- Staff roles + extra employees
-- =============================================================================
INSERT INTO userroles (rolename, description, issystemrole, isactive, createdat, cancelled)
SELECT s.rolename, s.description, FALSE, TRUE, NOW(), FALSE
FROM (VALUES
    ('Manager', 'Store manager — orders, stock, sales, homepage'),
    ('Cashier', 'Counter billing and walk-in sales')
) AS s(rolename, description)
WHERE NOT EXISTS (SELECT 1 FROM userroles r WHERE r.rolename = s.rolename);

INSERT INTO userrolepermissions (fk_userrole, fk_permission, createdat, cancelled)
SELECT r.id_userrole, p.id_permission, NOW(), FALSE
FROM userroles r
INNER JOIN permissions p ON p.cancelled = FALSE
WHERE r.rolename = 'Manager'
  AND r.cancelled = FALSE
  AND p.permissioncode IN (
      'Dashboard.View',
      'Orders.View', 'Orders.Edit', 'Orders.Cancel',
      'Stock.View', 'Stock.Adjust',
      'Purchases.View',
      'Sales.View', 'Sales.Create',
      'SalesReturns.View', 'SalesReturns.Create',
      'Billing.View', 'Billing.Create', 'Billing.Print',
      'Homepage.View', 'Homepage.Edit',
      'Ratings.View', 'Ratings.Delete',
      'Products.View', 'Categories.View', 'Suppliers.View'
  )
  AND NOT EXISTS (
      SELECT 1 FROM userrolepermissions urp
      WHERE urp.fk_userrole = r.id_userrole AND urp.fk_permission = p.id_permission
  );

INSERT INTO userrolepermissions (fk_userrole, fk_permission, createdat, cancelled)
SELECT r.id_userrole, p.id_permission, NOW(), FALSE
FROM userroles r
INNER JOIN permissions p ON p.cancelled = FALSE
WHERE r.rolename = 'Cashier'
  AND r.cancelled = FALSE
  AND p.permissioncode IN (
      'Dashboard.View',
      'Orders.View',
      'Sales.View', 'Sales.Create',
      'Billing.View', 'Billing.Create', 'Billing.Print'
  )
  AND NOT EXISTS (
      SELECT 1 FROM userrolepermissions urp
      WHERE urp.fk_userrole = r.id_userrole AND urp.fk_permission = p.id_permission
  );

INSERT INTO adminusers (
    fk_userrole, username, passwordhash, fullname, email,
    phonenumber, isactive, createdat, cancelled
)
SELECT r.id_userrole, s.username,
       'AQAAAAEAAYagAAAAEPGBDPr1vrIh1zlYcblDhIRjePewKQpikuv9yxb2oVFsGwoDVTS9GhHDPlcaMiPGEA==',
       s.fullname, s.email, s.phone, TRUE, NOW(), FALSE
FROM (VALUES
    ('Manager', 'manager', 'Nisha Varma', 'manager@smartcart.local', '9876503301'),
    ('Cashier', 'cashier', 'Arun Prakash', 'cashier@smartcart.local', '9876503302')
) AS s(rolename, username, fullname, email, phone)
INNER JOIN userroles r ON r.rolename = s.rolename AND r.cancelled = FALSE
WHERE NOT EXISTS (SELECT 1 FROM adminusers a WHERE a.username = s.username);

-- =============================================================================
-- Demo shoppers + addresses
-- =============================================================================
INSERT INTO users (username, fullname, passwordhash, email, phonenumber, isadmin, createdat, cancelled)
SELECT
    s.username,
    s.fullname,
    'AQAAAAEAAYagAAAAEPGBDPr1vrIh1zlYcblDhIRjePewKQpikuv9yxb2oVFsGwoDVTS9GhHDPlcaMiPGEA==',
    s.email,
    s.phone,
    FALSE,
    TIMESTAMP '2026-08-01 09:00:00' + ((s.n - 1) || ' days')::interval,
    FALSE
FROM (VALUES
    (1, 'demo1', 'Ananya Nair', 'demo1@smartcart.local', '9876504001'),
    (2, 'demo2', 'Rohan Menon', 'demo2@smartcart.local', '9876504002'),
    (3, 'demo3', 'Diya Krishnan', 'demo3@smartcart.local', '9876504003'),
    (4, 'demo4', 'Arjun Pillai', 'demo4@smartcart.local', '9876504004'),
    (5, 'demo5', 'Meera Thomas', 'demo5@smartcart.local', '9876504005'),
    (6, 'demo6', 'Fahad Rahman', 'demo6@smartcart.local', '9876504006'),
    (7, 'demo7', 'Sneha Jose', 'demo7@smartcart.local', '9876504007'),
    (8, 'demo8', 'Nikhil Varma', 'demo8@smartcart.local', '9876504008'),
    (9, 'demo9', 'Aisha Fathima', 'demo9@smartcart.local', '9876504009'),
    (10, 'demo10', 'Vivek Nambiar', 'demo10@smartcart.local', '9876504010'),
    (11, 'demo11', 'Lakshmi Iyer', 'demo11@smartcart.local', '9876504011'),
    (12, 'demo12', 'Aditya Shenoy', 'demo12@smartcart.local', '9876504012'),
    (13, 'demo13', 'Priya Kurian', 'demo13@smartcart.local', '9876504013'),
    (14, 'demo14', 'Hassan Ali', 'demo14@smartcart.local', '9876504014'),
    (15, 'demo15', 'Kavya Babu', 'demo15@smartcart.local', '9876504015'),
    (16, 'demo16', 'George Mathew', 'demo16@smartcart.local', '9876504016'),
    (17, 'demo17', 'Riya Sebastian', 'demo17@smartcart.local', '9876504017'),
    (18, 'demo18', 'Imran Khan', 'demo18@smartcart.local', '9876504018')
) AS s(n, username, fullname, email, phone)
WHERE NOT EXISTS (SELECT 1 FROM users u WHERE u.email = s.email);

INSERT INTO useraddresses (
    userid, addresstype, receivername, phone, addressline, city, pincode,
    latitude, longitude, isdefault, createdat, cancelled
)
SELECT
    u.id_user, s.addresstype, u.fullname, u.phonenumber,
    s.addressline, s.city, s.pincode,
    s.lat, s.lng, TRUE, NOW(), FALSE
FROM (VALUES
    ('demo1@smartcart.local', 'Home', '12, Panampilly Nagar', 'Kochi', '682036', 9.9648, 76.2940),
    ('demo2@smartcart.local', 'Home', '44, Kadavanthra Junction', 'Kochi', '682020', 9.9665, 76.3012),
    ('demo3@smartcart.local', 'Home', '8, Palarivattom Bypass', 'Kochi', '682025', 10.0030, 76.3100),
    ('demo4@smartcart.local', 'Home', '21, MG Road, Thrissur', 'Thrissur', '680001', 10.5276, 76.2144),
    ('demo5@smartcart.local', 'Home', '5, Punkunnam', 'Thrissur', '680002', 10.5350, 76.2050),
    ('demo6@smartcart.local', 'Home', '17, SM Street', 'Kozhikode', '673001', 11.2588, 75.7804),
    ('demo7@smartcart.local', 'Home', '9, West Hill', 'Kozhikode', '673005', 11.2760, 75.7660),
    ('demo8@smartcart.local', 'Home', '31, Kowdiar', 'Thiruvananthapuram', '695003', 8.5241, 76.9366),
    ('demo9@smartcart.local', 'Home', '14, Kazhakkoottam', 'Thiruvananthapuram', '695582', 8.5680, 76.8730),
    ('demo10@smartcart.local', 'Home', '6, Payyambalam', 'Kannur', '670001', 11.8745, 75.3704),
    ('demo11@smartcart.local', 'Home', '22, Aluva Market Road', 'Aluva', '683101', 10.1076, 76.3516),
    ('demo12@smartcart.local', 'Home', '18, Kakkanad Infopark', 'Kochi', '682030', 10.0110, 76.3420),
    ('demo13@smartcart.local', 'Home', '3, Fort Kochi Beach Road', 'Kochi', '682001', 9.9650, 76.2420),
    ('demo14@smartcart.local', 'Home', '11, Kaloor Stadium Link', 'Kochi', '682017', 9.9940, 76.2910),
    ('demo15@smartcart.local', 'Home', '27, Vyttila Mobility Hub', 'Kochi', '682019', 9.9670, 76.3180),
    ('demo16@smartcart.local', 'Home', '4, Palakkad Town Hall', 'Palakkad', '678001', 10.7867, 76.6548),
    ('demo17@smartcart.local', 'Home', '16, Kollam Chinnakada', 'Kollam', '691001', 8.8932, 76.6141),
    ('demo18@smartcart.local', 'Home', '2, Malappuram Down Hill', 'Malappuram', '676519', 11.0500, 76.0710)
) AS s(email, addresstype, addressline, city, pincode, lat, lng)
INNER JOIN users u ON u.email = s.email
WHERE NOT EXISTS (
    SELECT 1 FROM useraddresses a WHERE a.userid = u.id_user AND a.addressline = s.addressline
);

INSERT INTO useraddresses (
    userid, addresstype, receivername, phone, addressline, city, pincode,
    isdefault, createdat, cancelled
)
SELECT u.id_user, 'Work', u.fullname, u.phonenumber,
       'SmartCart Office, Infopark Phase 1', 'Kochi', '682030',
       FALSE, NOW(), FALSE
FROM users u
WHERE u.email IN ('demo1@smartcart.local', 'demo12@smartcart.local', 'demo4@smartcart.local')
  AND NOT EXISTS (
      SELECT 1 FROM useraddresses a
      WHERE a.userid = u.id_user AND a.addresstype = 'Work' AND a.cancelled = FALSE
  );

-- =============================================================================
-- Carts + wishlists
-- =============================================================================
INSERT INTO cart (fk_user, createdat)
SELECT u.id_user, NOW()
FROM users u
WHERE u.email IN (
    'demo1@smartcart.local', 'demo2@smartcart.local', 'demo5@smartcart.local',
    'demo8@smartcart.local', 'demo11@smartcart.local'
)
  AND NOT EXISTS (SELECT 1 FROM cart c WHERE c.fk_user = u.id_user);

INSERT INTO cartitems (fk_cart, fk_product, fk_productvariant, quantity, price, createdat)
SELECT c.id_cart, pv.fk_product, pv.id_productvariant, s.qty, pv.sellingprice, NOW()
FROM (VALUES
    ('demo1@smartcart.local', 'SM-TATA-TEA-500', 1),
    ('demo1@smartcart.local', 'SM-AMUL-MILK-1L', 2),
    ('demo2@smartcart.local', 'SM-LAYS-52', 3),
    ('demo2@smartcart.local', 'DEMO-OREO-150', 2),
    ('demo5@smartcart.local', 'SM-PAMPERS-M20', 1),
    ('demo8@smartcart.local', 'SM-IG-RICE-5KG', 1),
    ('demo11@smartcart.local', 'DEMO-PEPSI-750', 4)
) AS s(email, sku, qty)
INNER JOIN users u ON u.email = s.email
INNER JOIN cart c ON c.fk_user = u.id_user
INNER JOIN productvariants pv ON pv.sku = s.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM cartitems ci
    WHERE ci.fk_cart = c.id_cart AND ci.fk_productvariant = pv.id_productvariant
);

INSERT INTO wishlist (fk_user, createdat, cancelled)
SELECT u.id_user, NOW(), FALSE
FROM users u
WHERE u.email LIKE 'demo%@smartcart.local'
  AND NOT EXISTS (SELECT 1 FROM wishlist w WHERE w.fk_user = u.id_user);

INSERT INTO wishlistitems (fk_wishlist, fk_product, createdat, cancelled)
SELECT w.id_wishlist, p.id_product, NOW(), FALSE
FROM (VALUES
    ('demo1@smartcart.local', 'nescafe-classic'),
    ('demo1@smartcart.local', 'dove-daily-shine-shampoo'),
    ('demo3@smartcart.local', 'pampers-baby-dry-m'),
    ('demo4@smartcart.local', 'india-gate-basmati-rice'),
    ('demo6@smartcart.local', 'mccain-french-fries'),
    ('demo7@smartcart.local', 'horlicks-classic'),
    ('demo9@smartcart.local', 'alphonso-mango'),
    ('demo10@smartcart.local', 'magnum-classic-almond'),
    ('demo12@smartcart.local', 'kelloggs-corn-flakes'),
    ('demo15@smartcart.local', 'tata-tea-gold')
) AS s(email, slug)
INNER JOIN users u ON u.email = s.email
INNER JOIN wishlist w ON w.fk_user = u.id_user AND COALESCE(w.cancelled, FALSE) = FALSE
INNER JOIN products p ON p.slug = s.slug AND p.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM wishlistitems wi
    WHERE wi.fk_wishlist = w.id_wishlist AND wi.fk_product = p.id_product AND COALESCE(wi.cancelled, FALSE) = FALSE
);

-- =============================================================================
-- Dummy orders (80) with items, payments, shipping, light stock deduct
-- =============================================================================
DO $$
DECLARE
    n INT;
    v_user_id INT;
    v_order_id INT;
    v_total NUMERIC(10,2);
    v_status TEXT;
    v_pay TEXT;
    v_pay_status TEXT;
    v_ship_status TEXT;
    v_cancelled BOOLEAN;
    v_dt TIMESTAMP;
    v_addr RECORD;
    v_line_count INT;
    i INT;
    v_sku_count INT;
    v_rn INT;
    v_pvid INT;
    v_pid INT;
    v_name TEXT;
    v_label TEXT;
    v_sku TEXT;
    v_price NUMERIC(10,2);
    v_qty INT;
    v_avail INT;
    v_stock_id INT;
    v_have INT;
    v_need INT;
    v_ship_date TIMESTAMP;
BEGIN
    IF EXISTS (SELECT 1 FROM orders WHERE ordernumber LIKE 'DM%') THEN
        RAISE NOTICE 'Demo orders already present — skipping order seed.';
        RETURN;
    END IF;

    SELECT COUNT(*) INTO v_sku_count
    FROM productvariants
    WHERE COALESCE(cancelled, FALSE) = FALSE AND isactive AND sellonline;

    IF v_sku_count < 1 THEN
        RAISE NOTICE 'No sellable SKUs — skipping orders.';
        RETURN;
    END IF;

    CREATE TEMP TABLE demo_lines (
        pid INT,
        pvid INT,
        pname TEXT,
        vlabel TEXT,
        sku TEXT,
        price NUMERIC(10,2),
        qty INT,
        linetotal NUMERIC(10,2)
    ) ON COMMIT DROP;

    FOR n IN 1..80 LOOP
        TRUNCATE demo_lines;

        SELECT id_user INTO v_user_id
        FROM users
        WHERE email = 'demo' || (((n - 1) % 18) + 1)::TEXT || '@smartcart.local';

        IF v_user_id IS NULL THEN
            CONTINUE;
        END IF;

        SELECT * INTO v_addr
        FROM useraddresses
        WHERE userid = v_user_id AND cancelled = FALSE
        ORDER BY isdefault DESC, addressid
        LIMIT 1;

        v_dt := TIMESTAMP '2026-10-06 18:00:00'
                - (((n % 48)) || ' days')::interval
                - (((n % 13)) || ' hours')::interval;

        v_pay := CASE WHEN n % 2 = 0 THEN 'Razorpay' ELSE 'COD' END;

        IF n % 10 = 0 THEN
            v_status := 'Cancelled'; v_cancelled := TRUE;
            v_pay_status := 'Cancelled'; v_ship_status := 'Cancelled';
        ELSIF n % 10 IN (1, 2) THEN
            v_status := 'Placed'; v_cancelled := FALSE;
            v_pay_status := CASE WHEN v_pay = 'Razorpay' THEN 'Paid' ELSE 'Pending' END;
            v_ship_status := 'Pending';
        ELSIF n % 10 = 3 THEN
            v_status := 'Confirmed'; v_cancelled := FALSE;
            v_pay_status := CASE WHEN v_pay = 'Razorpay' THEN 'Paid' ELSE 'Pending' END;
            v_ship_status := 'Packed';
        ELSIF n % 10 IN (4, 5) THEN
            v_status := 'Shipped'; v_cancelled := FALSE;
            v_pay_status := CASE WHEN v_pay = 'Razorpay' THEN 'Paid' ELSE 'Pending' END;
            v_ship_status := 'Shipped';
        ELSE
            v_status := 'Delivered'; v_cancelled := FALSE;
            v_pay_status := 'Paid';
            v_ship_status := 'Delivered';
        END IF;

        v_line_count := 1 + (n % 3);
        FOR i IN 0..(v_line_count - 1) LOOP
            v_rn := ((n + (i * 11) - 1) % v_sku_count) + 1;
            SELECT pv.id_productvariant, pv.fk_product, pr.name, pv.variantlabel, pv.sku, pv.sellingprice
            INTO v_pvid, v_pid, v_name, v_label, v_sku, v_price
            FROM (
                SELECT id_productvariant,
                       ROW_NUMBER() OVER (ORDER BY sku) AS rn
                FROM productvariants
                WHERE COALESCE(cancelled, FALSE) = FALSE AND isactive AND sellonline
            ) ranked
            INNER JOIN productvariants pv ON pv.id_productvariant = ranked.id_productvariant
            INNER JOIN products pr ON pr.id_product = pv.fk_product
            WHERE ranked.rn = v_rn;

            IF v_pvid IS NULL THEN
                CONTINUE;
            END IF;

            IF EXISTS (SELECT 1 FROM demo_lines d WHERE d.pvid = v_pvid) THEN
                CONTINUE;
            END IF;

            SELECT COALESCE(SUM(quantity), 0) INTO v_avail
            FROM stock
            WHERE fk_productvariant = v_pvid AND COALESCE(cancelled, FALSE) = FALSE;

            v_qty := 1 + (n % 2);
            IF v_avail < v_qty THEN
                CONTINUE;
            END IF;

            INSERT INTO demo_lines VALUES (v_pid, v_pvid, v_name, v_label, v_sku, v_price, v_qty, v_price * v_qty);
        END LOOP;

        SELECT COALESCE(SUM(linetotal), 0) INTO v_total FROM demo_lines;
        IF v_total <= 0 THEN
            CONTINUE;
        END IF;

        INSERT INTO orders (
            fk_user, orderdate, totalamount, orderstatus, shippingaddress, paymentmethod,
            cancelled, cancelledon, cancelledreason, ordernumber,
            receivername, phone, addressline, city, pincode
        )
        VALUES (
            v_user_id,
            v_dt,
            v_total,
            v_status,
            COALESCE(v_addr.addressline, '') || ', ' || COALESCE(v_addr.city, '') || ' ' || COALESCE(v_addr.pincode, ''),
            v_pay,
            v_cancelled,
            CASE WHEN v_cancelled THEN v_dt + INTERVAL '2 hours' ELSE NULL END,
            CASE WHEN v_cancelled THEN 'Customer cancelled after placing' ELSE NULL END,
            'DM' || lpad(n::TEXT, 6, '0'),
            COALESCE(v_addr.receivername, 'Customer'),
            COALESCE(v_addr.phone, '9876500000'),
            COALESCE(v_addr.addressline, 'Demo address'),
            COALESCE(v_addr.city, 'Kochi'),
            COALESCE(v_addr.pincode, '682001')
        )
        RETURNING id_order INTO v_order_id;

        INSERT INTO orderitems (
            fk_order, fk_product, fk_productvariant, productname, variantlabel, sku, unitprice, quantity, linetotal, createdat
        )
        SELECT v_order_id, pid, pvid, pname, vlabel, sku, price, qty, linetotal, v_dt
        FROM demo_lines;

        INSERT INTO payments (
            fk_order, paymentdate, paymentamount, paymentstatus, paymentmethod, cancelled,
            razorpay_order_id, razorpay_payment_id
        )
        VALUES (
            v_order_id,
            v_dt + INTERVAL '3 minutes',
            v_total,
            v_pay_status,
            v_pay,
            v_cancelled,
            CASE WHEN v_pay = 'Razorpay' THEN 'order_demo_' || lpad(n::TEXT, 6, '0') ELSE NULL END,
            CASE WHEN v_pay = 'Razorpay' AND v_pay_status = 'Paid' THEN 'pay_demo_' || lpad(n::TEXT, 6, '0') ELSE NULL END
        );

        v_ship_date := CASE
            WHEN v_status IN ('Shipped', 'Delivered') THEN v_dt + INTERVAL '1 day'
            ELSE NULL
        END;

        INSERT INTO shipping (
            fk_order, shippingaddress, shippingdate, estimateddeliverydate, shippingstatus, cancelled
        )
        VALUES (
            v_order_id,
            COALESCE(v_addr.addressline, '') || ', ' || COALESCE(v_addr.city, '') || ' ' || COALESCE(v_addr.pincode, ''),
            v_ship_date,
            v_dt + INTERVAL '3 days',
            v_ship_status,
            v_cancelled
        );

        IF NOT v_cancelled THEN
            FOR v_pvid, v_qty IN SELECT pvid, qty FROM demo_lines LOOP
                v_need := v_qty;
                WHILE v_need > 0 LOOP
                    SELECT s.id_stock, s.quantity INTO v_stock_id, v_have
                    FROM stock s
                    WHERE s.fk_productvariant = v_pvid
                      AND COALESCE(s.cancelled, FALSE) = FALSE
                      AND s.quantity > 0
                    ORDER BY s.createdon, s.id_stock
                    LIMIT 1;

                    EXIT WHEN v_stock_id IS NULL;

                    IF v_have >= v_need THEN
                        UPDATE stock SET quantity = quantity - v_need WHERE id_stock = v_stock_id;
                        v_need := 0;
                    ELSE
                        UPDATE stock SET quantity = 0 WHERE id_stock = v_stock_id;
                        v_need := v_need - v_have;
                    END IF;
                END LOOP;
            END LOOP;
        END IF;
    END LOOP;
END $$;

-- Keep Magnum / Cornetto low so admin dashboard shows low-stock
UPDATE stock s
SET quantity = 3
FROM productvariants pv
WHERE pv.id_productvariant = s.fk_productvariant
  AND pv.sku IN ('SM-CORNETTO-1', 'DEMO-MAGNUM-1')
  AND COALESCE(s.cancelled, FALSE) = FALSE
  AND s.quantity > 3;

-- =============================================================================
-- Ratings
-- =============================================================================
INSERT INTO ratings (fk_product, fk_user, ratingvalue, review, createdat, cancelled)
SELECT p.id_product, u.id_user, s.rating, s.review, NOW() - (s.daysago || ' days')::interval, FALSE
FROM (VALUES
    ('demo1@smartcart.local', 'tata-tea-gold', 5.0, 'Rich flavour. Weekly staple in our house.', 12),
    ('demo2@smartcart.local', 'lays-classic-salted', 4.0, 'Crispy, good for evening snacks.', 9),
    ('demo3@smartcart.local', 'amul-taaza-toned-milk', 5.0, 'Always fresh, delivered on time.', 8),
    ('demo4@smartcart.local', 'india-gate-basmati-rice', 5.0, 'Long grain, perfect for biryani.', 20),
    ('demo5@smartcart.local', 'pampers-baby-dry-m', 4.0, 'Holds overnight. Pack size is convenient.', 6),
    ('demo6@smartcart.local', 'maggi-2-minute-noodles', 5.0, 'Kids favourite. Fast checkout too.', 4),
    ('demo7@smartcart.local', 'dove-daily-shine-shampoo', 4.0, 'Gentle on hair, decent quantity.', 11),
    ('demo8@smartcart.local', 'shimla-apple', 5.0, 'Crisp apples, better than the local market.', 3),
    ('demo9@smartcart.local', 'coca-cola-750ml', 4.0, 'Cold drinks aisle is well stocked.', 7),
    ('demo10@smartcart.local', 'kwality-walls-cornetto', 5.0, 'Classic cone. Wish you had more flavours.', 2),
    ('demo11@smartcart.local', 'surf-excel-easy-wash', 4.0, 'Washes well in bucket soak.', 15),
    ('demo12@smartcart.local', 'nescafe-classic', 5.0, 'Morning coffee sorted.', 18),
    ('demo13@smartcart.local', 'oreo-original', 5.0, 'Kids emptied the pack in a day.', 5),
    ('demo14@smartcart.local', 'horlicks-classic', 4.0, 'Good malt taste, sealed jar.', 10),
    ('demo15@smartcart.local', 'fortune-sunflower-oil', 4.0, 'Standard cooking oil, fair price.', 14),
    ('demo16@smartcart.local', 'parle-g-original', 5.0, 'Nostalgia pack. Great with chai.', 16),
    ('demo17@smartcart.local', 'britannia-whole-wheat-bread', 3.0, 'Soft but short shelf life — use quickly.', 1),
    ('demo18@smartcart.local', 'colgate-strong-teeth', 4.0, 'Family toothpaste, value for money.', 13),
    ('demo1@smartcart.local', 'farm-fresh-tomato', 4.0, 'Ripe and good for curry.', 2),
    ('demo4@smartcart.local', 'aashirvaad-atta', 5.0, 'Soft rotis every time.', 21)
) AS s(email, slug, rating, review, daysago)
INNER JOIN users u ON u.email = s.email
INNER JOIN products p ON p.slug = s.slug AND p.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM ratings r WHERE r.fk_user = u.id_user AND r.fk_product = p.id_product
);

-- =============================================================================
-- Walk-in POS sales + one return
-- =============================================================================
INSERT INTO sales (invoicenumber, saledate, customername, customerphone, paymentmethod, totalamount, notes, createdon, cancelled)
SELECT s.invoicenumber, s.saledate::date, s.customername, s.phone, s.pay, 0, s.notes, s.saledate::timestamp, FALSE
FROM (VALUES
    ('POS-DEMO-0001', '2026-09-18 11:20:00', 'Walk-in', '9876510001', 'Cash', 'Counter sale — snacks'),
    ('POS-DEMO-0002', '2026-09-22 16:40:00', 'Walk-in', '9876510002', 'UPI', 'Counter sale — dairy'),
    ('POS-DEMO-0003', '2026-09-27 10:05:00', 'Walk-in', '9876510003', 'Cash', 'Counter sale — staples'),
    ('POS-DEMO-0004', '2026-10-01 19:15:00', 'Walk-in', '9876510004', 'Card', 'Evening rush'),
    ('POS-DEMO-0005', '2026-10-03 12:30:00', 'Walk-in', '9876510005', 'UPI', 'Lunch hour'),
    ('POS-DEMO-0006', '2026-10-04 09:10:00', 'Walk-in', '9876510006', 'Cash', 'Morning milk and bread'),
    ('POS-DEMO-0007', '2026-10-05 17:45:00', 'Walk-in', '9876510007', 'UPI', 'Household refill'),
    ('POS-DEMO-0008', '2026-10-06 11:00:00', 'Walk-in', '9876510008', 'Cash', 'Same-day counter')
) AS s(invoicenumber, saledate, customername, phone, pay, notes)
WHERE NOT EXISTS (SELECT 1 FROM sales x WHERE x.invoicenumber = s.invoicenumber);

INSERT INTO salesdetail (fk_sale, fk_productvariant, quantity, sellingprice, mrp, createdon, cancelled)
SELECT sl.id_sale, pv.id_productvariant, x.qty, pv.sellingprice, pv.mrp, sl.createdon, FALSE
FROM (VALUES
    ('POS-DEMO-0001', 'SM-LAYS-52', 4),
    ('POS-DEMO-0001', 'DEMO-KURKURE-90', 2),
    ('POS-DEMO-0002', 'SM-AMUL-MILK-1L', 3),
    ('POS-DEMO-0002', 'SM-EGGS-12', 1),
    ('POS-DEMO-0003', 'SM-TATA-SALT-1KG', 2),
    ('POS-DEMO-0003', 'SM-AASH-ATTA-5KG', 1),
    ('POS-DEMO-0004', 'SM-COKE-750', 6),
    ('POS-DEMO-0004', 'DEMO-PEPSI-750', 4),
    ('POS-DEMO-0005', 'SM-MAGGI-12', 1),
    ('POS-DEMO-0005', 'DEMO-OREO-150', 2),
    ('POS-DEMO-0006', 'SM-BRIT-BREAD-400', 2),
    ('POS-DEMO-0006', 'SM-AMUL-MILK-1L', 2),
    ('POS-DEMO-0007', 'SM-SURF-1KG', 1),
    ('POS-DEMO-0007', 'SM-VIM-500', 1),
    ('POS-DEMO-0008', 'SM-TATA-TEA-500', 1),
    ('POS-DEMO-0008', 'DEMO-BRU-100', 1)
) AS x(invoicenumber, sku, qty)
INNER JOIN sales sl ON sl.invoicenumber = x.invoicenumber
INNER JOIN productvariants pv ON pv.sku = x.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM salesdetail sd
    WHERE sd.fk_sale = sl.id_sale AND sd.fk_productvariant = pv.id_productvariant
);

UPDATE sales sl
SET totalamount = x.totalamount
FROM (
    SELECT fk_sale, SUM(sellingprice * quantity) AS totalamount
    FROM salesdetail WHERE cancelled = FALSE GROUP BY fk_sale
) x
WHERE x.fk_sale = sl.id_sale AND sl.invoicenumber LIKE 'POS-DEMO-%';

INSERT INTO salesreturn (fk_sale, invoicenumber, returndate, totalamount, reason, notes, createdon, cancelled)
SELECT sl.id_sale, 'RET-DEMO-0001', DATE '2026-10-02', 0,
       'Opened pack damaged in bag', 'Dummy return for reports', TIMESTAMP '2026-10-02 14:00:00', FALSE
FROM sales sl
WHERE sl.invoicenumber = 'POS-DEMO-0001'
  AND NOT EXISTS (SELECT 1 FROM salesreturn r WHERE r.invoicenumber = 'RET-DEMO-0001');

INSERT INTO salesreturndetail (fk_salesreturn, fk_salesdetail, fk_productvariant, quantity, sellingprice, createdon, cancelled)
SELECT r.id_salesreturn, sd.id_salesdetail, sd.fk_productvariant, 1, sd.sellingprice, r.createdon, FALSE
FROM salesreturn r
INNER JOIN sales sl ON sl.id_sale = r.fk_sale
INNER JOIN salesdetail sd ON sd.fk_sale = sl.id_sale
INNER JOIN productvariants pv ON pv.id_productvariant = sd.fk_productvariant
WHERE r.invoicenumber = 'RET-DEMO-0001'
  AND pv.sku = 'SM-LAYS-52'
  AND NOT EXISTS (
      SELECT 1 FROM salesreturndetail d WHERE d.fk_salesreturn = r.id_salesreturn AND d.fk_salesdetail = sd.id_salesdetail
  );

UPDATE salesreturn r
SET totalamount = x.totalamount
FROM (
    SELECT fk_salesreturn, SUM(sellingprice * quantity) AS totalamount
    FROM salesreturndetail WHERE cancelled = FALSE GROUP BY fk_salesreturn
) x
WHERE x.fk_salesreturn = r.id_salesreturn AND r.invoicenumber = 'RET-DEMO-0001';

-- =============================================================================
-- Product status lookup + light audit trail
-- =============================================================================
INSERT INTO productstatus (statusname, cancelled)
SELECT s.statusname, FALSE
FROM (VALUES ('Active'), ('Inactive'), ('Out of Stock'), ('Discontinued')) AS s(statusname)
WHERE NOT EXISTS (SELECT 1 FROM productstatus p WHERE p.statusname = s.statusname);

INSERT INTO auditlogs (action, fk_user, tablename, recordid, changedetails, createdat, cancelled)
SELECT s.action, u.id_user, s.tablename, COALESCE((SELECT MIN(id_order) FROM orders WHERE fk_user = u.id_user), u.id_user),
       s.details, NOW() - (s.daysago || ' days')::interval, FALSE
FROM (VALUES
    ('demo1@smartcart.local', 'PLACE_ORDER', 'orders', 'Placed online order via Razorpay', 2),
    ('demo2@smartcart.local', 'PLACE_ORDER', 'orders', 'Placed online order via COD', 5),
    ('demo4@smartcart.local', 'UPDATE_ADDRESS', 'useraddresses', 'Saved default home address', 20),
    ('demo8@smartcart.local', 'WISHLIST_ADD', 'wishlistitems', 'Added product to wishlist', 3),
    ('demo12@smartcart.local', 'CART_ADD', 'cartitems', 'Added items to cart', 1)
) AS s(email, action, tablename, details, daysago)
INNER JOIN users u ON u.email = s.email
WHERE NOT EXISTS (
    SELECT 1 FROM auditlogs a WHERE a.fk_user = u.id_user AND a.action = s.action AND a.tablename = s.tablename
);

DROP TABLE IF EXISTS catalog_demo;

\echo 'Complete dummy data seed finished.'
\echo 'Shop logins: demo1@smartcart.local … demo18@smartcart.local / Admin@123'
\echo 'Staff logins: manager, cashier / Admin@123'
