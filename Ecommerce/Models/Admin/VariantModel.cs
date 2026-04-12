using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class VariantModel
    {
        public class VariantListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Variant IDs")]
            public string FilterVariantIDs { get; set; } = string.Empty;

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
            public string SortMode { get; set; } = string.Empty;
        }

        public class VariantUpdateInputVIEW
        {
            [Display(Name = "Variant ID")]
            public int VariantID { get; set; }

            [Display(Name = "Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Display Order")]
            [Range(0, 999999, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int DisplayOrder { get; set; }

            [Display(Name = "Active")]
            public bool? IsActive { get; set; }
        }

        public class VariantDeleteInputVIEW
        {
            [Display(Name = "Variant ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int VariantID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class VariantListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string FilterVariantIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = string.Empty;
        }

        public class VariantUpdateInput
        {
            public int VariantID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public bool IsActive { get; set; } = true;
            public int EnterBy { get; set; } = 1;
        }

        public class VariantDeleteInput
        {
            public int VariantID { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
            public int EnterBy { get; set; } = 1;
        }

        public class Variant
        {
            public int VariantID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int DisplayOrder { get; set; }
            public bool IsActive { get; set; }
            public bool Cancelled { get; set; }
        }
    }
}
