\echo 'Seeding supermarket catalog...'

INSERT INTO category (name, description, isactive, cancelled)
SELECT s.name, s.description, TRUE, FALSE
FROM (VALUES
    ('Fresh Produce', 'Fruits and vegetables'),
    ('Dairy & Eggs', 'Milk, curd, butter, cheese, and eggs'),
    ('Bakery', 'Bread, buns, and biscuits'),
    ('Grocery Staples', 'Rice, flour, oil, salt, spices, and noodles'),
    ('Snacks & Beverages', 'Chips, soft drinks, tea, coffee, and water'),
    ('Frozen Foods', 'Ice cream and frozen snacks'),
    ('Household', 'Laundry and kitchen cleaning'),
    ('Baby Care', 'Diapers and baby essentials'),
    ('Personal Care', 'Skincare, haircare, and daily wellness products')
) AS s(name, description)
WHERE NOT EXISTS (
    SELECT 1 FROM category c WHERE c.name = s.name
);

INSERT INTO brand (brandname, description, isactive, cancelled)
SELECT s.brandname, s.description, TRUE, FALSE
FROM (VALUES
    ('SmartCart Fresh', 'Store-brand fresh produce'),
    ('Amul', 'GCMMF dairy'),
    ('Nestle', 'Nestle dairy and beverages'),
    ('Britannia', 'Britannia bakery'),
    ('Parle', 'Parle biscuits and snacks'),
    ('India Gate', 'India Gate rice'),
    ('Aashirvaad', 'ITC Aashirvaad atta'),
    ('Fortune', 'Adani Wilmar edible oil'),
    ('Tata', 'Tata salt and tea'),
    ('Everest', 'Everest spices'),
    ('Maggi', 'Nestle Maggi noodles'),
    ('Lays', 'PepsiCo Lays chips'),
    ('Coca-Cola', 'Coca-Cola beverages'),
    ('Nescafe', 'Nestle Nescafe coffee'),
    ('Bisleri', 'Bisleri packaged water'),
    ('Kwality Walls', 'HUL ice cream'),
    ('McCain', 'McCain frozen snacks'),
    ('Colgate', 'Colgate oral care'),
    ('Dove', 'HUL Dove personal care'),
    ('Surf Excel', 'HUL laundry detergent'),
    ('Vim', 'HUL dishwash'),
    ('Pampers', 'P&G Pampers'),
    ('Himalaya', 'Himalaya Wellness personal care')
) AS s(brandname, description)
WHERE NOT EXISTS (
    SELECT 1 FROM brand b WHERE b.brandname = s.brandname
);

INSERT INTO supplier (
    name, companyname, email, phone,
    state, district, city, address, pincode,
    description, isactive, createdat, cancelled
)
SELECT
    'SmartCart Wholesale',
    'SmartCart Wholesale Pvt Ltd',
    'wholesale@smartcart.local',
    '9876501122',
    'Kerala', 'Ernakulam', 'Kochi',
    'Warehouse 2, Kalamassery Industrial Estate',
    '683104',
    'Primary grocery wholesaler for supermarket opening stock.',
    TRUE, NOW(), FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM supplier s WHERE s.email = 'wholesale@smartcart.local'
);

INSERT INTO variants (name, description, displayorder, isactive, cancelled)
SELECT 'Pack Size', 'Grocery pack or weight', 10, TRUE, FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM variants v WHERE v.name = 'Pack Size'
);

INSERT INTO variantvalues (fk_variant, name, description, displayorder, cancelled)
SELECT v.id_variant, s.name, s.description, s.displayorder, FALSE
FROM variants v
CROSS JOIN (VALUES
    ('1 kg', '1 kilogram', 1),
    ('5 kg', '5 kilograms', 2),
    ('500 g', '500 grams', 3),
    ('200 g', '200 grams', 4),
    ('100 g', '100 grams', 5),
    ('400 g', '400 grams', 6),
    ('800 g', '800 grams', 7),
    ('1 L', '1 litre', 8),
    ('750 ml', '750 millilitres', 9),
    ('340 ml', '340 millilitres', 10),
    ('1 dozen', '12 pieces', 11),
    ('12 pcs', 'Pack of 12', 12),
    ('20 pcs', 'Pack of 20', 13),
    ('52 g', '52 grams', 14),
    ('1 piece', 'Single unit', 15)
) AS s(name, description, displayorder)
WHERE v.name = 'Pack Size'
  AND COALESCE(v.cancelled, FALSE) = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM variantvalues vv
      WHERE vv.fk_variant = v.id_variant AND vv.name = s.name
  );

