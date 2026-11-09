-- =============================================================================
-- SmartCartPro | product_catalog_dummy_data.sql
-- Module: Product Catalog (Categories, Subcategories, Products)
-- Target: PostgreSQL 14+
-- Scope: TEST / DUMMY DATA ONLY
--
-- Description:
--   Populates a realistic product catalog dataset designed for testing the
--   SmartCartPro Product Catalog module:
--     - 10 Categories
--     - 35 Subcategories
--     - 100 Supermarket Products (Indian / Kerala retail FMCG staples)
--
-- Safety & Idempotency:
--   - Safe to rerun without generating duplicate records.
--   - Does NOT delete, truncate, or overwrite existing business data.
--   - Does NOT reset identity sequences.
--   - Does NOT create new Brand, Unit, Tax, Supplier, Company, Store, or User records.
--   - Existing Brands, Units, and Taxes are linked dynamically if present;
--     otherwise foreign keys evaluate safely to NULL (allowed by schema).
--   - Does NOT generate stock movements or inventory balances.
--   - Wrapped in a transaction (BEGIN / COMMIT).
-- =============================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ═════════════════════════════════════════════════════════════════════════════
-- 1. CATEGORIES (10 Categories)
-- ═════════════════════════════════════════════════════════════════════════════

INSERT INTO categories (categorycode, categoryname, description, displayorder, isactive)
SELECT v.categorycode, v.categoryname, v.description, v.displayorder, TRUE
FROM (VALUES
    ('CAT-RGP', 'Rice, Grains & Pulses', 'Rice, wheat, flours, dals, pulses, and whole grains.', 10),
    ('CAT-COG', 'Cooking Oils & Ghee', 'Refined oils, coconut oil, mustard oil, olive oil, and desi ghee.', 20),
    ('CAT-SPM', 'Spices & Masalas', 'Whole spices, powdered spices, blended curry powders, and pastes.', 30),
    ('CAT-SNB', 'Snacks & Biscuits', 'Biscuits, cookies, namkeen, savouries, chips, and instant noodles.', 40),
    ('CAT-BEV', 'Beverages', 'Tea, coffee, health food drinks, fruit juices, and soft drinks.', 50),
    ('CAT-DNB', 'Dairy & Bakery', 'Milk, curd, butter, cheese, fresh bread, rusk, and farm eggs.', 60),
    ('CAT-PER', 'Personal Care', 'Bath soaps, shampoos, toothpastes, and skin lotions.', 70),
    ('CAT-HMC', 'Home Cleaning', 'Laundry detergents, dishwash bars/gels, surface cleaners, and fresheners.', 80),
    ('CAT-FNV', 'Fruits & Vegetables', 'Fresh daily vegetables, root crops, onions, and seasonal fruits.', 90),
    ('CAT-SHE', 'Stationery & Household Essentials', 'School/office stationery, aluminium foil, kitchen tissues, and repellents.', 100)
) AS v(categorycode, categoryname, description, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM categories c
    WHERE c.categoryname = v.categoryname AND c.cancelled = FALSE
);


-- ═════════════════════════════════════════════════════════════════════════════
-- 2. SUBCATEGORIES (35 Subcategories across the 10 Categories)
-- ═════════════════════════════════════════════════════════════════════════════

INSERT INTO subcategories (fk_category, subcategoryname, description, displayorder, isactive)
SELECT c.id_category, v.subcategoryname, v.description, v.displayorder, TRUE
FROM (VALUES
    -- Rice, Grains & Pulses (4 subcategories)
    ('Rice, Grains & Pulses', 'Rice & Rice Products', 'Basmati, matta, raw, and boiled rice varieties.', 10),
    ('Rice, Grains & Pulses', 'Atta & Flours', 'Whole wheat atta, maida, sooji rava, puttu podi, and rice flour.', 20),
    ('Rice, Grains & Pulses', 'Dals & Pulses', 'Toor dal, moong dal, chana dal, urad dal, and rajma.', 30),
    ('Rice, Grains & Pulses', 'Grains & Millets', 'Oats, millets, poha, and ancient whole grains.', 40),

    -- Cooking Oils & Ghee (4 subcategories)
    ('Cooking Oils & Ghee', 'Sunflower & Refined Oils', 'Refined sunflower, soybean, and blended edible oils.', 10),
    ('Cooking Oils & Ghee', 'Coconut & Mustard Oils', 'Pure coconut oil, kachi ghani mustard oil, and gingelly oil.', 20),
    ('Cooking Oils & Ghee', 'Olive & Speciality Oils', 'Extra virgin olive oil, pure olive oil, and sesame oil.', 30),
    ('Cooking Oils & Ghee', 'Pure Desi Ghee', 'Traditional cow and buffalo desi ghee.', 40),

    -- Spices & Masalas (4 subcategories)
    ('Spices & Masalas', 'Whole Spices', 'Black pepper, cardamom, cloves, cinnamon, and cumin seeds.', 10),
    ('Spices & Masalas', 'Powdered Spices', 'Chilli powder, turmeric powder, coriander powder, and pepper powder.', 20),
    ('Spices & Masalas', 'Curry & Meat Masalas', 'Chicken masala, meat masala, sambar powder, and garam masala.', 30),
    ('Spices & Masalas', 'Cooking Pastes & Herbs', 'Ginger-garlic paste, tamarind paste, and culinary herbs.', 40),

    -- Snacks & Biscuits (4 subcategories)
    ('Snacks & Biscuits', 'Biscuits & Cookies', 'Glucose biscuits, cream biscuits, butter cookies, and rusks.', 10),
    ('Snacks & Biscuits', 'Namkeen & Savouries', 'Bhujia, mixture, roasted nuts, and traditional savouries.', 20),
    ('Snacks & Biscuits', 'Chips & Crisps', 'Potato chips, banana chips, and extruded corn snacks.', 30),
    ('Snacks & Biscuits', 'Noodles & Pasta', 'Instant noodles, macaroni, pasta, and vermicelli.', 40),

    -- Beverages (4 subcategories)
    ('Beverages', 'Tea & Chai', 'CTC leaf tea, dust tea, and green tea.', 10),
    ('Beverages', 'Coffee & Chicory', 'Instant coffee powders and filter coffee blends.', 20),
    ('Beverages', 'Health Drinks & Malt', 'Malt-based nutrition drinks and chocolate health powders.', 30),
    ('Beverages', 'Juices & Soft Drinks', 'Packaged fruit juices, nectars, and carbonated beverages.', 40),

    -- Dairy & Bakery (4 subcategories)
    ('Dairy & Bakery', 'Fresh Milk & Curd', 'Pasteurised toned milk, full cream milk, and set curd.', 10),
    ('Dairy & Bakery', 'Butter & Cheese', 'Table butter, processed cheese slices, cubes, and fresh paneer.', 20),
    ('Dairy & Bakery', 'Fresh Breads & Rusk', 'White sandwich bread, whole wheat bread, and tea rusks.', 30),
    ('Dairy & Bakery', 'Eggs', 'Farm fresh white and brown table eggs.', 40),

    -- Personal Care (4 subcategories)
    ('Personal Care', 'Bath & Body Soaps', 'Bathing soap bars and antibacterial soaps.', 10),
    ('Personal Care', 'Hair Care & Shampoos', 'Anti-dandruff and daily care shampoos.', 20),
    ('Personal Care', 'Oral Care & Toothpaste', 'Dental toothpastes, tooth powders, and freshness gels.', 30),
    ('Personal Care', 'Skin Care & Lotions', 'Body lotions, cold creams, and light moisturizers.', 40),

    -- Home Cleaning (4 subcategories)
    ('Home Cleaning', 'Laundry Detergents & Bars', 'Washing powders, liquid detergents, and laundry bars.', 10),
    ('Home Cleaning', 'Dishwash Liquids & Bars', 'Dishwash bars, gels, and scrub cleaners.', 20),
    ('Home Cleaning', 'Surface & Toilet Cleaners', 'Floor disinfectants, toilet bowl cleaners, and glass cleaners.', 30),
    ('Home Cleaning', 'Air Fresheners & Repellents', 'Room freshener blocks, sprays, and mosquito repellents.', 40),

    -- Fruits & Vegetables (3 subcategories)
    ('Fruits & Vegetables', 'Daily Vegetables', 'Tomatoes, green chillies, carrots, beans, and fresh greens.', 10),
    ('Fruits & Vegetables', 'Root Vegetables & Onions', 'Potatoes, onions, ginger, and garlic.', 20),
    ('Fruits & Vegetables', 'Fresh Fruits', 'Bananas, apples, oranges, and seasonal table fruits.', 30),

    -- Stationery & Household Essentials (3 subcategories)
    ('Stationery & Household Essentials', 'Writing Instruments & Pens', 'Lead pencils, ballpoint pens, and erasers.', 10),
    ('Stationery & Household Essentials', 'Notebooks & Paper', 'School exercise notebooks and spiral writing pads.', 20),
    ('Stationery & Household Essentials', 'Kitchen Foil & Tissues', 'Aluminium foil rolls, kitchen tissue paper, and scrub sponges.', 30)
) AS v(categoryname, subcategoryname, description, displayorder)
JOIN categories c ON c.categoryname = v.categoryname AND c.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM subcategories s
    WHERE s.fk_category = c.id_category
      AND s.subcategoryname = v.subcategoryname
      AND s.cancelled = FALSE
);


