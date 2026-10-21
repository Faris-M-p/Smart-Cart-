# Admin Module — Customers

## Module Name & Area
- **Area:** ADMIN
- **Module Name:** Customers
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Company-wide customer master shared by POS, invoices, and online orders. Staff maintain profiles; cashiers look up or quick-create walk-ins at the register.

## Implemented Features
- List with search (name, code, phone, email), status, sort, page
- Create / Edit (AJAX forms)
- Activate / Deactivate (`data-sc-post`, soft — no hard delete)
- Unique customer code generated as `CUS-{id:D6}` when left blank
- Phone search matches digits only without changing the stored display value
- `GET Customers/Search` and `POST Customers/QuickCreate` for POS pickers
- Invoices link to `Sales/Invoices?customerId=` when the user has `pos.sell` or `reports.view`

## Filters / Search / Sorting
- `Search`, `ActiveOnly` (All / Active / Inactive), `SortBy`, `SortDesc`, `Page`, `PageSize`
- Sort fields: `customercode`, `fullname`, `phone`, `email`, `isactive`, `createdat`
- Search phone: digits stripped from the query; stored phone is not rewritten

## Data
- `InCustomerCreateView` / `InputCustomerCreate` / `OutputCustomerCreate`
- `InCustomerUpdateView` / `InputCustomerUpdate` / `OutputCustomerUpdate`
- `InputCustomerSetActive` / `OutputCustomerSetActive`
- `InputCustomerSelect` / `OutputCustomerSelect`
- `InputCustomerList` / `OutputCustomerList`
- `InputCustomerSearch` / `OutputCustomerSearchItem`
- `InCustomerQuickCreateView` / `InputCustomerQuickCreate`
- Entity `Customer` stays in the repository

## Database Dependencies
- `customers` (company-wide). Unique email / phone when set. Soft cancel columns exist; UI uses activate/deactivate.

## API & Data Access
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/Customers` | customers.view | Paged list |
| GET | `/Customers/Create` | customers.manage | Create form |
| POST | `/Customers/Create` | customers.manage | Save (AJAX envelope) |
| GET | `/Customers/Edit/{id}` | customers.manage | Edit form |
| POST | `/Customers/Edit/{id}` | customers.manage | Save (AJAX envelope) |
| POST | `/Customers/SetActive` | customers.manage | Activate / deactivate |
| GET | `/Customers/Search?term=&take=` | customers.view **or** pos.sell | Active-only lookup; take default 10, max 20 |
| POST | `/Customers/QuickCreate` | customers.manage **or** pos.sell | Walk-in create; form `FullName`, `Phone`, `Email` |

Controller: `[ModuleAccess("CUSTOMERS","POS")]`. Search `data` = `[{idCustomer,customerCode,fullName,phone,email}]`. QuickCreate `data` = `{idCustomer,customerCode,fullName,phone,email}`.

## UI Rules & Conventions
- Category-style list + form; `wwwroot/js/customers/customers.js`
- Activate/deactivate uses `data-sc-post` + confirm
- No sample rows; empty state offers Add when the user can manage

## Business Rules
- Soft cancel only (no hard delete)
- Unique email/phone/code; empty optional fields stored as null
- Blank code → generated unique `CUS-{id}`
- Search/QuickCreate return **active** customers only
- Phone matching is digits-only on the query; display phone is unchanged

## Dependencies & Related Modules
- `pos.md`, `online-orders.md`, `invoices.md`

## Notes
- Customer addresses table is not on the admin screens (no form/list yet).
- `FindByPhoneAsync` remains on the service for internal POS use; the HTTP contract is `Search`.