INSERT INTO subcategory (name, description, fk_category, isactive, cancelled)
SELECT s.name, s.description, c.id_category, TRUE, FALSE
FROM (VALUES
    ('Fruits', 'Fresh fruits', 'Fresh Produce'),
    ('Vegetables', 'Fresh vegetables', 'Fresh Produce'),
    ('Milk', 'Packaged milk', 'Dairy & Eggs'),
    ('Butter & Cheese', 'Butter, cheese, and spreads', 'Dairy & Eggs'),
    ('Curd & Yogurt', 'Dahi and yogurt', 'Dairy & Eggs'),
    ('Eggs', 'Farm eggs', 'Dairy & Eggs'),
    ('Bread', 'Fresh bread', 'Bakery'),
    ('Biscuits', 'Packed biscuits', 'Bakery'),
    ('Rice & Grains', 'Rice and grains', 'Grocery Staples'),
    ('Flour & Sugar', 'Atta, maida, and sugar', 'Grocery Staples'),
    ('Oils', 'Cooking oils', 'Grocery Staples'),
    ('Spices', 'Masala and spices', 'Grocery Staples'),
    ('Noodles', 'Instant noodles', 'Grocery Staples'),
    ('Chips', 'Potato chips', 'Snacks & Beverages'),
    ('Soft Drinks', 'Carbonated drinks', 'Snacks & Beverages'),
    ('Tea & Coffee', 'Tea and coffee', 'Snacks & Beverages'),
    ('Water', 'Packaged drinking water', 'Snacks & Beverages'),
    ('Ice Cream', 'Ice cream and cones', 'Frozen Foods'),
    ('Frozen Snacks', 'Ready-to-cook frozen food', 'Frozen Foods'),
    ('Oral Care', 'Toothpaste and oral care', 'Personal Care'),
    ('Hair Care', 'Shampoo and hair care', 'Personal Care'),
    ('Laundry', 'Detergent and fabric care', 'Household'),
    ('Cleaning', 'Dishwash and surface cleaners', 'Household'),
    ('Diapers', 'Baby diapers', 'Baby Care')
) AS s(name, description, categoryname)
INNER JOIN category c ON c.name = s.categoryname AND c.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM subcategory sc
    WHERE sc.name = s.name AND sc.fk_category = c.id_category
);