-- ═════════════════════════════════════════════════════════════════════════════
-- 3. PRODUCTS (100 Realistic Supermarket Products)
-- ═════════════════════════════════════════════════════════════════════════════

CREATE TEMP TABLE tmp_dummy_products (
    productcode     TEXT PRIMARY KEY,
    productname     TEXT NOT NULL,
    categoryname    TEXT NOT NULL,
    subcategoryname TEXT NOT NULL,
    brandname       TEXT,
    unitcode        TEXT,
    taxcode         TEXT,
    costprice       NUMERIC(12,2) NOT NULL,
    sellingprice    NUMERIC(12,2) NOT NULL,
    mrp             NUMERIC(12,2),
    reorderlevel    NUMERIC(12,3) NOT NULL DEFAULT 10,
    sku             TEXT NOT NULL,
    barcode         TEXT,
    description     TEXT
) ON COMMIT DROP;

INSERT INTO tmp_dummy_products (
    productcode, productname, categoryname, subcategoryname, brandname, unitcode, taxcode,
    costprice, sellingprice, mrp, reorderlevel, sku, barcode, description
) VALUES
-- ── Category 1: Rice, Grains & Pulses (10 items) ────────────────────────────
('PRD-CAT-001', 'India Gate Basmati Rice Classic 1kg', 'Rice, Grains & Pulses', 'Rice & Rice Products', 'India Gate', 'PKT', 'GST5', 145.00, 180.00, 210.00, 15, 'SKU-CAT-001', 'TEST-BC-10001', 'Premium aged long-grain basmati rice for biryani and pulao.'),
('PRD-CAT-002', 'Double Horse Vadi Matta Rice 5kg', 'Rice, Grains & Pulses', 'Rice & Rice Products', 'Double Horse', 'PKT', 'GST5', 240.00, 295.00, 330.00, 12, 'SKU-CAT-002', 'TEST-BC-10002', 'Traditional Kerala parboiled red matta rice.'),
('PRD-CAT-003', 'Fortune Everyday Biryani Basmati Rice 1kg', 'Rice, Grains & Pulses', 'Rice & Rice Products', 'Fortune', 'PKT', 'GST5', 85.00, 110.00, 130.00, 20, 'SKU-CAT-003', 'TEST-BC-10003', 'Daily-use basmati rice with delicate aroma and fluffy texture.'),
('PRD-CAT-004', 'Aashirvaad Superior MP Sharbati Atta 5kg', 'Rice, Grains & Pulses', 'Atta & Flours', 'Aashirvaad', 'PKT', 'GST5', 235.00, 285.00, 320.00, 15, 'SKU-CAT-004', 'TEST-BC-10004', '100% pure whole wheat flour ground from golden MP Sharbati grains.'),
('PRD-CAT-005', 'Double Horse Roasted Rava 1kg', 'Rice, Grains & Pulses', 'Atta & Flours', 'Double Horse', 'PKT', 'GST5', 55.00, 72.00, 85.00, 10, 'SKU-CAT-005', 'TEST-BC-10005', 'Pre-roasted sooji semolina for upma, kesari, and rava dosa.'),
('PRD-CAT-006', 'Double Horse Easy Puttu Podi 1kg', 'Rice, Grains & Pulses', 'Atta & Flours', 'Double Horse', 'PKT', 'GST5', 60.00, 78.00, 90.00, 10, 'SKU-CAT-006', 'TEST-BC-10006', 'Steam-roasted coarse rice flour for soft Kerala puttu.'),
('PRD-CAT-007', 'Tata Sampann Unpolished Toor Dal 1kg', 'Rice, Grains & Pulses', 'Dals & Pulses', 'Tata', 'PKT', 'GST5', 140.00, 175.00, 199.00, 12, 'SKU-CAT-007', 'TEST-BC-10007', 'Unpolished yellow arhar toor dal rich in natural protein.'),
('PRD-CAT-008', 'Tata Sampann Unpolished Chana Dal 1kg', 'Rice, Grains & Pulses', 'Dals & Pulses', 'Tata', 'PKT', 'GST5', 95.00, 120.00, 140.00, 12, 'SKU-CAT-008', 'TEST-BC-10008', 'Unpolished nutritious Bengal gram chana dal.'),
('PRD-CAT-009', '24 Mantra Organic Moong Dal 500g', 'Rice, Grains & Pulses', 'Dals & Pulses', 'Tata', 'PKT', 'GST5', 75.00, 98.00, 115.00, 10, 'SKU-CAT-009', 'TEST-BC-10009', 'Certified organic split yellow moong lentils.'),
('PRD-CAT-010', '24 Mantra Organic Foxtail Millet 500g', 'Rice, Grains & Pulses', 'Grains & Millets', 'Tata', 'PKT', 'GST5', 52.00, 68.00, 80.00, 8, 'SKU-CAT-010', 'TEST-BC-10010', 'Nutrient-dense organic unpolished foxtail millet grain.'),

