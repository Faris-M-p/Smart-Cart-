-- =============================================================================
-- SmartCartPro | 032_Settings.sql
-- Scope: Platform / company / store (nullable fk_store)
-- =============================================================================

-- setting_scope: 'platform' | 'company' | 'store'
-- When scope = 'store', fk_store is required (application + CHECK).
CREATE TABLE IF NOT EXISTS settings (
    id_setting      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    setting_scope   TEXT NOT NULL DEFAULT 'company',
    fk_store        INT NULL,
    settingkey      TEXT NOT NULL,
    settingvalue    TEXT NULL,
    valuetype       TEXT NOT NULL DEFAULT 'string',
    description     TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_settings_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT ck_settings_scope
        CHECK (setting_scope IN ('platform','company','store')),
    CONSTRAINT ck_settings_store_scope
        CHECK (
            (setting_scope = 'store' AND fk_store IS NOT NULL)
            OR (setting_scope <> 'store' AND fk_store IS NULL)
        ),
    CONSTRAINT uq_settings_scope_store_key UNIQUE (setting_scope, fk_store, settingkey)
);

CREATE INDEX IF NOT EXISTS ix_settings_fk_store ON settings (fk_store);
