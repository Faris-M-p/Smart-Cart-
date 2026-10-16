using System.Text.Json.Serialization;

namespace SmartCartPro.Models;

/// <summary>Input for common filter / dropdown fill.</summary>
public sealed class FilterOptions
{
    /// <summary>1 = products, 2 = categories, 3 = subcategories.</summary>
    public int Mode { get; set; }

    /// <summary>
    /// Optional criteria from the client. Mode 3: category id as string.
    /// Empty = all subcategories; set = filter by that category.
    /// </summary>
    public string? Criteria { get; set; }

    /// <summary>
    /// Parsed category id prepared by the service from <see cref="Criteria"/>.
    /// Repository uses this only — do not parse Criteria in the repository.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>Optional store scope for future fills.</summary>
    public int? Fk_Store { get; set; }

    /// <summary>Optional search text for future fills.</summary>
    public string? Search { get; set; }
}

/// <summary>
/// Typed filter fill response. Only the lists for the requested mode are populated.
/// </summary>
public sealed class FilterDataResponse
{
    public IReadOnlyList<FillProduct>? Products { get; set; }

    public IReadOnlyList<FillCategory>? Categories { get; set; }

    public IReadOnlyList<FillSubCategory>? SubCategories { get; set; }
}

public class FillProduct
{
    [JsonPropertyName("ID_value")]
    public int Id_Value { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}

public class FillCategory
{
    [JsonPropertyName("ID_value")]
    public int Id_Value { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}

public class FillSubCategory
{
    [JsonPropertyName("ID_value")]
    public int Id_Value { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}
