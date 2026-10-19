# Admin Module — Company

## Status
`IMPLEMENTED` — MVC + In/Input/Output models (see `.cursor/rules/model-naming.mdc`)

## Stack
- Controllers: `CompanyController`, `StoreController`
- Service: `ICompanyService` / `CompanyService`
- Repository: `ICompanyRepository` / `CompanyRepository` (Entities only inside repo)
- Models: `Models/Company`, `Models/Store`, `Models/License`, `Models/Audit`

## Rules
- Platform claim: create company / create store / update license
- Normal admin: edit company and stores
