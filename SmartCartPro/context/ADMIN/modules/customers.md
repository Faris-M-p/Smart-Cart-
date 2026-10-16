# Admin Module — Customers

## Module Overview
- **Area:** ADMIN
- **Module Name:** Customers
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Shared customer master for POS and online.

## Implemented Features
- `Customers/Index` sample list

## Filters / Search / Sorting
- Planned

## Data
- Tables: `customers`, `customer_addresses`

## Database Dependencies
- Company-wide customer master

## API & Data Access
- `CustomersController.Index`
- View: `Views/Customers/Index.cshtml`

## Business Rules
- Soft cancel; unique email/phone when set

## Dependencies & Related Modules
- `pos.md`, `online-orders.md`, `invoices.md`