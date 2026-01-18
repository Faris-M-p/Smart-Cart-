# Quick Setup Guide

## After Cloning the Repository

### Step 1: Install Node.js Dependencies (REQUIRED)

**This is essential for SCSS compilation to work!**

```bash
npm install
```

**Why?** The project uses Bootstrap SCSS from `node_modules` for compiling styles. Without this, the Sass compiler will fail.

### Step 2: Restore .NET Packages

```bash
dotnet restore
```

### Step 3: Build the Project

```bash
dotnet build
```

This will:
- Compile SCSS files automatically
- Generate `style.css` from `style.scss`
- Include all components (Toast, etc.) in the compiled CSS

### Step 4: Run the Application

```bash
dotnet run
```

## What Gets Installed?

- **Bootstrap 5.3.0** (devDependency) - Required for SCSS compilation
- All .NET packages from `Ecommerce.csproj`

## Important Notes

- ✅ `node_modules/` is in `.gitignore` - **DO NOT commit it**
- ✅ `package.json` is committed - contains dependency list
- ✅ `package-lock.json` is committed - ensures consistent installs
- ❌ Never commit `node_modules/` - it's large and can be regenerated

## Troubleshooting

**"Cannot find Bootstrap SCSS"**
→ Run `npm install`

**"SCSS not compiling"**
→ Ensure `node_modules/bootstrap` exists
→ Clean and rebuild: `dotnet clean && dotnet build`

**"Toast not working"**
→ Check that `style.css` contains toast styles
→ Verify SCSS compiled successfully
