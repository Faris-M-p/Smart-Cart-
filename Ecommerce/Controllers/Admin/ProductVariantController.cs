using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/ProductVariant")]
    public class ProductVariantController : Controller
    {
        private readonly IProductVariantInterface _productVariantInterface;

        public ProductVariantController(IProductVariantInterface productVariantInterface)
        {
            _productVariantInterface = productVariantInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/ProductVariant/Index.cshtml");
        }

        [HttpPost]
        [Route("GetProductVariantList")]
        [Route("GetList")]
        public async Task<IActionResult> GetProductVariantList([FromBody] ProductVariantListInputVIEW viewInput)
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

                var input = new ProductVariantListInput
                {
                    FK_Product = viewInput.FK_Product,
                    SearchText = viewInput.SearchText,
                    FilterVariantIDs = viewInput.FilterVariantIDs,
                    FilterVariantValueIDs = viewInput.FilterVariantValueIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _productVariantInterface.GetProductVariantListAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _productVariantInterface.GetProductVariantByIdAsync(id);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound(new { message = "Product Variant not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] ProductVariantUpdateInputVIEW viewInput)
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

                if (viewInput.VariantValues == null || viewInput.VariantValues.Count == 0)
                {
                    return BadRequest(new { message = "At least one variant value row is required." });
                }

                var input = new ProductVariantUpdateInput
                {
                    ID_ProductVariant = 0,
                    FK_Product = viewInput.FK_Product,
                    SKU = viewInput.SKU,
                    VariantLabel = viewInput.VariantLabel,
                    MRP = viewInput.MRP,
                    SellingPrice = viewInput.SellingPrice,
                    IsActive = viewInput.IsActive,
                    IsDefault = viewInput.IsDefault,
                    VariantValues = viewInput.VariantValues
                        .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                        .Select(v => new ProductVariantValueRowInput
                        {
                            VariantId = v.VariantId,
                            VariantValueId = v.VariantValueId
                        })
                        .ToList(),
                    EnterBy = 1
                };

                var result = await _productVariantInterface.CreateProductVariantAsync(input);
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
        public async Task<IActionResult> Update([FromBody] ProductVariantUpdateInputVIEW viewInput)
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

                if (viewInput.VariantValues == null || viewInput.VariantValues.Count == 0)
                {
                    return BadRequest(new { message = "At least one variant value row is required." });
                }

                var input = new ProductVariantUpdateInput
                {
                    ID_ProductVariant = viewInput.ID_ProductVariant,
                    FK_Product = viewInput.FK_Product,
                    SKU = viewInput.SKU,
                    VariantLabel = viewInput.VariantLabel,
                    MRP = viewInput.MRP,
                    SellingPrice = viewInput.SellingPrice,
                    IsActive = viewInput.IsActive,
                    IsDefault = viewInput.IsDefault,
                    VariantValues = viewInput.VariantValues
                        .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                        .Select(v => new ProductVariantValueRowInput
                        {
                            VariantId = v.VariantId,
                            VariantValueId = v.VariantValueId
                        })
                        .ToList(),
                    EnterBy = 1
                };

                var result = await _productVariantInterface.UpdateProductVariantAsync(input);
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
        public async Task<IActionResult> Delete([FromBody] ProductVariantDeleteInputVIEW viewInput)
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

                var input = new ProductVariantDeleteInput
                {
                    ID_ProductVariant = viewInput.ID_ProductVariant,
                    EnterBy = 1
                };

                var result = await _productVariantInterface.DeleteProductVariantAsync(input);
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
    }
}
