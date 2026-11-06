-- Required module presets and their module catalog membership.
\set ON_ERROR_STOP on

INSERT INTO module_presets (presetcode, presetname, description, displayorder)
SELECT v.presetcode, v.presetname, v.description, v.displayorder
FROM (VALUES
    ('BASIC', 'Basic', 'Core modules plus inventory and point of sale.', 10),
    ('PREMIUM', 'Premium', 'All supported modules and features.', 20)
) AS v(presetcode, presetname, description, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM module_presets p WHERE p.presetcode = v.presetcode
);

INSERT INTO module_preset_items (fk_preset, fk_module)
SELECT p.id_preset, m.id_module
FROM module_presets p
JOIN modules m ON m.cancelled = FALSE
WHERE (
        (p.presetcode = 'BASIC' AND m.modulecode IN (
            'DASHBOARD','PRODUCTS','INVENTORY','POS','SETTINGS','USERS'))
     OR (p.presetcode = 'PREMIUM' AND m.modulecode IN (
            'DASHBOARD','PRODUCTS','INVENTORY','PURCHASES','POS',
            'CUSTOMERS','REPORTS','SETTINGS','USERS'))
      )
  AND NOT EXISTS (
      SELECT 1 FROM module_preset_items i
      WHERE i.fk_preset = p.id_preset AND i.fk_module = m.id_module
  );