-- ── Category 2: Cooking Oils & Ghee (10 items) ─────────────────────────────
('PRD-CAT-011', 'Fortune Sunlite Refined Sunflower Oil 1L Pouch', 'Cooking Oils & Ghee', 'Sunflower & Refined Oils', 'Fortune', 'PKT', 'GST5', 115.00, 138.00, 160.00, 24, 'SKU-CAT-011', 'TEST-BC-10011', 'Light and healthy refined sunflower oil enriched with vitamins A & D.'),
('PRD-CAT-012', 'Fortune Sunlite Refined Sunflower Oil 5L Can', 'Cooking Oils & Ghee', 'Sunflower & Refined Oils', 'Fortune', 'BTL', 'GST5', 580.00, 685.00, 780.00, 8, 'SKU-CAT-012', 'TEST-BC-10012', 'Economy 5 litre jerrycan refined sunflower cooking oil.'),
('PRD-CAT-013', 'Saffola Gold Pro Healthy Edible Oil 1L', 'Cooking Oils & Ghee', 'Sunflower & Refined Oils', 'Fortune', 'BTL', 'GST5', 148.00, 179.00, 210.00, 12, 'SKU-CAT-013', 'TEST-BC-10013', 'Dual-seed blend of rice bran and sunflower oil with antioxidants.'),
('PRD-CAT-014', 'Parachute Pure Coconut Oil 1L Bottle', 'Cooking Oils & Ghee', 'Coconut & Mustard Oils', 'Parachute', 'BTL', 'GST5', 210.00, 255.00, 290.00, 15, 'SKU-CAT-014', 'TEST-BC-10014', '100% pure edible expeller-pressed coconut oil in bottle.'),
('PRD-CAT-015', 'KLF Coconad Pure Coconut Oil 1L Pouch', 'Cooking Oils & Ghee', 'Coconut & Mustard Oils', 'Parachute', 'PKT', 'GST5', 195.00, 235.00, 265.00, 15, 'SKU-CAT-015', 'TEST-BC-10015', 'Traditional roasted aroma coconut oil in stay-fresh pouch.'),
('PRD-CAT-016', 'Dhara Kachi Ghani Mustard Oil 1L', 'Cooking Oils & Ghee', 'Coconut & Mustard Oils', 'Fortune', 'BTL', 'GST5', 135.00, 165.00, 190.00, 10, 'SKU-CAT-016', 'TEST-BC-10016', 'Cold-pressed pungent kachi ghani mustard cooking oil.'),
('PRD-CAT-017', 'Figaro Pure Olive Oil Tin 500ml', 'Cooking Oils & Ghee', 'Olive & Speciality Oils', 'Green Basket House', 'BTL', 'GST5', 480.00, 575.00, 650.00, 6, 'SKU-CAT-017', 'TEST-BC-10017', 'Mild Spanish pure olive oil suitable for sautéing and baking.'),
('PRD-CAT-018', 'Borges Extra Virgin Olive Oil 500ml', 'Cooking Oils & Ghee', 'Olive & Speciality Oils', 'Green Basket House', 'BTL', 'GST5', 540.00, 649.00, 725.00, 6, 'SKU-CAT-018', 'TEST-BC-10018', 'First cold-pressed extra virgin olive oil for salads and dressings.'),
('PRD-CAT-019', 'Amul Pure Ghee Jar 1L', 'Cooking Oils & Ghee', 'Pure Desi Ghee', 'Kochi Dairy Demo', 'BTL', 'GST12', 520.00, 610.00, 670.00, 10, 'SKU-CAT-019', 'TEST-BC-10019', 'Pure clarified milk fat desi ghee with rich granular texture.'),
('PRD-CAT-020', 'Milma Pure Ghee Jar 500ml', 'Cooking Oils & Ghee', 'Pure Desi Ghee', 'Kochi Dairy Demo', 'BTL', 'GST12', 285.00, 340.00, 375.00, 12, 'SKU-CAT-020', 'TEST-BC-10020', 'Authentic cooperative dairy cow ghee from Kerala farmers.'),

