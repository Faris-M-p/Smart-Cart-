# SmartCartPro `context/` — single source of truth

Same layout pattern as `Ecommerce/context/`.

## Start here

| File | Purpose |
|------|---------|
| **[RULES.md](./RULES.md)** | **Common rules for every AI / developer** |
| [PROJECT.md](./PROJECT.md) | What the product is |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Layers & folders |
| [DATABASE.md](./DATABASE.md) | PostgreSQL conventions |
| [FUTURE.md](./FUTURE.md) | Deferred work |

## Areas

| Path | Purpose |
|------|---------|
| [ADMIN/ADMIN.md](./ADMIN/ADMIN.md) | Admin area index |
| [ADMIN/modules/](./ADMIN/modules/) | Per-module specs |
| [USER/USER.md](./USER/USER.md) | Storefront (reserved) |

When you add a feature: update `RULES`/`DATABASE` if needed **and** the module file under `ADMIN/modules/` (or create one).
