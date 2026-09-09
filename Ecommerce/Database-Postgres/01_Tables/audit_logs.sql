CREATE TABLE IF NOT EXISTS audit_logs (
    log_id           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    action           TEXT NOT NULL,
    user_id          INT NOT NULL,
    table_name       TEXT NOT NULL,
    record_id        INT NOT NULL,
    change_details   TEXT NULL,
    created_at       TIMESTAMP NULL DEFAULT NOW(),
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);