-- ── Category 3: Spices & Masalas (10 items) ────────────────────────────────
('PRD-CAT-021', 'Everest Garam Masala 100g', 'Spices & Masalas', 'Curry & Meat Masalas', 'Green Basket House', 'PKT', 'GST5', 62.00, 78.00, 90.00, 12, 'SKU-CAT-021', 'TEST-BC-10021', 'Fragrant blend of 13 aromatic spices for rich North & South curries.'),
('PRD-CAT-022', 'Eastern Chicken Masala 100g', 'Spices & Masalas', 'Curry & Meat Masalas', 'Green Basket House', 'PKT', 'GST5', 38.00, 48.00, 55.00, 20, 'SKU-CAT-022', 'TEST-BC-10022', 'Authentic spice mix formulation for traditional chicken curries.'),
('PRD-CAT-023', 'Eastern Meat Masala 100g', 'Spices & Masalas', 'Curry & Meat Masalas', 'Green Basket House', 'PKT', 'GST5', 40.00, 50.00, 58.00, 15, 'SKU-CAT-023', 'TEST-BC-10023', 'Robust spice blend designed for mutton, beef, and spicy roasts.'),
('PRD-CAT-024', 'Eastern Sambar Powder 100g', 'Spices & Masalas', 'Curry & Meat Masalas', 'Green Basket House', 'PKT', 'GST5', 35.00, 44.00, 50.00, 18, 'SKU-CAT-024', 'TEST-BC-10024', 'Roasted lentils and aromatic spices for authentic Kerala sambar.'),
('PRD-CAT-025', 'Brahmins Rasam Powder 100g', 'Spices & Masalas', 'Curry & Meat Masalas', 'Green Basket House', 'PKT', 'GST5', 36.00, 45.00, 52.00, 15, 'SKU-CAT-025', 'TEST-BC-10025', 'Piquant peppery blend for quick South Indian rasam preparation.'),
('PRD-CAT-026', 'Eastern Kashmiri Chilli Powder 250g', 'Spices & Masalas', 'Powdered Spices', 'Green Basket House', 'PKT', 'GST5', 95.00, 120.00, 140.00, 12, 'SKU-CAT-026', 'TEST-BC-10026', 'Mild heat red chilli powder giving vibrant natural ruby red color.'),
('PRD-CAT-027', 'Everest Turmeric Powder 200g', 'Spices & Masalas', 'Powdered Spices', 'Green Basket House', 'PKT', 'GST5', 48.00, 62.00, 72.00, 15, 'SKU-CAT-027', 'TEST-BC-10027', 'High-curcumin golden yellow ground turmeric powder.'),
('PRD-CAT-028', 'Catch Coriander Powder 200g', 'Spices & Masalas', 'Powdered Spices', 'Green Basket House', 'PKT', 'GST5', 55.00, 70.00, 82.00, 12, 'SKU-CAT-028', 'TEST-BC-10028', 'Cool-ground fragrant green coriander seed powder.'),
('PRD-CAT-029', 'Catch Black Pepper Whole 100g', 'Spices & Masalas', 'Whole Spices', 'Green Basket House', 'PKT', 'GST5', 82.00, 105.00, 125.00, 10, 'SKU-CAT-029', 'TEST-BC-10029', 'Sun-dried high-piperine Malabar black peppercorns.'),
('PRD-CAT-030', 'Eastern Ginger Garlic Paste 200g', 'Spices & Masalas', 'Cooking Pastes & Herbs', 'Green Basket House', 'PKT', 'GST12', 38.00, 48.00, 55.00, 15, 'SKU-CAT-030', 'TEST-BC-10030', 'Thick aromatic crushed ginger and garlic cooking paste in pouch.'),

-- ── Category 4: Snacks & Biscuits (10 items) ───────────────────────────────
('PRD-CAT-031', 'Parle-G Gold Gluco Biscuits 250g', 'Snacks & Biscuits', 'Biscuits & Cookies', 'Green Basket House', 'PKT', 'GST12', 22.00, 28.00, 30.00, 25, 'SKU-CAT-031', 'TEST-BC-10031', 'Classic milk and wheat energy biscuits loved across generations.'),
('PRD-CAT-032', 'Britannia Good Day Cashew Biscuits 200g', 'Snacks & Biscuits', 'Biscuits & Cookies', 'Green Basket House', 'PKT', 'GST12', 34.00, 45.00, 50.00, 20, 'SKU-CAT-032', 'TEST-BC-10032', 'Rich butter-baked cookies packed with crunchy roasted cashew nuts.'),
('PRD-CAT-033', 'Britannia Marie Gold Biscuits 300g', 'Snacks & Biscuits', 'Biscuits & Cookies', 'Green Basket House', 'PKT', 'GST12', 28.00, 38.00, 42.00, 20, 'SKU-CAT-033', 'TEST-BC-10033', 'Light and crisp tea-time biscuits packed with 10 essential vitamins.'),
('PRD-CAT-034', 'Sunfeast Dark Fantasy Choco Fills 300g', 'Snacks & Biscuits', 'Biscuits & Cookies', 'Green Basket House', 'BOX', 'GST18', 95.00, 125.00, 150.00, 10, 'SKU-CAT-034', 'TEST-BC-10034', 'Indulgent baked crust cookies filled with luscious molten chocolate.'),
('PRD-CAT-035', 'Oreo Original Vanilla Creme Biscuits 120g', 'Snacks & Biscuits', 'Biscuits & Cookies', 'Green Basket House', 'PKT', 'GST18', 28.00, 36.00, 40.00, 15, 'SKU-CAT-035', 'TEST-BC-10035', 'Crunchy embossed dark cocoa biscuits sandwiching sweet vanilla cream.'),
('PRD-CAT-036', 'Haldirams Nagpur Aloo Bhujia 200g', 'Snacks & Biscuits', 'Namkeen & Savouries', 'Green Basket House', 'PKT', 'GST12', 44.00, 56.00, 65.00, 15, 'SKU-CAT-036', 'TEST-BC-10036', 'Crispy spicy potato gram flour sev seasoned with mint and spices.'),
('PRD-CAT-037', 'Haldirams All in One Mixture 200g', 'Snacks & Biscuits', 'Namkeen & Savouries', 'Green Basket House', 'PKT', 'GST12', 44.00, 56.00, 65.00, 15, 'SKU-CAT-037', 'TEST-BC-10037', 'Traditional savoury mix of cornflakes, nuts, lentils, and crispy sev.'),
('PRD-CAT-038', 'Lays Indias Magic Masala Potato Chips 50g', 'Snacks & Biscuits', 'Chips & Crisps', 'Green Basket House', 'PKT', 'GST12', 15.00, 20.00, 20.00, 30, 'SKU-CAT-038', 'TEST-BC-10038', 'Thinly sliced crispy potatoes tossed in spicy Indian street masala.'),
('PRD-CAT-039', 'Kurkure Masala Munch Crisps 85g', 'Snacks & Biscuits', 'Chips & Crisps', 'Green Basket House', 'PKT', 'GST12', 15.00, 20.00, 20.00, 25, 'SKU-CAT-039', 'TEST-BC-10039', 'Crunchy extruded corn and rice spirals coated with chatpata spices.'),
('PRD-CAT-040', 'Maggi 2-Minute Masala Instant Noodles 280g', 'Snacks & Biscuits', 'Noodles & Pasta', 'Green Basket House', 'PKT', 'GST12', 48.00, 58.00, 64.00, 20, 'SKU-CAT-040', 'TEST-BC-10040', 'Pack of 4 instant wheat noodle cakes with signature tastemaker seasoning.'),