CREATE TEMP TABLE catalog (
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

INSERT INTO catalog VALUES
('Fruits', 'Fresh Produce', 'SmartCart Fresh', 'Shimla Apple', 'shimla-apple', 'Crisp red apples, sold loose by weight.', 'SM-APPLE-1KG', '8902000000011', '1 kg', 180.00, 149.00, 110.00, 'kg', 1, 48, 'https://images.unsplash.com/photo-1560806887-1e4cd0b21054?auto=format&fit=crop&w=800&q=80'),
('Fruits', 'Fresh Produce', 'SmartCart Fresh', 'Robusta Banana', 'robusta-banana', 'Everyday robusta bananas, one dozen.', 'SM-BANANA-12', '8902000000028', '1 dozen', 70.00, 54.00, 38.00, 'dozen', 1, 60, 'https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Farm Fresh Tomato', 'farm-fresh-tomato', 'Ripe cooking tomatoes.', 'SM-TOMATO-1KG', '8902000000035', '1 kg', 50.00, 36.00, 24.00, 'kg', 1, 72, 'https://images.unsplash.com/photo-1546470427-e26264be0f40?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Farm Fresh Onion', 'farm-fresh-onion', 'Nashik red onions.', 'SM-ONION-1KG', '8902000000042', '1 kg', 45.00, 32.00, 22.00, 'kg', 1, 80, 'https://images.unsplash.com/photo-1508747703725-719777637510?auto=format&fit=crop&w=800&q=80'),
('Vegetables', 'Fresh Produce', 'SmartCart Fresh', 'Farm Fresh Potato', 'farm-fresh-potato', 'Table potatoes for daily cooking.', 'SM-POTATO-1KG', '8902000000059', '1 kg', 40.00, 28.00, 18.00, 'kg', 1, 90, 'https://images.unsplash.com/photo-1518977676601-b53f82aba655?auto=format&fit=crop&w=800&q=80'),
('Milk', 'Dairy & Eggs', 'Amul', 'Amul Taaza Toned Milk', 'amul-taaza-toned-milk', 'Toned milk, tetra pack, 1 litre.', 'SM-AMUL-MILK-1L', '8902000000066', '1 L', 74.00, 68.00, 58.00, 'L', 1, 120, 'https://images.unsplash.com/photo-1563636619-e9143da7973b?auto=format&fit=crop&w=800&q=80'),
('Butter & Cheese', 'Dairy & Eggs', 'Amul', 'Amul Butter', 'amul-butter', 'Salted table butter.', 'SM-AMUL-BUTTER-500', '8902000000073', '500 g', 295.00, 275.00, 230.00, 'g', 500, 36, 'https://images.unsplash.com/photo-1589985270826-4b7bb135bc9d?auto=format&fit=crop&w=800&q=80'),
('Curd & Yogurt', 'Dairy & Eggs', 'Nestle', 'Nestle A+ Dahi', 'nestle-a-plus-dahi', 'Thick set curd, 400 g cup.', 'SM-NESTLE-DAHI-400', '8902000000080', '400 g', 55.00, 48.00, 36.00, 'g', 400, 40, 'https://images.unsplash.com/photo-1488477181946-6428a0291777?auto=format&fit=crop&w=800&q=80'),
('Eggs', 'Dairy & Eggs', 'SmartCart Fresh', 'Farm Fresh Eggs', 'farm-fresh-eggs', 'White eggs, tray of 12.', 'SM-EGGS-12', '8902000000097', '12 pcs', 96.00, 84.00, 66.00, 'pcs', 12, 55, 'https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?auto=format&fit=crop&w=800&q=80'),
('Bread', 'Bakery', 'Britannia', 'Britannia Whole Wheat Bread', 'britannia-whole-wheat-bread', 'Soft whole wheat sandwich bread.', 'SM-BRIT-BREAD-400', '8902000000103', '400 g', 55.00, 48.00, 34.00, 'g', 400, 28, 'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=800&q=80'),
('Biscuits', 'Bakery', 'Parle', 'Parle-G Original Glucose', 'parle-g-original', 'Classic glucose biscuits, family pack.', 'SM-PARLE-G-800', '8902000000110', '800 g', 90.00, 80.00, 62.00, 'g', 800, 44, 'https://images.unsplash.com/photo-1558961363-fa8fdf82db35?auto=format&fit=crop&w=800&q=80'),
('Rice & Grains', 'Grocery Staples', 'India Gate', 'India Gate Basmati Rice', 'india-gate-basmati-rice', 'Aged basmati rice for daily cooking.', 'SM-IG-RICE-1KG', '8902000000127', '1 kg', 185.00, 159.00, 128.00, 'kg', 1, 32, 'https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=800&q=80'),
('Rice & Grains', 'Grocery Staples', 'India Gate', 'India Gate Basmati Rice', 'india-gate-basmati-rice', 'Aged basmati rice for daily cooking.', 'SM-IG-RICE-5KG', '8902000000134', '5 kg', 890.00, 749.00, 610.00, 'kg', 5, 18, 'https://images.unsplash.com/photo-1586201375761-83865001e31c?auto=format&fit=crop&w=800&q=80'),
('Flour & Sugar', 'Grocery Staples', 'Aashirvaad', 'Aashirvaad Superior MP Atta', 'aashirvaad-atta', 'Whole wheat atta for rotis.', 'SM-AASH-ATTA-5KG', '8902000000141', '5 kg', 310.00, 275.00, 230.00, 'kg', 5, 24, 'https://images.unsplash.com/photo-1574323347407-f5e1ad6d020b?auto=format&fit=crop&w=800&q=80'),
('Oils', 'Grocery Staples', 'Fortune', 'Fortune Sunflower Oil', 'fortune-sunflower-oil', 'Refined sunflower oil, 1 litre.', 'SM-FORT-OIL-1L', '8902000000158', '1 L', 175.00, 152.00, 128.00, 'L', 1, 30, 'https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?auto=format&fit=crop&w=800&q=80'),
('Flour & Sugar', 'Grocery Staples', 'Tata', 'Tata Salt', 'tata-salt', 'Iodised vacuum-evaporated salt.', 'SM-TATA-SALT-1KG', '8902000000165', '1 kg', 32.00, 28.00, 20.00, 'kg', 1, 70, 'https://images.unsplash.com/photo-1518110925495-5fe2ade03c8b?auto=format&fit=crop&w=800&q=80'),
('Spices', 'Grocery Staples', 'Everest', 'Everest Turmeric Powder', 'everest-turmeric', 'Pure haldi powder.', 'SM-EVE-HALDI-200', '8902000000172', '200 g', 72.00, 62.00, 48.00, 'g', 200, 38, 'https://images.unsplash.com/photo-1615485290382-441e4d049cbd?auto=format&fit=crop&w=800&q=80'),
('Noodles', 'Grocery Staples', 'Maggi', 'Maggi 2-Minute Noodles', 'maggi-2-minute-noodles', 'Masala instant noodles, pack of 12.', 'SM-MAGGI-12', '8902000000189', '12 pcs', 168.00, 144.00, 118.00, 'pcs', 12, 50, 'https://images.unsplash.com/photo-1612929633738-8fe44f4ec141?auto=format&fit=crop&w=800&q=80'),
('Chips', 'Snacks & Beverages', 'Lays', 'Lays Classic Salted', 'lays-classic-salted', 'Classic salted potato chips.', 'SM-LAYS-52', '8902000000196', '52 g', 20.00, 18.00, 13.00, 'g', 52, 96, 'https://images.unsplash.com/photo-1566478989037-eec170784d0b?auto=format&fit=crop&w=800&q=80'),
('Soft Drinks', 'Snacks & Beverages', 'Coca-Cola', 'Coca-Cola', 'coca-cola-750ml', 'Chilled cola, 750 ml PET.', 'SM-COKE-750', '8902000000202', '750 ml', 45.00, 40.00, 30.00, 'ml', 750, 64, 'https://images.unsplash.com/photo-1629203851122-3726ecdf080e?auto=format&fit=crop&w=800&q=80'),
('Tea & Coffee', 'Snacks & Beverages', 'Tata', 'Tata Tea Gold', 'tata-tea-gold', 'Rich leaf tea, 500 g.', 'SM-TATA-TEA-500', '8902000000219', '500 g', 310.00, 279.00, 230.00, 'g', 500, 26, 'https://images.unsplash.com/photo-1564890369478-c89ca4d9f3f9?auto=format&fit=crop&w=800&q=80'),
('Tea & Coffee', 'Snacks & Beverages', 'Nescafe', 'Nescafe Classic', 'nescafe-classic', 'Soluble coffee, glass jar.', 'SM-NESCAFE-100', '8902000000226', '100 g', 385.00, 345.00, 290.00, 'g', 100, 22, 'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?auto=format&fit=crop&w=800&q=80'),
('Water', 'Snacks & Beverages', 'Bisleri', 'Bisleri Packaged Water', 'bisleri-water-1l', 'Packaged drinking water, 1 litre.', 'SM-BISLERI-1L', '8902000000233', '1 L', 20.00, 18.00, 12.00, 'L', 1, 140, 'https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=800&q=80'),
('Ice Cream', 'Frozen Foods', 'Kwality Walls', 'Kwality Walls Cornetto', 'kwality-walls-cornetto', 'Chocolate cone ice cream.', 'SM-CORNETTO-1', '8902000000240', '1 piece', 50.00, 40.00, 28.00, 'pcs', 1, 8, 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=800&q=80'),
('Frozen Snacks', 'Frozen Foods', 'McCain', 'McCain French Fries', 'mccain-french-fries', 'Shoestring fries, 750 g.', 'SM-MCCAIN-FRIES-750', '8902000000257', '800 g', 210.00, 189.00, 150.00, 'g', 750, 16, 'https://images.unsplash.com/photo-1573080496219-bb080318a755?auto=format&fit=crop&w=800&q=80'),
('Oral Care', 'Personal Care', 'Colgate', 'Colgate Strong Teeth', 'colgate-strong-teeth', 'Family toothpaste, 200 g.', 'SM-COLGATE-200', '8902000000264', '200 g', 135.00, 118.00, 92.00, 'g', 200, 34, 'https://images.unsplash.com/photo-1559591937-abc3a5d02748?auto=format&fit=crop&w=800&q=80'),
('Hair Care', 'Personal Care', 'Dove', 'Dove Daily Shine Shampoo', 'dove-daily-shine-shampoo', 'Daily shine shampoo, 340 ml.', 'SM-DOVE-SHAMP-340', '8902000000271', '340 ml', 285.00, 249.00, 198.00, 'ml', 340, 20, 'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?auto=format&fit=crop&w=800&q=80'),
('Laundry', 'Household', 'Surf Excel', 'Surf Excel Easy Wash', 'surf-excel-easy-wash', 'Detergent powder, 1 kg.', 'SM-SURF-1KG', '8902000000288', '1 kg', 168.00, 149.00, 118.00, 'kg', 1, 27, 'https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?auto=format&fit=crop&w=800&q=80'),
('Cleaning', 'Household', 'Vim', 'Vim Dishwash Liquid', 'vim-dishwash-liquid', 'Lemon dishwash liquid, 500 ml.', 'SM-VIM-500', '8902000000295', '500 g', 125.00, 109.00, 84.00, 'ml', 500, 31, 'https://images.unsplash.com/photo-1563453392212-326f5e854473?auto=format&fit=crop&w=800&q=80'),
('Diapers', 'Baby Care', 'Pampers', 'Pampers Baby Dry M', 'pampers-baby-dry-m', 'Mid-size baby diapers, pack of 20.', 'SM-PAMPERS-M20', '8902000000301', '20 pcs', 399.00, 349.00, 280.00, 'pcs', 20, 14, 'https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?auto=format&fit=crop&w=800&q=80');

INSERT INTO products (fk_subcategory, fk_brand, name, slug, description, isactive, sellonline, createdat, cancelled)
SELECT
    sc.id_subcategory,
    b.id_brand,
    MIN(p.productname),
    p.slug,
    MIN(p.productdesc),
    TRUE,
    TRUE,
    NOW(),
    FALSE
FROM catalog p
INNER JOIN category c ON c.name = p.categoryname AND c.cancelled = FALSE
INNER JOIN subcategory sc ON sc.name = p.subcategory AND sc.fk_category = c.id_category AND COALESCE(sc.cancelled, FALSE) = FALSE
INNER JOIN brand b ON b.brandname = p.brandname AND b.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM products x WHERE x.slug = p.slug
)
GROUP BY p.slug, sc.id_subcategory, b.id_brand;

INSERT INTO productvariants (
    fk_product, sku, barcode, variantlabel, description,
    mrp, sellingprice, unitofmeasure, unitvalue,
    isdefault, maxorderqty, isactive, sellonline, createdat, cancelled
)
SELECT
    pr.id_product,
    c.sku,
    c.barcode,
    c.packsize,
    c.productname || ', ' || c.packsize,
    c.mrp,
    c.sellingprice,
    c.uom,
    c.unitvalue,
    CASE WHEN c.sku = 'SM-IG-RICE-5KG' THEN FALSE ELSE TRUE END,
    20,
    TRUE,
    TRUE,
    NOW(),
    FALSE
FROM catalog c
INNER JOIN products pr ON pr.slug = c.slug AND pr.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM productvariants v WHERE v.sku = c.sku
);

