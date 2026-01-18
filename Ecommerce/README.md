# Ecommerce - ASP.NET Core Project

An e-commerce application built with ASP.NET Core 8.0, using the Mantis Bootstrap Admin Template.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK 8.0** or later ([Download](https://dotnet.microsoft.com/download))
- **Node.js** (v14 or later) and **npm** ([Download](https://nodejs.org/))
- **SQL Server** (for database)
- **Visual Studio 2022** or **Visual Studio Code** (recommended)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Ecommerce
```

### 2. Install Node.js Dependencies

**IMPORTANT:** This project uses Bootstrap SCSS from `node_modules` for Sass compilation. You must install npm packages before building:

```bash
npm install
```

This will install:
- `bootstrap@5.3.0` - Required for SCSS compilation (Bootstrap SCSS source files)

### 3. Restore .NET Packages

```bash
dotnet restore
```

### 4. Database Setup

1. Execute the SQL scripts in the `xPROCEDURExTABLES/Tables/` directory to create the database schema
2. Execute the stored procedures from `xPROCEDURExTABLES/Procedure/` directory
3. Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  }
}
```

### 5. Build the Project

```bash
dotnet build
```

**Note:** The build process will:
- Compile SCSS files (`style.scss` → `style.css`) using AspNetCore.SassCompiler
- Copy compiled CSS files to the appropriate directories
- The Sass compiler requires `node_modules/bootstrap` to be present

### 6. Run the Application

```bash
dotnet run
```

Or use Visual Studio:
- Press `F5` to run with debugging
- Press `Ctrl+F5` to run without debugging

The application will be available at:
- `https://localhost:5001` (HTTPS)
- `http://localhost:5000` (HTTP)

## Project Structure

```
Ecommerce/
├── Controllers/          # MVC Controllers
├── Models/               # Data Models
├── Repository/           # Data Access Layer
├── Interface/            # Service Interfaces
├── DataAccess/           # Database Access (Dapper)
├── views/                # Razor Views
│   ├── Admin/           # Admin Panel Views
│   └── Shared/          # Shared Layouts
├── wwwroot/              # Static Files
│   └── Admin/
│       └── assets/
│           ├── css/      # Compiled CSS (auto-generated)
│           └── scss/     # SCSS Source Files
├── findings/             # Documentation & Feature Logs
│   ├── analysis/        # Technical Analysis
│   ├── decisions/       # Architecture Decisions
│   └── feature-logs/    # Feature Implementation Logs
├── xPROCEDURExTABLES/    # Database Scripts
└── Ecommerce.csproj      # Project File
```

## SCSS Compilation

This project uses **AspNetCore.SassCompiler** for automatic SCSS compilation:

- **Source:** `wwwroot/Admin/assets/scss/style.scss`
- **Output:** `wwwroot/Admin/assets/css/style.css`
- **Dependencies:** Bootstrap SCSS from `node_modules/bootstrap`

### How It Works

1. The `style.scss` file imports Bootstrap and all custom components
2. During build, AspNetCore.SassCompiler compiles SCSS → CSS
3. Compiled CSS is automatically copied to the `css` directory
4. **All components (including Toast) are compiled into a single `style.css` file**

### Important Notes

- **DO NOT** commit `node_modules/` to Git (already in `.gitignore`)
- **DO NOT** commit compiled CSS files from the `scss/` directory
- Always run `npm install` after cloning the repository
- The Sass compiler requires Bootstrap to be installed via npm

## Development Workflow

1. **Make SCSS changes** in `wwwroot/Admin/assets/scss/`
2. **Build the project** - SCSS will compile automatically
3. **Refresh browser** to see changes

## NuGet Packages

- `AspNetCore.SassCompiler` (v1.97.1) - SCSS compilation
- `Dapper` (v2.1.35) - Micro ORM
- `Microsoft.Data.SqlClient` (v5.2.2) - SQL Server driver

## Troubleshooting

### SCSS Not Compiling

1. **Ensure `node_modules` is installed:**
   ```bash
   npm install
   ```

2. **Check that Bootstrap is installed:**
   ```bash
   Test-Path node_modules/bootstrap/scss/_functions.scss
   ```

3. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

### Toast Notifications Not Working

- Ensure `style.css` contains toast styles (should be compiled from SCSS)
- Check browser console for JavaScript errors
- Verify `toast.js` is loaded in the layout

### Build Errors

- **"Cannot find Bootstrap"**: Run `npm install`
- **"File locked"**: Stop the running application before building
- **"Duplicate Content items"**: Check `.csproj` for duplicate file references

## Contributing

1. Create a feature branch
2. Make your changes
3. Ensure SCSS compiles successfully
4. Test the application
5. Submit a pull request

## License

[Your License Here]

## Documentation

- **Setup Guide:** See `SETUP.md` for quick setup instructions
- **Findings & Logs:** See `findings/` folder for:
  - Feature implementation logs
  - Technical analysis documents
  - Architecture decisions
- **Node.js Explanation:** See `findings/analysis/WHY_NODEJS.md`
- **Bootstrap Analysis:** See `findings/analysis/BOOTSTRAP_USAGE_ANALYSIS.md`

## Support

For issues or questions, please contact [Your Contact Information]