-- ── Category 5: Beverages (10 items) ───────────────────────────────────────
('PRD-CAT-041', 'Tata Tea Gold Leaf Tea 250g', 'Beverages', 'Tea & Chai', 'Tata', 'PKT', 'GST5', 110.00, 135.00, 155.00, 15, 'SKU-CAT-041', 'TEST-BC-10041', 'Exquisite balance of strong CTC tea and gently rolled aromatic long leaves.'),
('PRD-CAT-042', 'Brooke Bond Red Label Tea 500g', 'Beverages', 'Tea & Chai', 'Brooke Bond', 'PKT', 'GST5', 185.00, 230.00, 260.00, 12, 'SKU-CAT-042', 'TEST-BC-10042', 'Classic daily CTC black tea blend offering strong color and taste.'),
('PRD-CAT-043', 'Wagh Bakri Premium CTC Tea 500g', 'Beverages', 'Tea & Chai', 'Brooke Bond', 'PKT', 'GST5', 195.00, 245.00, 275.00, 10, 'SKU-CAT-043', 'TEST-BC-10043', 'Selected tea granules delivering brisk liquor and inviting aroma.'),
('PRD-CAT-044', 'Bru Instant Coffee Powder 100g Pouch', 'Beverages', 'Coffee & Chicory', 'Green Basket House', 'PKT', 'GST5', 145.00, 178.00, 200.00, 12, 'SKU-CAT-044', 'TEST-BC-10044', 'Fine blend of 70% coffee beans and 30% roasted chicory for rich froth.'),
('PRD-CAT-045', 'Nescafe Classic Instant Coffee Jar 100g', 'Beverages', 'Coffee & Chicory', 'Nescafe', 'BTL', 'GST18', 240.00, 295.00, 335.00, 8, 'SKU-CAT-045', 'TEST-BC-10045', '100% pure roasted robusta coffee beans giving stimulating bold aroma.'),
('PRD-CAT-046', 'Horlicks Classic Malt Refill 500g', 'Beverages', 'Health Drinks & Malt', 'Green Basket House', 'PKT', 'GST18', 195.00, 242.00, 270.00, 12, 'SKU-CAT-046', 'TEST-BC-10046', 'Malted barley nutrition powder packed with micro-nutrients for kids.'),
('PRD-CAT-047', 'Boost Energy Drink Refill 500g', 'Beverages', 'Health Drinks & Malt', 'Green Basket House', 'PKT', 'GST18', 215.00, 265.00, 295.00, 10, 'SKU-CAT-047', 'TEST-BC-10047', 'Chocolate flavored malt health drink with 3x stamina boosters.'),
('PRD-CAT-048', 'Cadbury Bournvita Health Drink 500g', 'Beverages', 'Health Drinks & Malt', 'Green Basket House', 'PKT', 'GST18', 205.00, 255.00, 285.00, 10, 'SKU-CAT-048', 'TEST-BC-10048', 'Cocoa and vitamin-enriched nutrition drink powder for active growth.'),
('PRD-CAT-049', 'Real Fruit Power Mixed Fruit Juice 1L', 'Beverages', 'Juices & Soft Drinks', 'Green Basket House', 'BOX', 'GST12', 95.00, 120.00, 135.00, 12, 'SKU-CAT-049', 'TEST-BC-10049', 'Wholesome blend of 9 handpicked fruits in multi-layer aseptic pack.'),
('PRD-CAT-050', 'Coca-Cola Original Soft Drink Bottle 750ml', 'Beverages', 'Juices & Soft Drinks', 'Green Basket House', 'BTL', 'GST18', 30.00, 40.00, 40.00, 24, 'SKU-CAT-050', 'TEST-BC-10050', 'Refreshing carbonated soft drink served chilled.'),

-- ── Category 6: Dairy & Bakery (10 items) ──────────────────────────────────
('PRD-CAT-051', 'Milma Toned Homogenised Milk 500ml', 'Dairy & Bakery', 'Fresh Milk & Curd', 'Kochi Dairy Demo', 'PKT', 'GST5', 25.00, 28.00, 28.00, 30, 'SKU-CAT-051', 'TEST-BC-10051', 'Pasteurised 3% fat toned milk pouch from local Kerala dairy cooperatives.'),
('PRD-CAT-052', 'Milma Rich Full Cream Milk 500ml', 'Dairy & Bakery', 'Fresh Milk & Curd', 'Kochi Dairy Demo', 'PKT', 'GST5', 29.00, 33.00, 33.00, 20, 'SKU-CAT-052', 'TEST-BC-10052', 'Wholesome 6% fat full cream milk ideal for desserts and thick tea.'),
('PRD-CAT-053', 'Milma Fresh Set Curd 500g Tub', 'Dairy & Bakery', 'Fresh Milk & Curd', 'Kochi Dairy Demo', 'BOX', 'GST5', 32.00, 40.00, 42.00, 15, 'SKU-CAT-053', 'TEST-BC-10053', 'Thick natural set curd prepared from fresh pasteurised toned milk.'),
('PRD-CAT-054', 'Amul Pasteurised Salted Butter 100g', 'Dairy & Bakery', 'Butter & Cheese', 'Kochi Dairy Demo', 'PCS', 'GST12', 46.00, 56.00, 58.00, 20, 'SKU-CAT-054', 'TEST-BC-10054', 'Creamy salted cow and buffalo milk table butter block.'),
('PRD-CAT-055', 'Amul Fresh Malai Paneer 200g', 'Dairy & Bakery', 'Butter & Cheese', 'Kochi Dairy Demo', 'PKT', 'GST5', 72.00, 89.00, 95.00, 12, 'SKU-CAT-055', 'TEST-BC-10055', 'Vacuum-packed tender and spongy cottage cheese cubes.'),
('PRD-CAT-056', 'Amul Processed Cheese Slices 200g', 'Dairy & Bakery', 'Butter & Cheese', 'Kochi Dairy Demo', 'PKT', 'GST12', 115.00, 142.00, 155.00, 10, 'SKU-CAT-056', 'TEST-BC-10056', 'Individually wrapped pasteurised processed cheddar cheese slices.'),
('PRD-CAT-057', 'Modern White Sandwich Bread 400g', 'Dairy & Bakery', 'Fresh Breads & Rusk', 'Green Basket House', 'PKT', 'GST5', 30.00, 38.00, 40.00, 15, 'SKU-CAT-057', 'TEST-BC-10057', 'Soft sliced white sandwich loaf baked fresh daily.'),
('PRD-CAT-058', 'Britannia 100% Whole Wheat Bread 400g', 'Dairy & Bakery', 'Fresh Breads & Rusk', 'Green Basket House', 'PKT', 'GST5', 38.00, 48.00, 52.00, 12, 'SKU-CAT-058', 'TEST-BC-10058', 'Nutritious brown loaf made with 100% whole wheat flour and dietary fiber.'),
('PRD-CAT-059', 'Elite Crispy Milk Rusk 200g', 'Dairy & Bakery', 'Fresh Breads & Rusk', 'Green Basket House', 'PKT', 'GST12', 32.00, 42.00, 48.00, 15, 'SKU-CAT-059', 'TEST-BC-10059', 'Double-baked crunchy sweet milk toast rusk for dipping in hot tea.'),
('PRD-CAT-060', 'Farm Fresh White Table Eggs Pack of 6', 'Dairy & Bakery', 'Eggs', 'Green Basket House', 'BOX', 'GST0', 36.00, 48.00, 52.00, 15, 'SKU-CAT-060', 'TEST-BC-10060', 'Graded clean white poultry eggs in protective paper pulp carton.'),

