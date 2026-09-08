using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class BrandModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class BrandListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Brand IDs")]
            public string FilterBrandIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 2, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class BrandUpdateInputVIEW
        {
            [Display(Name = "Brand ID")]
            public int BrandID { get; set; } = 0;

            [Display(Name = "Brand Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string BrandName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [System.ComponentModel.DataAnnotations.MaxLength(1000, ErrorMessage = "{0} cannot exceed 1000 characters.")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Active")]
            public bool? IsActive { get; set; }

            public IFormFile? BrandImage { get; set; }

            public bool RemoveImage { get; set; }
        }

        public class BrandDeleteInputVIEW
        {
            [Display(Name = "Brand ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int BrandID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class BrandListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Brand IDs")]
            public string FilterBrandIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class BrandUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; }

            [Display(Name = "Brand ID")]
            public int BrandID { get; set; } = 0;

            [Display(Name = "Brand Name")]
            public string BrandName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Is Active")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1;
        }

        public class BrandDeleteInput
        {
            [Display(Name = "Brand ID")]
            public int BrandID { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string CancelledReason { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1;
        }

        public class Brand
        {
            public int BrandID { get; set; }

            public string BrandName { get; set; } = string.Empty;

            public string? Description { get; set; }

            public bool IsActive { get; set; }

            public string? ImageUrl { get; set; }

            public bool Cancelled { get; set; }

            public DateTime? CancelledOn { get; set; }

            public string? CancelledReason { get; set; }
        }
    }
}
