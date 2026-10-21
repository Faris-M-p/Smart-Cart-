# Admin Module — Filter (common dropdown API)

## Module Overview
- **Area:** ADMIN (shared)
- **Module Name:** Filter
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
CRM-style common endpoint to fill dropdowns/filters for screens (products, categories, subcategories).

## Implemented Features
- `GET /Filter/GetFilterData?mode=&criteria=&search=`
- Mode 1 → products, 2 → categories, 3 → sub-categories (`criteria` = category id or empty = all)
- EF Core + LINQ repository; typed `FilterDataResponse` wrapped in `CommonResponseModel`

## Data
- **Single file:** `Models/FilterModels.cs` — `FilterOptions`, `FilterDataResponse`, `FillProduct`, `FillCategory`, `FillSubCategory`

## API & Data Access
| Method | Route | Permission | Module |
|--------|-------|------------|--------|
| GET | `/Filter/GetFilterData` | `products.view` | `PRODUCTS` |

- Controller: `FilterController` (MVC, not `[ApiController]`)
- Service: `IFilterService` / `FilterService`
- Repository: `IFilterRepository` / `FilterRepository`

## Business Rules
- Service validates mode and parses `Criteria` → `CategoryId`
- Repository does not parse criteria strings
- Signed-in users without `products.view` receive 403

## Dependencies
- Catalog modules (`product.md`, `category.md`, `subcategory.md`)
