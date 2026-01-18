# Decision: SCSS Compilation Approach

**Date:** 2025-01-XX  
**Decision Type:** Technical Architecture  
**Status:** ✅ Approved

## Decision

Use **AspNetCore.SassCompiler** with Bootstrap SCSS from `node_modules` for SCSS compilation.

## Context

The project uses the Mantis Bootstrap Admin Template which requires:
- Bootstrap SCSS source files for customization
- Custom SCSS components
- Theme customization via SCSS variables

## Options Considered

### Option 1: AspNetCore.SassCompiler + npm (✅ Chosen)
**Pros:**
- Automatic compilation during build
- Full Bootstrap customization
- Single CSS output file
- Modern workflow

**Cons:**
- Requires Node.js/npm
- Additional setup step

### Option 2: Pre-compiled Bootstrap CSS
**Pros:**
- No Node.js needed
- Simpler setup

**Cons:**
- Limited customization
- Multiple CSS files
- Can't use Bootstrap SCSS features

### Option 3: Manual SCSS Compilation
**Pros:**
- Full control

**Cons:**
- Manual process
- Not integrated with build
- Error-prone

## Rationale

Chose Option 1 because:
1. Template requires Bootstrap SCSS for full customization
2. Automatic compilation integrates with build process
3. Single CSS file improves performance
4. Modern development workflow

## Implementation

- AspNetCore.SassCompiler NuGet package
- Bootstrap installed via npm
- SCSS files compiled during `dotnet build`
- Compiled CSS copied to output directory

## Impact

- ✅ All styles in single `style.css` file
- ✅ Easy to customize Bootstrap
- ✅ Automatic compilation
- ⚠️ Requires `npm install` after cloning

## Related Documents

- `../analysis/WHY_NODEJS.md`
- `../feature-logs/SCSS-COMPILATION-SETUP.md`
