-- =============================================================================
-- SmartCartPro | 033_AuditLogs.sql
-- Scope: Company-wide (optional fk_store)
-- =============================================================================

CREATE TABLE IF NOT EXISTS audit_logs (
    id_auditlog     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NULL,
    fk_user         INT NULL,
    entityname      TEXT NOT NULL,
    entityid        INT NULL,
    action          TEXT NOT NULL,
    details         TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_audit_logs_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_audit_logs_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_createdat ON audit_logs (createdat);
CREATE INDEX IF NOT EXISTS ix_audit_logs_entity ON audit_logs (entityname, entityid);
