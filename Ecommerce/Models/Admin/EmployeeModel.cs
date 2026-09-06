using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ecommerce.CustomModelValidation;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.Admin
{
    public class EmployeeModel
    {
        public class EmployeeListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class EmployeeUpdateInputVIEW
        {
            [Display(Name = "Employee ID")]
            public int EmployeeID { get; set; } = 0;

            [Display(Name = "Employee Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(150, ErrorMessage = "{0} cannot exceed 150 characters.")]
            public string EmployeeName { get; set; } = string.Empty;

            [Display(Name = "Username")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(50, ErrorMessage = "{0} cannot exceed 50 characters.")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "Password")]
            [MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Confirm Password")]
            [MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "User Role")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            [JsonPropertyName("fk_UserRole")]
            public int FK_UserRole { get; set; }

            [Display(Name = "Active")]
            public bool? IsActive { get; set; }

            public IFormFile? ProfileImage { get; set; }

            public bool RemoveProfileImage { get; set; }
        }

        public class EmployeeDeleteInputVIEW
        {
            [Display(Name = "Employee ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int EmployeeID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class EmployeeListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = string.Empty;
        }

        public class EmployeeUpdateInput
        {
            public int EmployeeID { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
            public int FK_UserRole { get; set; }
            public bool IsActive { get; set; } = true;
        }

        public class EmployeeDeleteInput
        {
            public int EmployeeID { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class Employee
        {
            public int EmployeeID { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;

            [JsonPropertyName("fk_UserRole")]
            public int FK_UserRole { get; set; }

            public string UserRoleName { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public bool IsProtected { get; set; }
            public string? ProfileImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class EmployeeRoleOption
        {
            public int UserRoleID { get; set; }
            public string RoleName { get; set; } = string.Empty;
        }
    }
}
