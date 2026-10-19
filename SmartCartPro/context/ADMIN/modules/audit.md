# Audit Log — Phase 7

## Status
IMPLEMENTED (Phase 7) — read-only list + pagination + filters.

## Table
`audit_logs` — immutable trail of admin actions (entityname, entityid, action, details).

## Access
`[RequirePermission("settings.manage")]` — allows both:
- Platform Super Admin (bypasses all permission checks via `RequirePermissionFilter`)
- Admin users with `settings.manage` permission

## Files
| Layer | File |
|---|---|
| Entity | `Entities/AuditLog.cs` (existing) |
| Models | `Models/AuditModels.cs` (expanded with `InputAuditLogList`, `OutputAuditLogItem`, `OutputAuditLogList`) |
| Repository interface | `Repositories/Interfaces/IAuditRepository.cs` |
| Repository impl | `Repositories/AuditRepository.cs` |
| Service interface | `Services/Interfaces/IAuditService.cs` |
| Service impl | `Services/AuditService.cs` |
| Controller | `Controllers/AuditController.cs` |

## Endpoints
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/Audit` | settings.manage | Paginated list with filters |
| GET | `/Audit/ListJson` | settings.manage | JSON list for export |

## Filters Available
- Entity name, entity ID, user, store, action type, date range
