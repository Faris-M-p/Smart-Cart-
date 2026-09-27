CREATE TABLE IF NOT EXISTS useraddresses (
    addressid       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    userid          INT NOT NULL,
    addresstype     TEXT NOT NULL DEFAULT 'Home',
    receivername    TEXT NOT NULL,
    phone           TEXT NOT NULL,
    addressline     TEXT NOT NULL,
    city            TEXT NOT NULL,
    pincode         TEXT NOT NULL,
    latitude        NUMERIC(10, 7) NULL,
    longitude       NUMERIC(10, 7) NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_user_addresses_users
        FOREIGN KEY (userid) REFERENCES users (id_user)
);
