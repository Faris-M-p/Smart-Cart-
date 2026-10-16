# Admin Module — Filter (common dropdown API)

## Module Overview
- **Area:** ADMIN (shared)
- **Module Name:** Filter
- **Current Status:** `PARTIALLY IMPLEMENTED`

## Purpose & Responsibilities
CRM-style common endpoint to fill dropdowns/filters for screens (products, categories, subcategories).

## Implemented Features
- `GET /Filter/GetFilterData?mode=&criteria=&search=&fk_Store=`
- Mode 1 → `FillProduct` list
- Mode 2 → `FillCategory` list
- Mode 3 → `FillSubCategory` list (`criteria` = category id or empty = all)
- EF Core + LINQ repository; typed `FilterDataResponse`

## Filters / Search / Sorting
- `FilterOptions.Search` optional contains-filter on name
- Mode 3 criteria filters by `fk_category`

## Data
- **Single file:** `Models/FilterModels.cs`
  - `FilterOptions` (input)
  - `FilterDataResponse` (envelope)
  - `FillProduct`, `FillCategory`, `FillSubCategory` (output rows: `ID_value`, `Name`)
- Do **not** split these into separate class files

## Database Dependencies
- `products`, `categories`, `subcategories`
- Access: `IFilterRepository` / `FilterRepository` via EF

## API & Data Access
- Controller: `FilterController`
- Service: `IFilterService` / `FilterService`
- Repository: `IFilterRepository` / `FilterRepository`

## UI Rules & Conventions
- Pages should call this API to populate `<select>` options instead of duplicating fill queries

## Business Rules
- Every repo fill method receives `FilterOptions` (extensible)
- Service validates mode, parses `Criteria` → `CategoryId`, composes `FilterDataResponse`
- Repository does **not** parse criteria strings or host private parse helpers — only queries using prepared options

## Dependencies & Related Modules
- Catalog modules (`product.md`, `category.md`, `subcategory.md`)