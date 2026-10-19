using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly SmartCartDbContext _db;

    public CatalogRepository(SmartCartDbContext db) => _db = db;

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputCategorySelect?> GetCategoryAsync(InputCategorySelect input, CancellationToken ct = default)
        => await _db.Categories
            .Where(x => x.IdCategory == input.IdCategory && !x.Cancelled)
            .Select(x => new OutputCategorySelect
            {
                IdCategory = x.IdCategory,
                CategoryCode = x.CategoryCode,
                CategoryName = x.CategoryName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputCategoryList> ListCategoriesAsync(InputCategoryList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        var sortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "categoryname" : input.SortBy;

        var q = _db.Categories.Where(x => !x.Cancelled);
        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim();
            q = q.Where(x => x.CategoryName.Contains(term)
                || (x.CategoryCode != null && x.CategoryCode.Contains(term)));
        }

        q = sortBy.ToLowerInvariant() switch
        {
            "categorycode" or "code" => input.SortDesc
                ? q.OrderByDescending(x => x.CategoryCode).ThenBy(x => x.CategoryName)
                : q.OrderBy(x => x.CategoryCode).ThenBy(x => x.CategoryName),
            "displayorder" => input.SortDesc
                ? q.OrderByDescending(x => x.DisplayOrder).ThenBy(x => x.CategoryName)
                : q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CategoryName),
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.CategoryName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.CategoryName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.CategoryName)
                : q.OrderBy(x => x.CategoryName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OutputCategorySelect
            {
                IdCategory = x.IdCategory,
                CategoryCode = x.CategoryCode,
                CategoryName = x.CategoryName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return new OutputCategoryList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
            Filter = new InputCategoryList
            {
                Search = input.Search,
                ActiveOnly = input.ActiveOnly,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDesc = input.SortDesc
            }
        };
    }

    public async Task<OutputCategoryCreate> CreateCategoryAsync(InputCategoryCreate input, CancellationToken ct = default)
    {
        var name = input.CategoryName.Trim();
        var duplicate = await _db.Categories.AnyAsync(
            x => !x.Cancelled && x.CategoryName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputCategoryCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A category with this name already exists.",
                StatusCode = 409
            };
        }

        var entity = new Category
        {
            CategoryCode = string.IsNullOrWhiteSpace(input.CategoryCode) ? null : input.CategoryCode.Trim(),
            CategoryName = name,
            Description = input.Description,
            DisplayOrder = input.DisplayOrder,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputCategoryCreate { IdCategory = entity.IdCategory, CategoryName = entity.CategoryName };
    }

    public async Task<OutputCategoryUpdate> UpdateCategoryAsync(InputCategoryUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.IdCategory == input.IdCategory && !x.Cancelled, ct);
        if (entity is null) return new OutputCategoryUpdate { ResponseCode = 1, ResponseMsg = "Category not found.", StatusCode = 404 };

        var name = input.CategoryName.Trim();
        var duplicate = await _db.Categories.AnyAsync(
            x => !x.Cancelled && x.IdCategory != input.IdCategory && x.CategoryName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputCategoryUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A category with this name already exists.",
                StatusCode = 409
            };
        }

        entity.CategoryCode = string.IsNullOrWhiteSpace(input.CategoryCode) ? null : input.CategoryCode.Trim();
        entity.CategoryName = name;
        entity.Description = input.Description;
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return new OutputCategoryUpdate { IdCategory = entity.IdCategory };
    }

    // ═══════════════════════════════════════════════════════════════
    // SUB-CATEGORY
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSubCategorySelect?> GetSubCategoryAsync(InputSubCategorySelect input, CancellationToken ct = default)
        => await _db.SubCategories
            .Where(x => x.IdSubcategory == input.IdSubCategory && !x.Cancelled)
            .Select(x => new OutputSubCategorySelect
            {
                IdSubCategory = x.IdSubcategory,
                FkCategory = x.FkCategory,
                CategoryName = x.Category != null ? x.Category.CategoryName : string.Empty,
                SubCategoryName = x.SubCategoryName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputSubCategoryList> ListSubCategoriesAsync(InputSubCategoryList input, CancellationToken ct = default)
    {
        var q = _db.SubCategories.Where(x => !x.Cancelled);

        var categoryId = input.FkCategory ?? 0;
        if (categoryId > 0) q = q.Where(x => x.FkCategory == categoryId);

        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(x => x.SubCategoryName.ToLower().Contains(term)
                || (x.Category != null && x.Category.CategoryName.ToLower().Contains(term)));
        }

        q = input.SortBy.ToLowerInvariant() switch
        {
            "subcategoryname" or "name" => input.SortDesc
                ? q.OrderByDescending(x => x.SubCategoryName)
                : q.OrderBy(x => x.SubCategoryName),
            "displayorder" => input.SortDesc
                ? q.OrderByDescending(x => x.DisplayOrder).ThenBy(x => x.SubCategoryName)
                : q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.SubCategoryName),
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.SubCategoryName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.SubCategoryName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.Category!.CategoryName).ThenBy(x => x.SubCategoryName)
                : q.OrderBy(x => x.Category!.CategoryName).ThenBy(x => x.SubCategoryName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputSubCategorySelect
            {
                IdSubCategory = x.IdSubcategory,
                FkCategory = x.FkCategory,
                CategoryName = x.Category != null ? x.Category.CategoryName : string.Empty,
                SubCategoryName = x.SubCategoryName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return new OutputSubCategoryList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputSubCategoryCreate> CreateSubCategoryAsync(InputSubCategoryCreate input, CancellationToken ct = default)
    {
        // The parent must exist, be live and active (never trust the posted id).
        var parent = await _db.Categories
            .Where(x => x.IdCategory == input.FkCategory && !x.Cancelled)
            .Select(x => new { x.IsActive })
            .FirstOrDefaultAsync(ct);
        if (parent is null || !parent.IsActive)
        {
            return new OutputSubCategoryCreate
            {
                ResponseCode = 1,
                ResponseMsg = "Please select an active Category.",
                StatusCode = 400
            };
        }

        // uq_subcategories_category_name covers cancelled rows too, so they are not excluded here.
        var name = input.SubCategoryName.Trim();
        var duplicate = await _db.SubCategories.AnyAsync(
            x => x.FkCategory == input.FkCategory && x.SubCategoryName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputSubCategoryCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A sub-category with this name already exists in the selected category.",
                StatusCode = 409
            };
        }

        var entity = new SubCategory
        {
            FkCategory = input.FkCategory,
            SubCategoryName = name,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            DisplayOrder = input.DisplayOrder,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.SubCategories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputSubCategoryCreate { IdSubCategory = entity.IdSubcategory, SubCategoryName = entity.SubCategoryName };
    }

    public async Task<OutputSubCategoryUpdate> UpdateSubCategoryAsync(InputSubCategoryUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.SubCategories.FirstOrDefaultAsync(x => x.IdSubcategory == input.IdSubCategory && !x.Cancelled, ct);
        if (entity is null) return new OutputSubCategoryUpdate { ResponseCode = 1, ResponseMsg = "Sub-category not found.", StatusCode = 404 };

        // Only a re-parent needs an active target; an unchanged (possibly inactive) parent must not block edits or status toggles.
        if (entity.FkCategory != input.FkCategory)
        {
            var parent = await _db.Categories
                .Where(x => x.IdCategory == input.FkCategory && !x.Cancelled)
                .Select(x => new { x.IsActive })
                .FirstOrDefaultAsync(ct);
            if (parent is null || !parent.IsActive)
            {
                return new OutputSubCategoryUpdate
                {
                    ResponseCode = 1,
                    ResponseMsg = "Please select an active Category.",
                    StatusCode = 400
                };
            }
        }

        var name = input.SubCategoryName.Trim();
        var duplicate = await _db.SubCategories.AnyAsync(
            x => x.IdSubcategory != input.IdSubCategory
                 && x.FkCategory == input.FkCategory
                 && x.SubCategoryName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputSubCategoryUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A sub-category with this name already exists in the selected category.",
                StatusCode = 409
            };
        }

        entity.FkCategory = input.FkCategory;
        entity.SubCategoryName = name;
        entity.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return new OutputSubCategoryUpdate { IdSubCategory = entity.IdSubcategory };
    }

    // ═══════════════════════════════════════════════════════════════
    // BRAND
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputBrandSelect?> GetBrandAsync(InputBrandSelect input, CancellationToken ct = default)
        => await _db.Brands
            .Where(x => x.IdBrand == input.IdBrand && !x.Cancelled)
            .Select(x => new OutputBrandSelect { IdBrand = x.IdBrand, BrandName = x.BrandName, Description = x.Description, IsActive = x.IsActive })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputBrandList> ListBrandsAsync(InputBrandList input, CancellationToken ct = default)
    {
        var q = _db.Brands.Where(x => !x.Cancelled);

        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(x => x.BrandName.ToLower().Contains(term));
        }

        q = input.SortBy.ToLowerInvariant() switch
        {
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.BrandName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.BrandName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.BrandName)
                : q.OrderBy(x => x.BrandName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputBrandSelect { IdBrand = x.IdBrand, BrandName = x.BrandName, Description = x.Description, IsActive = x.IsActive })
            .ToListAsync(ct);

        return new OutputBrandList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputBrandCreate> CreateBrandAsync(InputBrandCreate input, CancellationToken ct = default)
    {
        // uq_brands_brandname covers cancelled rows too, so they are not excluded here.
        var name = input.BrandName.Trim();
        var duplicate = await _db.Brands.AnyAsync(x => x.BrandName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputBrandCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A brand with this name already exists.",
                StatusCode = 409
            };
        }

        var entity = new Brand
        {
            BrandName = name,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Brands.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputBrandCreate { IdBrand = entity.IdBrand, BrandName = entity.BrandName };
    }

    public async Task<OutputBrandUpdate> UpdateBrandAsync(InputBrandUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Brands.FirstOrDefaultAsync(x => x.IdBrand == input.IdBrand && !x.Cancelled, ct);
        if (entity is null) return new OutputBrandUpdate { ResponseCode = 1, ResponseMsg = "Brand not found.", StatusCode = 404 };

        var name = input.BrandName.Trim();
        var duplicate = await _db.Brands.AnyAsync(
            x => x.IdBrand != input.IdBrand && x.BrandName.ToLower() == name.ToLower(), ct);
        if (duplicate)
        {
            return new OutputBrandUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A brand with this name already exists.",
                StatusCode = 409
            };
        }

        entity.BrandName = name;
        entity.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new OutputBrandUpdate { IdBrand = entity.IdBrand };
    }

    // ═══════════════════════════════════════════════════════════════
    // UNIT
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputUnitSelect?> GetUnitAsync(InputUnitSelect input, CancellationToken ct = default)
        => await _db.Units
            .Where(x => x.IdUnit == input.IdUnit && !x.Cancelled)
            .Select(x => new OutputUnitSelect { IdUnit = x.IdUnit, UnitCode = x.UnitCode, UnitName = x.UnitName, IsActive = x.IsActive })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputUnitList> ListUnitsAsync(InputUnitList input, CancellationToken ct = default)
    {
        var q = _db.Units.Where(x => !x.Cancelled);

        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(x => x.UnitName.ToLower().Contains(term) || x.UnitCode.ToLower().Contains(term));
        }

        q = input.SortBy.ToLowerInvariant() switch
        {
            "unitcode" or "code" => input.SortDesc
                ? q.OrderByDescending(x => x.UnitCode).ThenBy(x => x.UnitName)
                : q.OrderBy(x => x.UnitCode).ThenBy(x => x.UnitName),
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.UnitName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.UnitName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.UnitName)
                : q.OrderBy(x => x.UnitName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputUnitSelect { IdUnit = x.IdUnit, UnitCode = x.UnitCode, UnitName = x.UnitName, IsActive = x.IsActive })
            .ToListAsync(ct);

        return new OutputUnitList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputUnitCreate> CreateUnitAsync(InputUnitCreate input, CancellationToken ct = default)
    {
        var code = input.UnitCode.Trim();
        var name = input.UnitName.Trim();

        // uq_units_unitcode covers cancelled rows too, so they are not excluded for the code check.
        var codeTaken = await _db.Units.AnyAsync(x => x.UnitCode.ToLower() == code.ToLower(), ct);
        if (codeTaken)
        {
            return new OutputUnitCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A unit with this code already exists.",
                StatusCode = 409
            };
        }

        var nameTaken = await _db.Units.AnyAsync(x => !x.Cancelled && x.UnitName.ToLower() == name.ToLower(), ct);
        if (nameTaken)
        {
            return new OutputUnitCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A unit with this name already exists.",
                StatusCode = 409
            };
        }

        var entity = new Unit { UnitCode = code, UnitName = name, IsActive = input.IsActive, CreatedAt = DateTime.UtcNow };
        _db.Units.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputUnitCreate { IdUnit = entity.IdUnit, UnitCode = entity.UnitCode };
    }

    public async Task<OutputUnitUpdate> UpdateUnitAsync(InputUnitUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Units.FirstOrDefaultAsync(x => x.IdUnit == input.IdUnit && !x.Cancelled, ct);
        if (entity is null) return new OutputUnitUpdate { ResponseCode = 1, ResponseMsg = "Unit not found.", StatusCode = 404 };

        var code = input.UnitCode.Trim();
        var name = input.UnitName.Trim();

        var codeTaken = await _db.Units.AnyAsync(
            x => x.IdUnit != input.IdUnit && x.UnitCode.ToLower() == code.ToLower(), ct);
        if (codeTaken)
        {
            return new OutputUnitUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A unit with this code already exists.",
                StatusCode = 409
            };
        }

        var nameTaken = await _db.Units.AnyAsync(
            x => !x.Cancelled && x.IdUnit != input.IdUnit && x.UnitName.ToLower() == name.ToLower(), ct);
        if (nameTaken)
        {
            return new OutputUnitUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A unit with this name already exists.",
                StatusCode = 409
            };
        }

        entity.UnitCode = code;
        entity.UnitName = name;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new OutputUnitUpdate { IdUnit = entity.IdUnit };
    }

    // ═══════════════════════════════════════════════════════════════
    // TAX
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputTaxSelect?> GetTaxAsync(InputTaxSelect input, CancellationToken ct = default)
        => await _db.Taxes
            .Where(x => x.IdTax == input.IdTax && !x.Cancelled)
            .Select(x => new OutputTaxSelect { IdTax = x.IdTax, TaxCode = x.TaxCode, TaxName = x.TaxName, TaxRate = x.TaxRate, IsInclusive = x.IsInclusive, IsActive = x.IsActive })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputTaxList> ListTaxesAsync(InputTaxList input, CancellationToken ct = default)
    {
        var q = _db.Taxes.Where(x => !x.Cancelled);

        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(x => x.TaxName.ToLower().Contains(term) || x.TaxCode.ToLower().Contains(term));
        }

        q = input.SortBy.ToLowerInvariant() switch
        {
            "taxcode" or "code" => input.SortDesc
                ? q.OrderByDescending(x => x.TaxCode).ThenBy(x => x.TaxName)
                : q.OrderBy(x => x.TaxCode).ThenBy(x => x.TaxName),
            "taxrate" or "rate" => input.SortDesc
                ? q.OrderByDescending(x => x.TaxRate).ThenBy(x => x.TaxName)
                : q.OrderBy(x => x.TaxRate).ThenBy(x => x.TaxName),
            "isinclusive" => input.SortDesc
                ? q.OrderByDescending(x => x.IsInclusive).ThenBy(x => x.TaxName)
                : q.OrderBy(x => x.IsInclusive).ThenBy(x => x.TaxName),
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.TaxName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.TaxName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.TaxName)
                : q.OrderBy(x => x.TaxName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputTaxSelect { IdTax = x.IdTax, TaxCode = x.TaxCode, TaxName = x.TaxName, TaxRate = x.TaxRate, IsInclusive = x.IsInclusive, IsActive = x.IsActive })
            .ToListAsync(ct);

        return new OutputTaxList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputTaxCreate> CreateTaxAsync(InputTaxCreate input, CancellationToken ct = default)
    {
        var code = input.TaxCode.Trim();
        var name = input.TaxName.Trim();

        // uq_taxes_taxcode covers cancelled rows too, so they are not excluded for the code check.
        var codeTaken = await _db.Taxes.AnyAsync(x => x.TaxCode.ToLower() == code.ToLower(), ct);
        if (codeTaken)
        {
            return new OutputTaxCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A tax with this code already exists.",
                StatusCode = 409
            };
        }

        var nameTaken = await _db.Taxes.AnyAsync(x => !x.Cancelled && x.TaxName.ToLower() == name.ToLower(), ct);
        if (nameTaken)
        {
            return new OutputTaxCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A tax with this name already exists.",
                StatusCode = 409
            };
        }

        var entity = new Tax
        {
            TaxCode = code,
            TaxName = name,
            TaxRate = Math.Round(input.TaxRate, 4, MidpointRounding.AwayFromZero),
            IsInclusive = input.IsInclusive,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Taxes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputTaxCreate { IdTax = entity.IdTax, TaxCode = entity.TaxCode };
    }

    public async Task<OutputTaxUpdate> UpdateTaxAsync(InputTaxUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Taxes.FirstOrDefaultAsync(x => x.IdTax == input.IdTax && !x.Cancelled, ct);
        if (entity is null) return new OutputTaxUpdate { ResponseCode = 1, ResponseMsg = "Tax not found.", StatusCode = 404 };

        var code = input.TaxCode.Trim();
        var name = input.TaxName.Trim();

        var codeTaken = await _db.Taxes.AnyAsync(
            x => x.IdTax != input.IdTax && x.TaxCode.ToLower() == code.ToLower(), ct);
        if (codeTaken)
        {
            return new OutputTaxUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A tax with this code already exists.",
                StatusCode = 409
            };
        }

        var nameTaken = await _db.Taxes.AnyAsync(
            x => !x.Cancelled && x.IdTax != input.IdTax && x.TaxName.ToLower() == name.ToLower(), ct);
        if (nameTaken)
        {
            return new OutputTaxUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A tax with this name already exists.",
                StatusCode = 409
            };
        }

        entity.TaxCode = code;
        entity.TaxName = name;
        entity.TaxRate = Math.Round(input.TaxRate, 4, MidpointRounding.AwayFromZero);
        entity.IsInclusive = input.IsInclusive;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new OutputTaxUpdate { IdTax = entity.IdTax };
    }

    // ═══════════════════════════════════════════════════════════════
    // LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<OutputCategorySelect>> GetCategoryLookupAsync(CancellationToken ct = default)
        => _db.Categories.Where(x => !x.Cancelled && x.IsActive)
            .OrderBy(x => x.CategoryName)
            .Select(x => new OutputCategorySelect { IdCategory = x.IdCategory, CategoryName = x.CategoryName, IsActive = x.IsActive })
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputCategorySelect>>(t => t.Result, ct);

    public async Task<IReadOnlyList<OutputCategorySelect>> GetCategoryOptionsAsync(CancellationToken ct = default)
        => await _db.Categories
            .Where(x => !x.Cancelled)
            .OrderBy(x => x.CategoryName)
            .Select(x => new OutputCategorySelect { IdCategory = x.IdCategory, CategoryName = x.CategoryName, IsActive = x.IsActive })
            .ToListAsync(ct);

    public Task<IReadOnlyList<OutputSubCategorySelect>> GetSubCategoryLookupAsync(int fkCategory, CancellationToken ct = default)
        => _db.SubCategories.Where(x => !x.Cancelled && x.IsActive && x.FkCategory == fkCategory)
            .OrderBy(x => x.SubCategoryName)
            .Select(x => new OutputSubCategorySelect { IdSubCategory = x.IdSubcategory, FkCategory = x.FkCategory, SubCategoryName = x.SubCategoryName, IsActive = x.IsActive })
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputSubCategorySelect>>(t => t.Result, ct);

    public Task<IReadOnlyList<OutputBrandSelect>> GetBrandLookupAsync(CancellationToken ct = default)
        => _db.Brands.Where(x => !x.Cancelled && x.IsActive)
            .OrderBy(x => x.BrandName)
            .Select(x => new OutputBrandSelect { IdBrand = x.IdBrand, BrandName = x.BrandName, IsActive = x.IsActive })
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputBrandSelect>>(t => t.Result, ct);

    public Task<IReadOnlyList<OutputUnitSelect>> GetUnitLookupAsync(CancellationToken ct = default)
        => _db.Units.Where(x => !x.Cancelled && x.IsActive)
            .OrderBy(x => x.UnitName)
            .Select(x => new OutputUnitSelect { IdUnit = x.IdUnit, UnitCode = x.UnitCode, UnitName = x.UnitName, IsActive = x.IsActive })
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputUnitSelect>>(t => t.Result, ct);

    public Task<IReadOnlyList<OutputTaxSelect>> GetTaxLookupAsync(CancellationToken ct = default)
        => _db.Taxes.Where(x => !x.Cancelled && x.IsActive)
            .OrderBy(x => x.TaxName)
            .Select(x => new OutputTaxSelect { IdTax = x.IdTax, TaxCode = x.TaxCode, TaxName = x.TaxName, TaxRate = x.TaxRate, IsInclusive = x.IsInclusive, IsActive = x.IsActive })
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputTaxSelect>>(t => t.Result, ct);
}