-- ── Category 7: Personal Care (10 items) ───────────────────────────────────
('PRD-CAT-061', 'Dettol Original Germ Protection Soap 125g', 'Personal Care', 'Bath & Body Soaps', 'Dove', 'PCS', 'GST18', 42.00, 54.00, 60.00, 20, 'SKU-CAT-061', 'TEST-BC-10061', 'Antibacterial bathing soap bar with pine fragrance for hygiene.'),
('PRD-CAT-062', 'Dove Cream Beauty Bathing Bar 100g', 'Personal Care', 'Bath & Body Soaps', 'Dove', 'PCS', 'GST18', 52.00, 68.00, 75.00, 18, 'SKU-CAT-062', 'TEST-BC-10062', 'Gentle skin cleanser containing 1/4th moisturizing cream.'),
('PRD-CAT-063', 'Pears Pure & Gentle Soap Bar 125g', 'Personal Care', 'Bath & Body Soaps', 'Dove', 'PCS', 'GST18', 56.00, 72.00, 80.00, 15, 'SKU-CAT-063', 'TEST-BC-10063', 'Glycerine-rich transparent amber bath bar for sensitive skin.'),
('PRD-CAT-064', 'Lifebuoy Total 10 Soap Bar 125g', 'Personal Care', 'Bath & Body Soaps', 'Dove', 'PCS', 'GST18', 28.00, 36.00, 40.00, 20, 'SKU-CAT-064', 'TEST-BC-10064', 'Active silver formula bathing bar protecting against 10 infection germs.'),
('PRD-CAT-065', 'Head & Shoulders Anti Dandruff Shampoo 180ml', 'Personal Care', 'Hair Care & Shampoos', 'Dove', 'BTL', 'GST18', 140.00, 178.00, 200.00, 10, 'SKU-CAT-065', 'TEST-BC-10065', 'Zinc pyrithione formula shampoo eliminating visible flakes and itch.'),
('PRD-CAT-066', 'Clinic Plus Strong & Long Health Shampoo 175ml', 'Personal Care', 'Hair Care & Shampoos', 'Dove', 'BTL', 'GST18', 82.00, 105.00, 120.00, 15, 'SKU-CAT-066', 'TEST-BC-10066', 'Milk protein infused shampoo nourishing hair from root to tip.'),
('PRD-CAT-067', 'Colgate Strong Teeth Dental Paste 150g', 'Personal Care', 'Oral Care & Toothpaste', 'Colgate', 'PCS', 'GST18', 75.00, 96.00, 110.00, 15, 'SKU-CAT-067', 'TEST-BC-10067', 'Amino Shakti formula adding natural calcium to strengthen tooth enamel.'),
('PRD-CAT-068', 'Close Up Everfresh Red Hot Gel Toothpaste 150g', 'Personal Care', 'Oral Care & Toothpaste', 'Colgate', 'PCS', 'GST18', 80.00, 102.00, 115.00, 15, 'SKU-CAT-068', 'TEST-BC-10068', 'Spicy clove red gel toothpaste delivering up to 12 hours fresh breath.'),
('PRD-CAT-069', 'Nivea Soft Light Moisturiser Cream 100ml', 'Personal Care', 'Skin Care & Lotions', 'Dove', 'BOX', 'GST18', 145.00, 185.00, 210.00, 8, 'SKU-CAT-069', 'TEST-BC-10069', 'Fast-absorbing non-greasy cream with jojoba oil and vitamin E.'),
('PRD-CAT-070', 'Vaseline Intensive Care Deep Moisture Lotion 100ml', 'Personal Care', 'Skin Care & Lotions', 'Dove', 'BTL', 'GST18', 95.00, 122.00, 139.00, 10, 'SKU-CAT-070', 'TEST-BC-10070', 'Micro-droplets of Vaseline jelly restoring dry and dehydrated skin.'),