INSERT INTO productvariantattributes (fk_productvariant, fk_variant, fk_variantvalue, description)
SELECT
    pv.id_productvariant,
    v.id_variant,
    vv.id_variantvalue,
    'Pack Size'
FROM catalog c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
INNER JOIN variants v ON v.name = 'Pack Size' AND COALESCE(v.cancelled, FALSE) = FALSE
INNER JOIN variantvalues vv ON vv.fk_variant = v.id_variant AND vv.name = c.packsize AND COALESCE(vv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1
    FROM productvariantattributes a
    WHERE a.fk_productvariant = pv.id_productvariant AND a.fk_variant = v.id_variant
);

INSERT INTO productmedia (fk_product, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT
    pr.id_product,
    'Image',
    MIN(c.imageurl),
    0,
    TRUE,
    NOW()
FROM catalog c
INNER JOIN products pr ON pr.slug = c.slug AND pr.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM productmedia m WHERE m.fk_product = pr.id_product
)
GROUP BY pr.id_product;

INSERT INTO skumedia (fk_productsku, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT
    pv.id_productvariant,
    'Image',
    c.imageurl,
    0,
    TRUE,
    NOW()
FROM catalog c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM skumedia m WHERE m.fk_productsku = pv.id_productvariant
);

INSERT INTO productmedia (fk_product, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT p.id_product, 'Image', x.imageurl, 0, TRUE, NOW()
FROM (VALUES
    ('iphone-15', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    ('samsung-galaxy-a35', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80'),
    ('boat-airdopes-141', 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=800&q=80'),
    ('prestige-pic-20-induction-cooktop', 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=800&q=80'),
    ('himalaya-purifying-neem-face-wash-150ml', 'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=800&q=80')
) AS x(slug, imageurl)
INNER JOIN products p ON p.slug = x.slug AND p.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM productmedia m WHERE m.fk_product = p.id_product
);

INSERT INTO skumedia (fk_productsku, mediatype, mediaurl, displayorder, isprimary, createdat)
SELECT pv.id_productvariant, 'Image', x.imageurl, 0, TRUE, NOW()
FROM (VALUES
    ('IP15-BLK-128', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    ('IP15-BLU-256', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80'),
    ('SMA35-NVY-128', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80'),
    ('BOAT-AD141-BLK', 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=800&q=80'),
    ('PRE-PIC20-2000W', 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=800&q=80'),
    ('HIM-NEEM-FW-150', 'https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=800&q=80')
) AS x(sku, imageurl)
INNER JOIN productvariants pv ON pv.sku = x.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM skumedia m WHERE m.fk_productsku = pv.id_productvariant
);

INSERT INTO purchase (fk_supplier, purchasedate, invoicenumber, totalamount, notes, createdon, cancelled)
SELECT
    s.id_supplier,
    DATE '2026-08-20',
    'INV-SMKT-26001',
    COALESCE((SELECT SUM(c.cost * c.stockqty) FROM catalog c), 0),
    'Supermarket opening stock — grocery aisles',
    TIMESTAMP '2026-08-20 09:00:00',
    FALSE
FROM supplier s
WHERE s.email = 'wholesale@smartcart.local'
  AND s.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM purchase p WHERE p.invoicenumber = 'INV-SMKT-26001'
  );

INSERT INTO purchasedetail (fk_purchase, fk_productvariant, quantity, purchaseprice, mrp, createdon, cancelled)
SELECT
    p.id_purchase,
    pv.id_productvariant,
    c.stockqty,
    c.cost,
    c.mrp,
    TIMESTAMP '2026-08-20 09:00:00',
    FALSE
FROM catalog c
INNER JOIN productvariants pv ON pv.sku = c.sku AND COALESCE(pv.cancelled, FALSE) = FALSE
INNER JOIN purchase p ON p.invoicenumber = 'INV-SMKT-26001'
WHERE NOT EXISTS (
    SELECT 1
    FROM purchasedetail pd
    WHERE pd.fk_purchase = p.id_purchase AND pd.fk_productvariant = pv.id_productvariant
);

INSERT INTO stock (fk_purchasedetail, fk_productvariant, quantity, createdon, cancelled)
SELECT
    pd.id_purchasedetail,
    pd.fk_productvariant,
    pd.quantity,
    TIMESTAMP '2026-08-20 09:00:00',
    FALSE
FROM purchasedetail pd
INNER JOIN purchase p ON p.id_purchase = pd.fk_purchase
WHERE p.invoicenumber = 'INV-SMKT-26001'
  AND NOT EXISTS (
      SELECT 1 FROM stock s WHERE s.fk_purchasedetail = pd.id_purchasedetail
  );

UPDATE purchase p
SET totalamount = x.totalamount
FROM (
    SELECT fk_purchase, SUM(purchaseprice * quantity) AS totalamount
    FROM purchasedetail
    WHERE cancelled = FALSE
    GROUP BY fk_purchase
) x
WHERE x.fk_purchase = p.id_purchase
  AND p.invoicenumber = 'INV-SMKT-26001';

\echo 'Supermarket catalog seed complete.'
