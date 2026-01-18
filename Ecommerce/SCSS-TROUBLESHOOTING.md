# SCSS Compilation Troubleshooting Guide

## Quick Fix Steps

### Step 1: Ensure Node.js and npm are installed
```bash
node --version
npm --version
```

### Step 2: Install Bootstrap (if node_modules missing)
```bash
npm install
```

### Step 3: Clean and Rebuild
```bash
# Stop your running application first!
dotnet clean
dotnet build
```

### Step 4: Check if SCSS compiled
After build, check if these files exist:
- `wwwroot\Admin\assets\scss\style.css` (compiled output)
- `wwwroot\Admin\assets\css\style.css` (copied file)

## Common Issues

### Issue 1: node_modules Missing
**Symptom:** Build fails with "Cannot find module 'bootstrap'"
**Solution:**
```bash
npm install
```

### Issue 2: Application Running During Build
**Symptom:** Build fails with file lock errors
**Solution:**
1. Stop the running application (Ctrl+C in terminal)
2. Close Visual Studio if running
3. Run `dotnet build` again

### Issue 3: SCSS Not Compiling
**Symptom:** No `style.css` in `scss` folder after build
**Solution:**
1. Check for SCSS syntax errors
2. Verify `AspNetCore.SassCompiler` package is installed
3. Check `SassIncludePath` in `.csproj` points to `node_modules`

### Issue 4: Compiled CSS Not Copied
**Symptom:** `style.css` exists in `scss` folder but not in `css` folder
**Solution:**
- The build target should copy it automatically
- If not, manually copy or check build output

## Verification Checklist

- [ ] `node_modules` folder exists
- [ ] `package.json` has `bootstrap` dependency
- [ ] `AspNetCore.SassCompiler` package installed in `.csproj`
- [ ] `SassIncludePath` set to `node_modules` in `.csproj`
- [ ] Application is stopped before building
- [ ] `dotnet build` completes without errors
- [ ] `wwwroot\Admin\assets\scss\style.css` exists after build
- [ ] `wwwroot\Admin\assets\css\style.css` exists after build

## Alternative: Use Direct CSS (Current Solution)

Since SCSS compilation is problematic, we've created:
- `wwwroot\Admin\assets\css\empty-state.css` - Direct CSS file (no compilation needed)

This file is already linked in `_AdminLayout.cshtml` and works immediately.

## To Switch Back to SCSS Later

1. Remove the direct CSS link from `_AdminLayout.cshtml`
2. Ensure SCSS compilation works
3. The `_empty-state.scss` file is already in `style.scss` imports
