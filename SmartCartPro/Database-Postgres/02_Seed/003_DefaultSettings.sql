-- Default company-level settings (no fake transactional data)
\set ON_ERROR_STOP on

INSERT INTO settings (setting_scope, fk_store, settingkey, settingvalue, valuetype, description)
SELECT 'company', NULL, v.settingkey, v.settingvalue, v.valuetype, v.description
FROM (VALUES
    ('pos.invoice_prefix', 'INV', 'string', 'Default POS invoice prefix'),
    ('pos.allow_negative_stock', 'false', 'bool', 'Allow selling below zero stock'),
    ('inventory.track_variants', 'true', 'bool', 'Track stock per product variant'),
    ('locale.currency', 'INR', 'string', 'Default currency code'),
    ('locale.timezone', 'Asia/Kolkata', 'string', 'Default timezone')
) AS v(settingkey, settingvalue, valuetype, description)
WHERE NOT EXISTS (
    SELECT 1 FROM settings s
    WHERE s.setting_scope = 'company'
      AND s.fk_store IS NULL
      AND s.settingkey = v.settingkey
);
