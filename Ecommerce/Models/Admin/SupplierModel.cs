using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class SupplierModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class SupplierListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Supplier IDs")]
            public string FilterSupplierIDs { get; set; } = string.Empty;

            [Display(Name = "Show Cancelled")]
            public bool ShowCancelled { get; set; } = false;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 20;

            [Display(Name = "Sort Column")]
            [Range(0, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class SupplierUpdateInputVIEW
        {
            [Display(Name = "Supplier ID")]
            public long ID_Supplier { get; set; } = 0;

            [Display(Name = "Supplier Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string SupplierName { get; set; } = string.Empty;

            [Display(Name = "Contact Person")]
            [System.ComponentModel.DataAnnotations.MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string? ContactPerson { get; set; }

            [Display(Name = "Phone")]
            [System.ComponentModel.DataAnnotations.MaxLength(20, ErrorMessage = "{0} cannot exceed 20 characters.")]
            public string? Phone { get; set; }

            [Display(Name = "Email")]
            [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Invalid email format.")]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string? Email { get; set; }

            [Display(Name = "GST Number")]
            [System.ComponentModel.DataAnnotations.MaxLength(50, ErrorMessage = "{0} cannot exceed 50 characters.")]
            public string? GSTNumber { get; set; }

            [Display(Name = "Address")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string? Address { get; set; }
        }

        public class SupplierDeleteInputVIEW
        {
            [Display(Name = "Supplier ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public long ID_Supplier { get; set; }

            [Display(Name = "Cancelled Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
        public class SupplierListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Supplier IDs")]
            public string FilterSupplierIDs { get; set; } = string.Empty;

            [Display(Name = "Show Cancelled")]
            public bool ShowCancelled { get; set; } = false;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 20;

            [Display(Name = "Sort Column")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class SupplierUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; } // 1 = Add, 2 = Edit, 3 = Delete

            [Display(Name = "Supplier ID")]
            public long ID_Supplier { get; set; } = 0;

            [Display(Name = "Supplier Name")]
            public string SupplierName { get; set; } = string.Empty;

            [Display(Name = "Contact Person")]
            public string? ContactPerson { get; set; }

            [Display(Name = "Phone")]
            public string? Phone { get; set; }

            [Display(Name = "Email")]
            public string? Email { get; set; }

            [Display(Name = "GST Number")]
            public string? GSTNumber { get; set; }

            [Display(Name = "Address")]
            public string? Address { get; set; }

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth

            [Display(Name = "Cancelled Reason")]
            public string? CancelledReason { get; set; }
        }

        // Output Models from Stored Procedures
        public class Supplier
        {
            [Display(Name = "Supplier ID")]
            public long ID_Supplier { get; set; }

            [Display(Name = "Supplier Name")]
            public string SupplierName { get; set; } = string.Empty;

            [Display(Name = "Contact Person")]
            public string? ContactPerson { get; set; }

            [Display(Name = "Phone")]
            public string? Phone { get; set; }

            [Display(Name = "Email")]
            public string? Email { get; set; }

            [Display(Name = "GST Number")]
            public string? GSTNumber { get; set; }

            [Display(Name = "Address")]
            public string? Address { get; set; }

            [Display(Name = "Created On")]
            public DateTime? CreatedOn { get; set; }

            [Display(Name = "Cancelled")]
            public bool Cancelled { get; set; }

            [Display(Name = "Cancelled On")]
            public DateTime? CancelledOn { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string? CancelledReason { get; set; }
        }
    }
}