-- ── Category 8: Home Cleaning (10 items) ───────────────────────────────────
('PRD-CAT-071', 'Surf Excel Easy Wash Detergent Powder 1kg', 'Home Cleaning', 'Laundry Detergents & Bars', 'Surf Excel', 'PKT', 'GST18', 105.00, 132.00, 150.00, 15, 'SKU-CAT-071', 'TEST-BC-10071', 'Superfine washing powder designed to remove tough stains effortlessly.'),
('PRD-CAT-072', 'Ariel Matic Front Load Detergent Powder 1kg', 'Home Cleaning', 'Laundry Detergents & Bars', 'Surf Excel', 'BOX', 'GST18', 210.00, 265.00, 299.00, 8, 'SKU-CAT-072', 'TEST-BC-10072', 'Low-suds automated washing machine powder with active enzymatic cleaning.'),
('PRD-CAT-073', 'Rin Advanced Detergent Bar 250g', 'Home Cleaning', 'Laundry Detergents & Bars', 'Surf Excel', 'PCS', 'GST18', 16.00, 22.00, 25.00, 25, 'SKU-CAT-073', 'TEST-BC-10073', 'Bright clean laundry bar for collar and cuff stain removal.'),
('PRD-CAT-074', 'Vim Lemon Dishwash Bar 200g', 'Home Cleaning', 'Dishwash Liquids & Bars', 'Vim', 'PCS', 'GST18', 18.00, 24.00, 26.00, 30, 'SKU-CAT-074', 'TEST-BC-10074', 'Grease-cutting dish cleaning bar formulated with real lemon juice.'),
('PRD-CAT-075', 'Vim Lemon Dishwash Gel Bottle 500ml', 'Home Cleaning', 'Dishwash Liquids & Bars', 'Vim', 'BTL', 'GST18', 85.00, 110.00, 125.00, 12, 'SKU-CAT-075', 'TEST-BC-10075', 'Concentrated liquid dish soap removing burnt residues without scratches.'),
('PRD-CAT-076', 'Harpic Power Plus Toilet Cleaner Original 500ml', 'Home Cleaning', 'Surface & Toilet Cleaners', 'Green Basket House', 'BTL', 'GST18', 74.00, 94.00, 105.00, 12, 'SKU-CAT-076', 'TEST-BC-10076', 'Thick acidic formula killing 99.9% germs and removing yellow limescale.'),
('PRD-CAT-077', 'Lizol Disinfectant Floor Cleaner Citrus 500ml', 'Home Cleaning', 'Surface & Toilet Cleaners', 'Green Basket House', 'BTL', 'GST18', 78.00, 99.00, 112.00, 12, 'SKU-CAT-077', 'TEST-BC-10077', 'Antibacterial multi-surface cleaner leaving fresh citrus fragrance.'),
('PRD-CAT-078', 'Colin Glass and Surface Cleaner Spray 500ml', 'Home Cleaning', 'Surface & Toilet Cleaners', 'Green Basket House', 'BTL', 'GST18', 76.00, 98.00, 110.00, 10, 'SKU-CAT-078', 'TEST-BC-10078', 'Streak-free shine spray for windows, mirrors, tabletops, and appliances.'),
('PRD-CAT-079', 'Odonil Room Air Freshener Block Mystic Rose 50g', 'Home Cleaning', 'Air Fresheners & Repellents', 'Green Basket House', 'PCS', 'GST18', 44.00, 56.00, 62.00, 15, 'SKU-CAT-079', 'TEST-BC-10079', 'Long-lasting fragrant bathroom and wardrobe odor-neutralizing block.'),
('PRD-CAT-080', 'All Out Ultra Mosquito Repellent Refill 45ml', 'Home Cleaning', 'Air Fresheners & Repellents', 'Green Basket House', 'BTL', 'GST18', 65.00, 82.00, 92.00, 15, 'SKU-CAT-080', 'TEST-BC-10080', 'Liquid vaporiser refill providing 60 nights mosquito knockdown action.'),

-- ── Category 9: Fruits & Vegetables (10 items) ─────────────────────────────
('PRD-CAT-081', 'Fresh Local Tomato (Thakkali) 1kg', 'Fruits & Vegetables', 'Daily Vegetables', 'Green Basket House', 'KG', 'GST0', 28.00, 38.00, 45.00, 20, 'SKU-CAT-081', 'TEST-BC-10081', 'Farm-fresh ripe red firm salad and cooking tomatoes.'),
('PRD-CAT-082', 'Fresh Green Chilli 250g', 'Fruits & Vegetables', 'Daily Vegetables', 'Green Basket House', 'PKT', 'GST0', 16.00, 24.00, 30.00, 15, 'SKU-CAT-082', 'TEST-BC-10082', 'Pungent fresh green chillies packed in breathable punnet.'),
('PRD-CAT-083', 'Fresh Orange Carrot 500g', 'Fruits & Vegetables', 'Daily Vegetables', 'Green Basket House', 'PKT', 'GST0', 24.00, 34.00, 40.00, 15, 'SKU-CAT-083', 'TEST-BC-10083', 'Washed and graded sweet crunchy orange salad carrots.'),
('PRD-CAT-084', 'Fresh Curry Leaves (Kariveppila) 100g', 'Fruits & Vegetables', 'Daily Vegetables', 'Green Basket House', 'PKT', 'GST0', 8.00, 14.00, 18.00, 15, 'SKU-CAT-084', 'TEST-BC-10084', 'Fresh plucked fragrant dark green curry leaves for tempering.'),
('PRD-CAT-085', 'Fresh Big Onion (Savala) 1kg', 'Fruits & Vegetables', 'Root Vegetables & Onions', 'Green Basket House', 'KG', 'GST0', 32.00, 44.00, 50.00, 25, 'SKU-CAT-085', 'TEST-BC-10085', 'Dry cured pinkish red cooking onions with firm layers.'),
('PRD-CAT-086', 'Fresh Potato (Urulaikizhangu) 1kg', 'Fruits & Vegetables', 'Root Vegetables & Onions', 'Green Basket House', 'KG', 'GST0', 28.00, 38.00, 45.00, 25, 'SKU-CAT-086', 'TEST-BC-10086', 'Clean earthy medium-sized baking and boiling table potatoes.'),
('PRD-CAT-087', 'Fresh Ginger (Inchi) 250g', 'Fruits & Vegetables', 'Root Vegetables & Onions', 'Green Basket House', 'PKT', 'GST0', 30.00, 42.00, 50.00, 12, 'SKU-CAT-087', 'TEST-BC-10087', 'Aromatic juicy washed rhizome ginger root.'),
('PRD-CAT-088', 'Fresh Garlic (Veluthulli) 250g', 'Fruits & Vegetables', 'Root Vegetables & Onions', 'Green Basket House', 'PKT', 'GST0', 45.00, 60.00, 70.00, 12, 'SKU-CAT-088', 'TEST-BC-10088', 'Peeled outer skin firm aromatic white garlic cloves in net bag.'),
('PRD-CAT-089', 'Fresh Kerala Nendran Banana 1kg', 'Fruits & Vegetables', 'Fresh Fruits', 'Green Basket House', 'KG', 'GST0', 48.00, 64.00, 72.00, 15, 'SKU-CAT-089', 'TEST-BC-10089', 'Golden sweet Kerala plantain bananas ideal for steaming and chips.'),
('PRD-CAT-090', 'Fresh Royal Gala Apple 1kg Pack', 'Fruits & Vegetables', 'Fresh Fruits', 'Green Basket House', 'PKT', 'GST0', 140.00, 185.00, 210.00, 10, 'SKU-CAT-090', 'TEST-BC-10090', 'Crisp sweet thin-skinned imported dessert apples.'),

