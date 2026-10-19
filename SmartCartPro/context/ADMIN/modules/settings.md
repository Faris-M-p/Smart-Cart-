# Settings — Phase 7

## Status
IMPLEMENTED (Phase 7)

## Table
`settings` — key-value store with scope (`platform` | `company` | `store`).

## Known Keys (company scope, fk_store = NULL)
| Key | Type | Description |
|---|---|---|
| `pos.invoice_prefix` | string | Prefix for POS invoice numbers |
| `pos.allow_negative_stock` | bool | Allow selling below zero stock |
| `default_tax_code` | string | Default tax code applied at POS |
| `locale.currency` | string | ISO currency code (e.g. INR) |
| `locale.timezone` | string | Default timezone (e.g. Asia/Kolkata) |

## Files
| Layer | File |
|---|---|
| Entity | `Entities/Setting.cs` |
| Models | `Models/SettingModels.cs` |
| Repository interface | `Repositories/Interfaces/ISettingRepository.cs` |
| Repository impl | `Repositories/SettingRepository.cs` |
| Service interface | `Services/Interfaces/ISettingService.cs` |
| Service impl | `Services/SettingService.cs` |
| Controller | `Controllers/SettingController.cs` |

## Endpoints
| Method | Route | Module | Permission | Description |
|---|---|---|---|---|
| GET | `/Setting` | SETTINGS | settings.manage | Settings form (Index) |
| POST | `/Setting/Save` | SETTINGS | settings.manage | Upsert known keys |

## Utility
`ISettingService.GetValueAsync(key, scope, fkStore)` — single-key lookup for other services.
