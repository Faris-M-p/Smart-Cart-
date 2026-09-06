using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.EmployeeModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeInterface _employeeInterface;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(
            IEmployeeInterface employeeInterface,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _employeeInterface = employeeInterface;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [HttpGet("/admin/employees")]
        [Route("")]
        [Route("Index")]
        [RequirePermission("Employees.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Employee/Index.cshtml");
        }

        [HttpPost]
        [Route("GetEmployeeList")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetEmployeeList([FromBody] EmployeeListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Employee>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new EmployeeListInput
                {
                    SearchText = viewInput.SearchText,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _employeeInterface.GetEmployeeListAsync(input);
                return Ok(new ApiResponse<TableOutput<Employee>>
                {
                    Success = true,
                    Message = "Employees loaded successfully",
                    Data = result
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _employeeInterface.GetEmployeeByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Employee not found." });
                }

                return Ok(row);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetActiveUserRoles")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetActiveUserRoles()
        {
            try
            {
                var rows = await _employeeInterface.GetActiveUserRolesAsync();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Create")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequirePermission("Employees.Create")]
        public async Task<IActionResult> Create([FromForm] EmployeeUpdateInputVIEW viewInput)
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

                if (viewInput.ProfileImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.ProfileImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var result = await _employeeInterface.CreateEmployeeAsync(MapWriteInput(viewInput));
                if (result.StatusCode)
                {
                    await ApplyProfileImageAsync((int)result.ResponseCode, viewInput);
                }

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequirePermission("Employees.Edit")]
        public async Task<IActionResult> Update([FromForm] EmployeeUpdateInputVIEW viewInput)
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

                if (viewInput.ProfileImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.ProfileImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var result = await _employeeInterface.UpdateEmployeeAsync(MapWriteInput(viewInput));
                if (result.StatusCode)
                {
                    await ApplyProfileImageAsync(viewInput.EmployeeID, viewInput);
                }

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("Employees.Deactivate")]
        public async Task<IActionResult> Delete([FromBody] EmployeeDeleteInputVIEW viewInput)
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

                var input = new EmployeeDeleteInput
                {
                    EmployeeID = viewInput.EmployeeID,
                    CancelledReason = viewInput.CancelledReason
                };

                var result = await _employeeInterface.DeleteEmployeeAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        private async Task ApplyProfileImageAsync(int employeeId, EmployeeUpdateInputVIEW viewInput)
        {
            if (viewInput.ProfileImage == null && !viewInput.RemoveProfileImage)
            {
                return;
            }

            var current = await _employeeInterface.GetEmployeeByIdAsync(employeeId);
            if (current == null)
            {
                return;
            }

            var previousUrl = current.ProfileImageUrl;
            string? nextUrl = previousUrl;

            if (viewInput.ProfileImage != null)
            {
                var folder = _commonImageService.GetEmployeeUploadRoot(_environment.WebRootPath, employeeId);
                var fileName = await _commonImageService.SaveEmployeeProfileAsync(viewInput.ProfileImage, folder);
                nextUrl = _commonImageService.BuildEmployeeImageUrl(employeeId, fileName);
            }
            else if (viewInput.RemoveProfileImage)
            {
                nextUrl = null;
            }

            if (!string.Equals(previousUrl, nextUrl, StringComparison.OrdinalIgnoreCase))
            {
                await _employeeInterface.SetProfileImageUrlAsync(employeeId, nextUrl);
                if (!string.IsNullOrWhiteSpace(previousUrl) &&
                    !string.Equals(previousUrl, nextUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _commonImageService.TryDeleteByUrl(_environment.WebRootPath, previousUrl);
                }
            }
        }

        private static EmployeeUpdateInput MapWriteInput(EmployeeUpdateInputVIEW viewInput) =>
            new()
            {
                EmployeeID = viewInput.EmployeeID,
                EmployeeName = viewInput.EmployeeName,
                UserName = viewInput.UserName,
                Password = viewInput.Password,
                ConfirmPassword = viewInput.ConfirmPassword,
                FK_UserRole = viewInput.FK_UserRole,
                IsActive = viewInput.IsActive ?? true
            };
    }
}
