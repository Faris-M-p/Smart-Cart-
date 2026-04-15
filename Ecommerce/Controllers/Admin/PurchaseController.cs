using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.PurchaseModel;
using static Ecommerce.Models.Admin.SupplierModel;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.CommonModel;
using System.Linq;
using System.Text.Json;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Purchase")]
    public class PurchaseController : Controller
    {
        private readonly IPurchaseInterface _purchaseInterface;
        private readonly ISupplierInterface _supplierInterface;
        private readonly IProductVariantInterface _productVariantInterface;
        private readonly IProductInterface _productInterface;

        public PurchaseController(
            IPurchaseInterface purchaseInterface,
            ISupplierInterface supplierInterface,
            IProductVariantInterface productVariantInterface,
            IProductInterface productInterface)
        {
            _purchaseInterface = purchaseInterface;
            _supplierInterface = supplierInterface;
            _productVariantInterface = productVariantInterface;
            _productInterface = productInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Purchase/Index.cshtml");
        }

        [HttpPost]
        [Route("GetPurchaseList")]
        public async Task<IActionResult> GetPurchaseList([FromBody] PurchaseListInputVIEW viewInput)
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

                var input = new PurchaseListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterSupplierIDs = viewInput.FilterSupplierIDs,
                    FromDate = viewInput.FromDate,
                    ToDate = viewInput.ToDate,
                    PaymentStatus = viewInput.PaymentStatus,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _purchaseInterface.GetPurchaseListAsync(input);
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
                var result = await _purchaseInterface.GetPurchaseByIdAsync(id);

                if (result != null && result.PurchaseHeader.ID_Purchase > 0)
                {
                    return Ok(result);
                }

                return NotFound(new { message = "Purchase not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                // Get all active suppliers for dropdown
                var input = new SupplierListInput
                {
                    SearchText = string.Empty,
                    FilterSupplierIDs = string.Empty,
                    PageIndex = 1,
                    PageSize = 1000,
                    SortColumn = (int)SupplierSortColumn.Name,
                    SortMode = "ASC"
                };
                var result = await _supplierInterface.GetSupplierListAsync(input);
                var suppliers = result.TableData?
                    .Where(s => !s.Cancelled && s.IsActive)
                    .ToList() ?? new List<Supplier>();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while fetching suppliers: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var input = new ProductListInput
                {
                    SearchText = string.Empty,
                    FilterCategoryIDs = string.Empty,
                    FilterSubCategoryIDs = string.Empty,
                    FilterBrandIDs = string.Empty,
                    PageIndex = 1,
                    PageSize = 1000,
                    SortColumn = (int)ProductSortColumn.Name,
                    SortMode = "ASC"
                };
                var result = await _productInterface.GetProductListAsync(input);
                var products = result.TableData?.Where(p => !p.Cancelled).ToList() ?? new List<Product>();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while fetching products: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetProductVariants/{productId}")]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            try
            {
                var input = new ProductVariantListInput
                {
                    FK_Product = productId,
                    SearchText = string.Empty,
                    FilterVariantIDs = string.Empty,
                    FilterVariantValueIDs = string.Empty,
                    PageIndex = 1,
                    PageSize = 1000,
                    SortColumn = (int)ProductVariantSortColumn.Id,
                    SortMode = "ASC"
                };
                var result = await _productVariantInterface.GetProductVariantListAsync(input);
                var variants = result.TableData?.ToList() ?? new List<ProductVariant>();
                return Ok(variants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while fetching product variants: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] PurchaseUpdateInputVIEW viewInput)
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

                // Validate PurchaseDetails
                if (viewInput.PurchaseDetails == null || viewInput.PurchaseDetails.Count == 0)
                {
                    return BadRequest(new { message = "At least one Purchase Detail is required." });
                }

                // Convert PurchaseDetails to JSON
                var purchaseDetailsJson = JsonSerializer.Serialize(viewInput.PurchaseDetails.Select(pd => new
                {
                    ID_PurchaseDetail = pd.ID_PurchaseDetail,
                    FK_ProductVariant = pd.FK_ProductVariant,
                    Quantity = pd.Quantity,
                    PurchasePrice = pd.PurchasePrice,
                    MRP = pd.MRP,
                    ExpiryDate = pd.ExpiryDate?.ToString("yyyy-MM-dd")
                }));

                var input = new PurchaseUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    ID_Purchase = viewInput.ID_Purchase,
                    FK_Supplier = viewInput.FK_Supplier,
                    PurchaseDate = viewInput.PurchaseDate,
                    InvoiceNumber = viewInput.InvoiceNumber,
                    Notes = viewInput.Notes,
                    PurchaseDetails = purchaseDetailsJson,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _purchaseInterface.CreatePurchaseAsync(input);
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
        public async Task<IActionResult> Update([FromBody] PurchaseUpdateInputVIEW viewInput)
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

                // Validate PurchaseDetails
                if (viewInput.PurchaseDetails == null || viewInput.PurchaseDetails.Count == 0)
                {
                    return BadRequest(new { message = "At least one Purchase Detail is required." });
                }

                // Convert PurchaseDetails to JSON
                var purchaseDetailsJson = JsonSerializer.Serialize(viewInput.PurchaseDetails.Select(pd => new
                {
                    ID_PurchaseDetail = pd.ID_PurchaseDetail,
                    FK_ProductVariant = pd.FK_ProductVariant,
                    Quantity = pd.Quantity,
                    PurchasePrice = pd.PurchasePrice,
                    MRP = pd.MRP,
                    ExpiryDate = pd.ExpiryDate?.ToString("yyyy-MM-dd")
                }));

                var input = new PurchaseUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    ID_Purchase = viewInput.ID_Purchase,
                    FK_Supplier = viewInput.FK_Supplier,
                    PurchaseDate = viewInput.PurchaseDate,
                    InvoiceNumber = viewInput.InvoiceNumber,
                    Notes = viewInput.Notes,
                    PurchaseDetails = purchaseDetailsJson,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _purchaseInterface.UpdatePurchaseAsync(input);
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
        public async Task<IActionResult> Delete([FromBody] PurchaseDeleteInputVIEW viewInput)
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

                var input = new PurchaseDeleteInput
                {
                    UserAction = 3, // 3 = Delete
                    ID_Purchase = viewInput.ID_Purchase,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _purchaseInterface.DeletePurchaseAsync(input);
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
