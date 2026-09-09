CREATE TABLE IF NOT EXISTS product_images (
    product_image_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id       INT NOT NULL,
    image_url        TEXT NOT NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT fk_product_images_product
        FOREIGN KEY (product_id) REFERENCES products (id_product)
);
