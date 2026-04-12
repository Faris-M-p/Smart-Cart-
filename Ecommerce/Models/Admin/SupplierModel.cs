using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class SupplierModel
    {
        public class SupplierListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Supplier IDs")]
            public string FilterSupplierIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 4, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class SupplierUpdateInputVIEW
        {
            [Display(Name = "Supplier ID")]
            public int SupplierId { get; set; }

            [Display(Name = "Supplier Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(250, ErrorMessage = "{0} cannot exceed 250 characters.")]
            public string SupplierName { get; set; } = string.Empty;

            [Display(Name = "Company Name")]
            [MaxLength(250, ErrorMessage = "{0} cannot exceed 250 characters.")]
            public string CompanyName { get; set; } = string.Empty;

            [Display(Name = "Phone")]
            [MaxLength(20, ErrorMessage = "{0} cannot exceed 20 characters.")]
            public string Phone { get; set; } = string.Empty;

            [Display(Name = "Email")]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            [MaxLength(250, ErrorMessage = "{0} cannot exceed 250 characters.")]
            public string Email { get; set; } = string.Empty;

            [Display(Name = "State")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(150, ErrorMessage = "{0} cannot exceed 150 characters.")]
            public string State { get; set; } = string.Empty;

            [Display(Name = "District")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(150, ErrorMessage = "{0} cannot exceed 150 characters.")]
            public string District { get; set; } = string.Empty;

            [Display(Name = "City")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(150, ErrorMessage = "{0} cannot exceed 150 characters.")]
            public string City { get; set; } = string.Empty;

            [Display(Name = "Address")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Address { get; set; } = string.Empty;

            [Display(Name = "Pincode")]
            [MaxLength(10, ErrorMessage = "{0} cannot exceed 10 characters.")]
            public string Pincode { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [MaxLength(1000, ErrorMessage = "{0} cannot exceed 1000 characters.")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Active")]
            public bool? IsActive { get; set; }
        }

        public class SupplierDeleteInputVIEW
        {
            [Display(Name = "Supplier ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int SupplierId { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class SupplierListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string FilterSupplierIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = string.Empty;
        }

        public class SupplierUpdateInput
        {
            public int UserAction { get; set; }
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string District { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public int EnterBy { get; set; } = 1;
        }

        public class SupplierDeleteInput
        {
            public int SupplierId { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
            public int EnterBy { get; set; } = 1;
        }

        public class Supplier
        {
            public int SupplierID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? CompanyName { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string State { get; set; } = string.Empty;
            public string District { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string? Address { get; set; }
            public string? Pincode { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public bool Cancelled { get; set; }
        }
    }
}
