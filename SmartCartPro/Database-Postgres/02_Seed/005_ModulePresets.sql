-- =============================================================================
-- SmartCartPro | Seed V005 — Module Presets (Basic / Premium)
-- Applied by: BuildDatabase.bat (02_Seed phase) or manually.
-- Idempotent — safe to rerun.
-- =============================================================================
\set ON_ERROR_STOP on

-- Module codes in use (from 002_DefaultPermissions.sql):
-- DASHBOARD, PRODUCTS, INVENTORY, PURCHASES, POS, CUSTOMERS, REPORTS, SETTINGS, USERS

-- ---------------------------------------------------------------------------
-- Presets
-- ---------------------------------------------------------------------------
INSERT INTO module_presets (presetcode, presetname, description, displayorder)
SELECT v.presetcode, v.presetname, v.description, v.displayorder
FROM (VALUES
    ('BASIC',   'Basic',   'Core modules: Dashboard, POS, Products, Inventory', 10),
    ('PREMIUM', 'Premium', 'All supported modules including Reports and User Management', 20)
) AS v(presetcode, presetname, description, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM module_presets p WHERE p.presetcode = v.presetcode
);

-- ---------------------------------------------------------------------------
-- Basic preset modules: DASHBOARD, PRODUCTS, POS, INVENTORY
-- ---------------------------------------------------------------------------
INSERT INTO module_preset_items (fk_preset, fk_module)
SELECT p.id_preset, m.id_module
FROM module_presets p
CROSS JOIN modules m
WHERE p.presetcode = 'BASIC'
  AND m.modulecode IN ('DASHBOARD','PRODUCTS','POS','INVENTORY')
  AND m.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM module_preset_items i
      WHERE i.fk_preset = p.id_preset AND i.fk_module = m.id_module
  );

-- ---------------------------------------------------------------------------
-- Premium preset modules: all 9 modules
-- ---------------------------------------------------------------------------
INSERT INTO module_preset_items (fk_preset, fk_module)
SELECT p.id_preset, m.id_module
FROM module_presets p
CROSS JOIN modules m
WHERE p.presetcode = 'PREMIUM'
  AND m.modulecode IN ('DASHBOARD','PRODUCTS','INVENTORY','PURCHASES','POS',
                       'CUSTOMERS','REPORTS','SETTINGS','USERS')
  AND m.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM module_preset_items i
      WHERE i.fk_preset = p.id_preset AND i.fk_module = m.id_module
  );
