CREATE TABLE IF NOT EXISTS productstatus (
    id_productstatus INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    statusname       TEXT NOT NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelledon      TIMESTAMP NULL,
    cancelledreason  TEXT NULL
);
