using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class VariantValueModel
    {
        public class VariantValueListInputVIEW
        {
            [Display(Name = "Variant")]
            [Range(0, int.MaxValue, ErrorMessage = "{0} must be zero or greater.")]
            public int FK_Variant { get; set; }

            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Variant IDs")]
            public string FilterVariantIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 50000, ErrorMessage = "{0} must be between 1 and 50000.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 2, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class VariantValueUpdateInputVIEW
        {
            [Display(Name = "Variant Value ID")]
            public int VariantValueID { get; set; }

            [Display(Name = "Variant")]
            [GreaterThanZero(ErrorMessage = "{0} is required.")]
            public int FK_Variant { get; set; }

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
        }

        public class VariantValueDeleteInputVIEW
        {
            [Display(Name = "Variant Value ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int VariantValueID { get; set; }
        }

        public class VariantValueListInput
        {
            /// <summary>When &gt; 0, list is restricted to this variant.</summary>
            public int FK_Variant { get; set; }

            public string SearchText { get; set; } = string.Empty;
            public string FilterVariantIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = string.Empty;
        }

        public class VariantValueCreateInput
        {
            public int FK_Variant { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        public class VariantValueUpdateInput
        {
            public int VariantValueID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        public class VariantValueDeleteInput
        {
            public int VariantValueID { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        public class VariantValue
        {
            public int VariantValueID { get; set; }
            public int FK_Variant { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int DisplayOrder { get; set; }
            public string? VariantName { get; set; }
        }
    }
}
