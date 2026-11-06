-- Feature catalog and selected-feature schema.

CREATE TABLE IF NOT EXISTS module_features (
    id_feature          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_module           INT NOT NULL,
    fk_parent_feature   INT NULL,
    featurecode         TEXT NOT NULL,
    featurename         TEXT NOT NULL,
    description         TEXT NULL,
    is_core             BOOLEAN NOT NULL DEFAULT FALSE,
    displayorder        INT NOT NULL DEFAULT 0,
    isactive            BOOLEAN NOT NULL DEFAULT TRUE,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat           TIMESTAMP NULL,
    cancelled           BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon         TIMESTAMP NULL,
    cancelledreason     TEXT NULL,
    CONSTRAINT uq_module_features_code UNIQUE (featurecode),
    CONSTRAINT fk_module_features_module FOREIGN KEY (fk_module) REFERENCES modules (id_module),
    CONSTRAINT fk_module_features_parent FOREIGN KEY (fk_parent_feature) REFERENCES module_features (id_feature)
);
CREATE INDEX IF NOT EXISTS ix_module_features_fk_module ON module_features (fk_module);
CREATE INDEX IF NOT EXISTS ix_module_features_fk_parent ON module_features (fk_parent_feature);

CREATE TABLE IF NOT EXISTS installation_features (
    id_inst_feature     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_feature          INT NOT NULL,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_inst_features_feature FOREIGN KEY (fk_feature) REFERENCES module_features (id_feature),
    CONSTRAINT uq_installation_features_feature UNIQUE (fk_feature)
);

CREATE TABLE IF NOT EXISTS module_preset_features (
    id_preset_feature   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_preset           INT NOT NULL,
    fk_feature          INT NOT NULL,
    CONSTRAINT fk_preset_features_preset FOREIGN KEY (fk_preset) REFERENCES module_presets (id_preset),
    CONSTRAINT fk_preset_features_feature FOREIGN KEY (fk_feature) REFERENCES module_features (id_feature),
    CONSTRAINT uq_preset_features UNIQUE (fk_preset, fk_feature)
);
CREATE INDEX IF NOT EXISTS ix_preset_features_fk_preset ON module_preset_features (fk_preset);
CREATE INDEX IF NOT EXISTS ix_preset_features_fk_feature ON module_preset_features (fk_feature);
