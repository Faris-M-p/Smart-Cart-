# SmartCart — E-Commerce Application

SmartCart is a supermarket-style e-commerce platform built with ASP.NET Core 8.0, featuring a public storefront for customer shopping and an employee admin back office for catalog, purchasing, stock, and order fulfillment.

---

## 📚 Single Source of Truth Documentation (`context/`)

All permanent project knowledge, system architecture, database design, development rules, user/admin specs, and module documentation are strictly maintained inside the **[`context/`](context/)** directory:

```text
context/
├── PROJECT.md          # Project purpose, scope, tech stack & setup guide
├── RULES.md            # Master development & context maintenance rules
├── ARCHITECTURE.md     # System architecture, layers, auth & UI components
├── DATABASE.md         # Database schema, tables, stored procs & EF mappings
│
├── USER/               # Storefront documentation
│   ├── USER.md         # Storefront overview & rules
│   └── modules/        # Storefront module specifications (Home, Shop, Cart, etc.)
│
├── ADMIN/              # Admin back-office documentation
│   ├── ADMIN.md        # Admin overview & rules
│   └── modules/        # Admin module specifications (Product, Inventory, Orders, etc.)
│
└── FUTURE.md           # Planned direction & future non-implemented features
```

> **Developer & AI Note:** Do not create duplicate documentation outside `context/`. Refer to [`context/PROJECT.md`](context/PROJECT.md) and [`context/RULES.md`](context/RULES.md) for full project specifications and guidelines.

---

## 🚀 Quick Setup & Run

1. **Install Node.js Dependencies (Required for SCSS compilation):**
   ```bash
   npm install
   ```
2. **Restore .NET Dependencies & Build:**
   ```bash
   dotnet restore
   dotnet build
   ```
3. **Run Application:**
   ```bash
   dotnet run
   ```

For database setup instructions (`Database/RunDatabase.bat` / `Database/RunPatch.bat`) and comprehensive setup details, see [`context/PROJECT.md`](context/PROJECT.md).