-- ── Category 10: Stationery & Household Essentials (10 items) ─────────────
('PRD-CAT-091', 'Nataraj HB 621 Lead Pencils Pack of 10', 'Stationery & Household Essentials', 'Writing Instruments & Pens', 'Green Basket House', 'BOX', 'GST12', 42.00, 55.00, 60.00, 15, 'SKU-CAT-091', 'TEST-BC-10091', 'Smooth dark bonded lead graphite pencils with eraser and sharpener.'),
('PRD-CAT-092', 'Cello Butterflow Classic Blue Ball Pen 5 Pcs', 'Stationery & Household Essentials', 'Writing Instruments & Pens', 'Green Basket House', 'PKT', 'GST12', 38.00, 48.00, 50.00, 20, 'SKU-CAT-092', 'TEST-BC-10092', 'Low-viscosity Lubriflow ink system ballpens with comfortable rubber grip.'),
('PRD-CAT-093', 'Reynolds 045 Fine Carbure Ball Pen Blue', 'Stationery & Household Essentials', 'Writing Instruments & Pens', 'Green Basket House', 'PCS', 'GST12', 7.00, 10.00, 10.00, 50, 'SKU-CAT-093', 'TEST-BC-10093', 'Legendary white-body fine-tip ballpoint pen for smudge-free writing.'),
('PRD-CAT-094', 'Classmate Long Notebook Single Line 172 Pages', 'Stationery & Household Essentials', 'Notebooks & Paper', 'Green Basket House', 'PCS', 'GST12', 45.00, 58.00, 65.00, 20, 'SKU-CAT-094', 'TEST-BC-10094', 'Chlorine-free bright white paper single-rule exercise notebook.'),
('PRD-CAT-095', 'Classmate Pulse Spiral Notebook 300 Pages', 'Stationery & Household Essentials', 'Notebooks & Paper', 'Green Basket House', 'PCS', 'GST12', 110.00, 140.00, 160.00, 10, 'SKU-CAT-095', 'TEST-BC-10095', 'Multi-subject spiral wire-bound notebook with poly cover and dividers.'),
('PRD-CAT-096', 'Camlin Kokuyo Sharpener and Eraser Set', 'Stationery & Household Essentials', 'Writing Instruments & Pens', 'Green Basket House', 'PKT', 'GST12', 22.00, 30.00, 35.00, 15, 'SKU-CAT-096', 'TEST-BC-10096', 'Dust-free non-abrasive vinyl eraser paired with nickel-plated blade sharpener.'),
('PRD-CAT-097', 'Freshwrap Food Grade Aluminium Foil 9m', 'Stationery & Household Essentials', 'Kitchen Foil & Tissues', 'Green Basket House', 'BOX', 'GST18', 72.00, 95.00, 115.00, 12, 'SKU-CAT-097', 'TEST-BC-10097', 'Bacteriostatic aluminium foil preserving aroma, warmth, and freshness.'),
('PRD-CAT-098', 'Origami Premium Kitchen Paper Towel 2 Ply', 'Stationery & Household Essentials', 'Kitchen Foil & Tissues', 'Green Basket House', 'PCS', 'GST18', 68.00, 89.00, 105.00, 15, 'SKU-CAT-098', 'TEST-BC-10098', 'Highly absorbent virgin pulp embossed kitchen roll for oil and spills.'),
('PRD-CAT-099', 'Scotch-Brite Heavy Duty Scrub Sponge 3 Pcs', 'Stationery & Household Essentials', 'Kitchen Foil & Tissues', 'Green Basket House', 'PKT', 'GST18', 80.00, 105.00, 120.00, 15, 'SKU-CAT-099', 'TEST-BC-10099', 'Dual-action green scouring pad laminated on durable cellulose sponge.'),
('PRD-CAT-100', 'Good Knight Gold Flash Mosquito Machine & Refill', 'Stationery & Household Essentials', 'Kitchen Foil & Tissues', 'Green Basket House', 'BOX', 'GST18', 85.00, 110.00, 125.00, 12, 'SKU-CAT-100', 'TEST-BC-10100', 'Dual mode electrical mosquito repellent machine with smart flash vapor technology.');


-- ═════════════════════════════════════════════════════════════════════════════
-- 4. INSERT INTO PRODUCTS
-- ═════════════════════════════════════════════════════════════════════════════

INSERT INTO products (
    fk_category,
    fk_subcategory,
    fk_brand,
    fk_unit,
    fk_tax,
    productcode,
    productname,
    description,
    sku,
    barcode,
    costprice,
    sellingprice,
    mrp,
    reorderlevel,
    isactive,
    isonline
)
SELECT
    c.id_category,
    sc.id_subcategory,
    (SELECT b.id_brand FROM brands b WHERE b.brandname = t.brandname AND b.cancelled = FALSE LIMIT 1),
    (SELECT u.id_unit FROM units u WHERE u.unitcode = t.unitcode AND u.cancelled = FALSE LIMIT 1),
    (SELECT tx.id_tax FROM taxes tx WHERE tx.taxcode = t.taxcode AND tx.cancelled = FALSE LIMIT 1),
    t.productcode,
    t.productname,
    t.description,
    t.sku,
    t.barcode,
    t.costprice,
    t.sellingprice,
    t.mrp,
    t.reorderlevel,
    TRUE,
    TRUE
FROM tmp_dummy_products t
JOIN categories c
    ON c.categoryname = t.categoryname AND c.cancelled = FALSE
JOIN subcategories sc
    ON sc.fk_category = c.id_category AND sc.subcategoryname = t.subcategoryname AND sc.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM products p
    WHERE p.productcode = t.productcode
);

COMMIT;

