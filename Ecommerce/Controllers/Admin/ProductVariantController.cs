using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;
using System.Linq;
using System.Text.Json;

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

                // Validate VariantAttributes
                if (viewInput.VariantAttributes == null || viewInput.VariantAttributes.Count == 0)
                {
                    return BadRequest(new { message = "At least one Variant Attribute is required." });
                }

                // Convert VariantAttributes to JSON
                var variantAttributesJson = JsonSerializer.Serialize(viewInput.VariantAttributes.Select(va => new
                {
                    FK_Variant = va.FK_Variant,
                    FK_VariantValue = va.FK_VariantValue
                }));

                var input = new ProductVariantUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    ID_ProductVariant = viewInput.ID_ProductVariant,
                    FK_Product = viewInput.FK_Product,
                    PriceAdjustment = viewInput.PriceAdjustment,
                    IsDefault = viewInput.IsDefault,
                    VariantAttributes = variantAttributesJson,
                    EnterBy = 1 // TODO: Get from session/auth
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

                // Validate VariantAttributes (optional for update, but if provided, must be valid)
                string variantAttributesJson = null;
                if (viewInput.VariantAttributes != null && viewInput.VariantAttributes.Count > 0)
                {
                    variantAttributesJson = JsonSerializer.Serialize(viewInput.VariantAttributes.Select(va => new
                    {
                        FK_Variant = va.FK_Variant,
                        FK_VariantValue = va.FK_VariantValue
                    }));
                }

                var input = new ProductVariantUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    ID_ProductVariant = viewInput.ID_ProductVariant,
                    FK_Product = viewInput.FK_Product,
                    PriceAdjustment = viewInput.PriceAdjustment,
                    IsDefault = viewInput.IsDefault,
                    VariantAttributes = variantAttributesJson ?? string.Empty,
                    EnterBy = 1 // TODO: Get from session/auth
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
                    UserAction = 3, // 3 = Delete
                    ID_ProductVariant = viewInput.ID_ProductVariant,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
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
