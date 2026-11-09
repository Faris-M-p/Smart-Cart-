# SmartCartPro — Aiven Cloud & Database Connection Guide

This folder (`SmartCartPro/Common/`) is designated for important shared configuration details, environment notes, and database connection instructions.

---

## 1. Aiven Cloud PostgreSQL Connection Details

| Property | Value |
|---|---|
| **Host** | `pg-3b6aeb4c-smartcartpro.d.aivencloud.com` |
| **Port** | `15307` |
| **Username** | `avnadmin` |
| **Maintenance / Default Database** | `defaultdb` |
| **Target Application Database** | `smartcartpro` |
| **SSL Mode** | `require` |
| **Recommended Pool Size** | `Maximum Pool Size=10;Minimum Pool Size=1;` (Aiven plan limit is 15 connections) |

---

## 2. Ready-to-Use Connection Strings

### A. Aiven Cloud Connection String
```text
Host=pg-3b6aeb4c-smartcartpro.d.aivencloud.com;Port=15307;Database=smartcartpro;Username=avnadmin;Password=YOUR_AIVEN_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10;Minimum Pool Size=1;Connection Lifetime=300;
```

### B. Local PostgreSQL Connection String
```text
Host=localhost;Port=5432;Database=smartcart_pro;Username=postgres;Password=YOUR_LOCAL_PASSWORD;
```

---

## 3. Database Automation Scripts

The scripts in `SmartCartPro\Database-Postgres\` support both targets with an interactive prompt:

```text
========================================
 SmartCartPro PostgreSQL Database Setup
========================================

1. Local PostgreSQL
2. Aiven Cloud PostgreSQL

Select database target (1 or 2) [default: 1]:
```

### A. Build Database (`BuildDatabase.bat`)
- **Option 1 (Local)**: Connects to local PostgreSQL (`localhost:5432`), creates `smartcart_pro` (if needed), and installs the V001 baseline + patches.
- **Option 2 (Aiven)**: Connects to Aiven cloud (`pg-3b6aeb4c-smartcartpro.d.aivencloud.com:15307`) via SSL, prompts for the `avnadmin` password securely (masked input), ensures `smartcartpro` exists, and applies the V001 baseline + patches.

```cmd
cd SmartCartPro\Database-Postgres
BuildDatabase.bat
```

### B. Apply Patches (`ApplyPatches.bat`)
Applies pending incremental patches (`V002`, `V003`, `V004`) to your target database without modifying existing data:

```cmd
cd SmartCartPro\Database-Postgres
ApplyPatches.bat
```

### C. Run Dummy Data (`RunDummyData.bat`)
Seeds test data into the selected database:
- `001_DevelopmentPlatformSuperAdmin.sql`
- `002_DevelopmentTrialLicense.sql`
- `003_GreenBasketSupermarket.sql`
- `product_catalog_dummy_data.sql` (10 Categories, 35 Subcategories, 100 FMCG items)

```cmd
cd SmartCartPro\Database-Postgres
RunDummyData.bat
```

---

## 4. How to Connect the ASP.NET Core Application

When you want to run the application against Aiven, you can set the connection string using any of these methods:

### Method 1: Using .NET User Secrets (Recommended — Keeps passwords out of Git)
Open a terminal in the `SmartCartPro` project directory and run:
```bash
cd SmartCartPro
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=pg-3b6aeb4c-smartcartpro.d.aivencloud.com;Port=15307;Database=smartcartpro;Username=avnadmin;Password=YOUR_AIVEN_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10;Minimum Pool Size=1;"
```

### Method 2: In `appsettings.Development.json`
Update `DefaultConnection` in `SmartCartPro\appsettings.Development.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=pg-3b6aeb4c-smartcartpro.d.aivencloud.com;Port=15307;Database=smartcartpro;Username=avnadmin;Password=YOUR_AIVEN_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=10;Minimum Pool Size=1;"
}
```

---

## 5. Quick Verification Queries

Run these in any SQL editor connected to your database to verify:

```sql
-- 1. Check total table count (expect ~46 tables)
SELECT count(*) AS total_tables 
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

-- 2. Check applied patches
SELECT version, description, appliedat, success 
FROM patch_history 
ORDER BY version;

-- 3. Check Purchase Management simplified tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name IN ('purchases', 'purchase_details', 'purchase_returns', 'purchase_orders', 'supplier_transactions');

-- 4. Check system roles
SELECT rolecode, rolename, issystem FROM roles WHERE cancelled = FALSE;
```

