using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;

namespace Ecommerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OrderInterface _orderInterface;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CheckoutController(
            OrderInterface orderInterface,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _orderInterface = orderInterface;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Checkout";
            return View();
        }

        [HttpGet("Checkout/Confirmation/{orderId:int}")]
        public IActionResult Confirmation(int orderId)
        {
            ViewBag.Title = "Order placed";
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet("Checkout/Location/IndiaStates")]
        public async Task<IActionResult> GetIndiaStates(CancellationToken cancellationToken)
        {
            var apiKey = _configuration["CountryStateCity:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Content("[]", "application/json");
            }

            var client = _httpClientFactory.CreateClient("CountryStateCity");
            using var request = new HttpRequestMessage(HttpMethod.Get, "countries/IN/states");
            request.Headers.TryAddWithoutValidation("X-CSCAPI-KEY", apiKey.Trim());

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ContentResult
            {
                Content = body,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        [HttpGet("Checkout/Location/IndiaCities/{stateIso}")]
        public async Task<IActionResult> GetIndiaCitiesForState(string stateIso, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["CountryStateCity:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Content("[]", "application/json");
            }

            var code = (stateIso ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length == 0 || code.Length > 10)
            {
                return Content("[]", "application/json");
            }

            var client = _httpClientFactory.CreateClient("CountryStateCity");
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"countries/IN/states/{Uri.EscapeDataString(code)}/cities");
            request.Headers.TryAddWithoutValidation("X-CSCAPI-KEY", apiKey.Trim());

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ContentResult
            {
                Content = body,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        [HttpGet]
        public async Task<IActionResult> Preview([FromQuery] int productVariantId = 0, [FromQuery] int quantity = 1)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.GetPreviewAsync(userId, productVariantId, quantity));
        }

        [HttpPost]
        public async Task<IActionResult> Place([FromBody] PlaceOrderInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.PlaceOrderAsync(userId, input ?? new PlaceOrderInput()));
        }

        [HttpGet]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            var page = await _orderInterface.GetOrderAsync(userId, orderId);
            if (page == null)
            {
                return NotFound();
            }

            return Ok(page);
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.GetUserAddressesAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> SaveAddress([FromBody] Ecommerce.Models.UserAddressModel.SaveAddressInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.SaveUserAddressAsync(userId, input ?? new Ecommerce.Models.UserAddressModel.SaveAddressInput()));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress([FromBody] int addressId)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.DeleteUserAddressAsync(userId, addressId));
        }

        private bool TryGetUserId(out int userId, out IActionResult unauthorized)
        {
            if (UserAuthHelper.TryGetUserId(User, out userId))
            {
                unauthorized = null!;
                return true;
            }

            unauthorized = Unauthorized(new CommonResponse
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = UserAuthHelper.UnauthorizedMessage
            });
            return false;
        }
    }
}
