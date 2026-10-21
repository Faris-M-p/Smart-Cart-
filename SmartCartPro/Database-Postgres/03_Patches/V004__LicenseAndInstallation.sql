-- =============================================================================
-- SmartCartPro | Patch V004 — License + Installation Configuration
-- Applied by: ApplyPatches.bat
-- =============================================================================
\set ON_ERROR_STOP on

-- ---------------------------------------------------------------------------
-- license
-- One row per installation. status: 'active' | 'suspended' | 'expired' | 'trial'
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS license (
    id_license          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    licensekey          TEXT NOT NULL,
    license_type        TEXT NOT NULL DEFAULT 'standard',
    status              TEXT NOT NULL DEFAULT 'active',
    startdate           DATE NOT NULL,
    expirydate          DATE NOT NULL,
    maxstores           INT NOT NULL DEFAULT 1,
    notes               TEXT NULL,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat           TIMESTAMP NULL,
    cancelled           BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon         TIMESTAMP NULL,
    cancelledreason     TEXT NULL,
    CONSTRAINT uq_license_licensekey UNIQUE (licensekey),
    CONSTRAINT ck_license_status
        CHECK (status IN ('active','suspended','expired','trial')),
    CONSTRAINT ck_license_dates
        CHECK (expirydate >= startdate),
    CONSTRAINT ck_license_maxstores
        CHECK (maxstores >= 1)
);

-- ---------------------------------------------------------------------------
-- installation_config
-- Tracks onboarding wizard completion state, theme, store mode.
-- One row per installation.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS installation_config (
    id_config           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_license          INT NULL,
    theme               TEXT NOT NULL DEFAULT 'light',
    store_mode          TEXT NOT NULL DEFAULT 'single',
    onboarding_step     INT NOT NULL DEFAULT 0,
    onboarding_complete BOOLEAN NOT NULL DEFAULT FALSE,
    activated_at        TIMESTAMP NULL,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat           TIMESTAMP NULL,
    CONSTRAINT fk_installation_license
        FOREIGN KEY (fk_license) REFERENCES license (id_license),
    CONSTRAINT ck_installation_store_mode
        CHECK (store_mode IN ('single','multi')),
    CONSTRAINT ck_installation_theme
        CHECK (theme IN ('light','dark'))
);

CREATE INDEX IF NOT EXISTS ix_installation_config_fk_license
    ON installation_config (fk_license);

-- ---------------------------------------------------------------------------
-- module_presets  (Basic / Premium)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS module_presets (
    id_preset           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    presetcode          TEXT NOT NULL,
    presetname          TEXT NOT NULL,
    description         TEXT NULL,
    displayorder        INT NOT NULL DEFAULT 0,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_module_presets_code UNIQUE (presetcode)
);

-- ---------------------------------------------------------------------------
-- module_preset_items  (which modules belong to each preset)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS module_preset_items (
    id_preset_item      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_preset           INT NOT NULL,
    fk_module           INT NOT NULL,
    CONSTRAINT fk_preset_items_preset
        FOREIGN KEY (fk_preset) REFERENCES module_presets (id_preset),
    CONSTRAINT fk_preset_items_module
        FOREIGN KEY (fk_module) REFERENCES modules (id_module),
    CONSTRAINT uq_preset_items_preset_module UNIQUE (fk_preset, fk_module)
);

CREATE INDEX IF NOT EXISTS ix_preset_items_fk_preset
    ON module_preset_items (fk_preset);

-- ---------------------------------------------------------------------------
-- installation_modules  (the final selected module list for this installation)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS installation_modules (
    id_inst_module      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_module           INT NOT NULL,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_inst_modules_module
        FOREIGN KEY (fk_module) REFERENCES modules (id_module),
    CONSTRAINT uq_installation_modules_module UNIQUE (fk_module)
);

-- ---------------------------------------------------------------------------
-- user_invitations  (Company Admin account invitation flow)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_invitations (
    id_invitation       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user             INT NULL,           -- set when account is created
    invited_email       TEXT NOT NULL,
    invited_fullname    TEXT NOT NULL,
    invitation_token    TEXT NOT NULL,      -- single-use token for account setup
    fk_role             INT NOT NULL,       -- assigned role (ADMIN for the company administrator)
    fk_store            INT NOT NULL,       -- initial store access
    status              TEXT NOT NULL DEFAULT 'pending',
    expires_at          TIMESTAMP NOT NULL,
    accepted_at         TIMESTAMP NULL,
    delivery_status     TEXT NOT NULL DEFAULT 'pending',  -- 'pending'|'sent'|'failed'
    delivery_error      TEXT NULL,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat           TIMESTAMP NULL,
    CONSTRAINT fk_invitations_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT fk_invitations_role
        FOREIGN KEY (fk_role) REFERENCES roles (id_role),
    CONSTRAINT fk_invitations_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT uq_invitation_token UNIQUE (invitation_token),
    CONSTRAINT ck_invitation_status
        CHECK (status IN ('pending','accepted','expired','cancelled')),
    CONSTRAINT ck_invitation_delivery
        CHECK (delivery_status IN ('pending','sent','failed'))
);

CREATE INDEX IF NOT EXISTS ix_invitations_email ON user_invitations (invited_email);
CREATE INDEX IF NOT EXISTS ix_invitations_token ON user_invitations (invitation_token);

-- ---------------------------------------------------------------------------
-- Add fk_user index to audit_logs if not exists
-- ---------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS ix_audit_logs_fk_user ON audit_logs (fk_user);
