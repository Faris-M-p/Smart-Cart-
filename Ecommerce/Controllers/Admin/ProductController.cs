using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private readonly IProductInterface _productInterface;
        private readonly ICategoryInterface _categoryInterface;
        private readonly ISubCategoryInterface _subCategoryInterface;
        private readonly IBrandInterface _brandInterface;

        public ProductController(
            IProductInterface productInterface,
            ICategoryInterface categoryInterface,
            ISubCategoryInterface subCategoryInterface,
            IBrandInterface brandInterface)
        {
            _productInterface = productInterface;
            _categoryInterface = categoryInterface;
            _subCategoryInterface = subCategoryInterface;
            _brandInterface = brandInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Product/Index.cshtml");
        }

        [HttpPost]
        [Route("GetProductList")]
        public async Task<IActionResult> GetProductList([FromBody] ProductListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Product>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new ProductListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterCategoryIDs = viewInput.FilterCategoryIDs,
                    FilterSubCategoryIDs = viewInput.FilterSubCategoryIDs,
                    FilterBrandIDs = viewInput.FilterBrandIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _productInterface.GetProductListAsync(input);

                return Ok(new ApiResponse<TableOutput<Product>>
                {
                    Success = true,
                    Message = "Products loaded successfully",
                    Data = result
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] ProductUpdateInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var sub = await _subCategoryInterface.GetSubCategoryByIdAsync(viewInput.FK_SubCategory);
                if (sub == null || sub.Cancelled)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Invalid subcategory." } });
                }

                if (sub.FK_Category != viewInput.FK_Category)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Subcategory does not belong to the selected category." } });
                }

                var input = new ProductUpdateInput
                {
                    ID_Product = 0,
                    Name = viewInput.Name,
                    Slug = viewInput.Slug,
                    Description = viewInput.Description,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    IsActive = viewInput.IsActive,
                    EnterBy = 1
                };

                var result = await _productInterface.CreateProductAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] ProductUpdateInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var sub = await _subCategoryInterface.GetSubCategoryByIdAsync(viewInput.FK_SubCategory);
                if (sub == null || sub.Cancelled)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Invalid subcategory." } });
                }

                if (sub.FK_Category != viewInput.FK_Category)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Subcategory does not belong to the selected category." } });
                }

                var input = new ProductUpdateInput
                {
                    ID_Product = viewInput.ID_Product,
                    Name = viewInput.Name,
                    Slug = viewInput.Slug,
                    Description = viewInput.Description,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    IsActive = viewInput.IsActive,
                    EnterBy = 1
                };

                var result = await _productInterface.UpdateProductAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromBody] ProductDeleteInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var input = new ProductDeleteInput
                {
                    ID_Product = viewInput.ID_Product,
                    EnterBy = 1
                };

                var result = await _productInterface.DeleteProductAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productInterface.GetProductByIdAsync(id);

                if (product != null)
                {
                    return Ok(product);
                }

                return NotFound(new { message = "Product not found." });
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var input = new CategoryListInput
                {
                    PageIndex = 1,
                    PageSize = 1000,
                    SearchText = string.Empty,
                    FilterCategoryIDs = string.Empty,
                    SortColumn = 0,
                    SortMode = "ASC"
                };

                var result = await _categoryInterface.GetCategoryListAsync(input);
                return Ok(result?.TableData ?? new List<Category>());
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetSubCategories")]
        public async Task<IActionResult> GetSubCategories(int? categoryId = null)
        {
            try
            {
                var input = new SubCategoryListInput
                {
                    PageIndex = 1,
                    PageSize = 1000,
                    SearchText = string.Empty,
                    FilterCategoryIDs = categoryId.HasValue ? $"[{{\"ID_Value\":{categoryId}}}]" : string.Empty,
                    FilterSubCategoryIDs = string.Empty,
                    SortColumn = (int)SubCategorySortColumn.Name,
                    SortMode = "ASC"
                };

                var result = await _subCategoryInterface.GetSubCategoryListAsync(input);
                return Ok(result?.TableData ?? new List<SubCategory>());
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetBrands")]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var input = new BrandListInput
                {
                    PageIndex = 1,
                    PageSize = 2000,
                    SearchText = string.Empty,
                    FilterBrandIDs = string.Empty,
                    SortColumn = 0,
                    SortMode = "ASC"
                };

                var result = await _brandInterface.GetBrandListAsync(input);
                var data = result?.TableData;
                if (data == null)
                {
                    return Ok(Array.Empty<object>());
                }

                var rows = data
                    .Where(b => !b.Cancelled && b.IsActive)
                    .Select(b => new { ID_Brand = b.BrandID, Name = b.BrandName })
                    .ToList();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }
    }
}
