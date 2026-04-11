using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.CommonModel;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Ecommerce.DataAccess;
using Ecommerce.Interface;
using Ecommerce.Models.Enums;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private readonly IProductInterface _productInterface;
        private readonly ICategoryInterface _categoryInterface;
        private readonly ISubCategoryInterface _subCategoryInterface;
        private readonly IDataAccessDapper _dataAccessDapper;

        public ProductController(
            IProductInterface productInterface,
            ICategoryInterface categoryInterface,
            ISubCategoryInterface subCategoryInterface,
            IDataAccessDapper dataAccessDapper)
        {
            _productInterface = productInterface;
            _categoryInterface = categoryInterface;
            _subCategoryInterface = subCategoryInterface;
            _dataAccessDapper = dataAccessDapper;
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
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                // Map VIEW model to Procedure Input model
                var input = new ProductListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterCategoryIDs = viewInput.FilterCategoryIDs,
                    FilterSubCategoryIDs = viewInput.FilterSubCategoryIDs,
                    FilterBrandIDs = viewInput.FilterBrandIDs,
                    FilterStatusIDs = viewInput.FilterStatusIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _productInterface.GetProductListAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
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

                // Validate MRP >= Price
                if (viewInput.MRP.HasValue && viewInput.MRP < viewInput.Price)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "MRP must be greater than or equal to Price." } });
                }

                // Map VIEW model to Procedure Input model
                var input = new ProductUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    ID_Product = viewInput.ID_Product,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    Price = viewInput.Price,
                    MRP = viewInput.MRP,
                    FK_Category = viewInput.FK_Category,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    Rating = viewInput.Rating,
                    Gender = viewInput.Gender,
                    FK_Status = viewInput.FK_Status,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _productInterface.CreateProductAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
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

                // Validate MRP >= Price
                if (viewInput.MRP.HasValue && viewInput.MRP < viewInput.Price)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "MRP must be greater than or equal to Price." } });
                }

                // Map VIEW model to Procedure Input model
                var input = new ProductUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    ID_Product = viewInput.ID_Product,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    Price = viewInput.Price,
                    MRP = viewInput.MRP,
                    FK_Category = viewInput.FK_Category,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    Rating = viewInput.Rating,
                    Gender = viewInput.Gender,
                    FK_Status = viewInput.FK_Status,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _productInterface.UpdateProductAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
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

                // Map VIEW model to Procedure Input model
                var input = new ProductDeleteInput
                {
                    ID_Product = viewInput.ID_Product,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _productInterface.DeleteProductAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetBrands")]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                using var connection = _dataAccessDapper.CreateConnection();
                
                var brands = await connection.QueryAsync<Brand>(@"
                    SELECT BrandId AS ID_Brand, BrandName AS Name
                    FROM Brands
                    WHERE Cancelled = 0
                    ORDER BY BrandName ASC
                ");
                
                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetProductStatuses")]
        public async Task<IActionResult> GetProductStatuses()
        {
            try
            {
                using var connection = _dataAccessDapper.CreateConnection();
                
                var statuses = await connection.QueryAsync<ProductStatus>(@"
                    SELECT StatusId AS ID_Status, StatusName AS Name
                    FROM ProductStatus
                    WHERE Cancelled = 0
                    ORDER BY StatusName ASC
                ");
                
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Helper classes for dropdowns
        private class Brand
        {
            public int ID_Brand { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class ProductStatus
        {
            public int ID_Status { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
