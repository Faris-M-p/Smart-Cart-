using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Module List / Select

public sealed class OutputModuleSelect
{
    public int IdModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsCore { get; set; }
}

public sealed class InputModuleList { }

public sealed class OutputModuleList
{
    public List<OutputModuleSelect> Modules { get; set; } = new();
    public List<int> EnabledModuleIds { get; set; } = new();
    public List<OutputModulePresetSelect> Presets { get; set; } = new();
}

#endregion

#region Module Preset List

public sealed class OutputModulePresetSelect
{
    public int IdPreset { get; set; }
    public string PresetCode { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<int> ModuleIds { get; set; } = new();
}

#endregion

#region Module Assign

public sealed class InModuleAssignView
{
    [Display(Name = "Enabled Modules")]
    public List<int> EnabledModuleIds { get; set; } = new();

    [Display(Name = "Preset")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Preset cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Preset cannot contain script tags.")]
    public string? SelectedPresetCode { get; set; }

    [Display(Name = "All Modules")]
    public List<OutputModuleSelect> AllModules { get; set; } = new();

    [Display(Name = "Presets")]
    public List<OutputModulePresetSelect> Presets { get; set; } = new();
}

public sealed class InputModuleAssign
{
    public int ActorUserId { get; set; }
    public List<int> EnabledModuleIds { get; set; } = new();
}

public sealed class OutputModuleAssign
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int EnabledCount { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Feature tree

public sealed class OutputModuleFeatureNode
{
    public int IdFeature { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public bool IsGroup { get; set; }
    public int DisplayOrder { get; set; }
    public List<OutputModuleFeatureNode> Children { get; set; } = new();
}

public sealed class OutputModuleFeatureModule
{
    public int IdModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public int DisplayOrder { get; set; }
    public List<OutputModuleFeatureNode> Features { get; set; } = new();
}

public sealed class OutputModuleFeaturePreset
{
    public string PresetCode { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<string> FeatureCodes { get; set; } = new();
}

public sealed class OutputModuleFeatureTree
{
    public List<OutputModuleFeatureModule> Modules { get; set; } = new();
    public List<OutputModuleFeaturePreset> Presets { get; set; } = new();
    public List<string> EnabledFeatureCodes { get; set; } = new();
    public int TotalLeaves { get; set; }
    public int EnabledLeaves { get; set; }
    public int DisabledLeaves { get; set; }
    public int CoreLeaves { get; set; }
}

public sealed class InModuleFeatureTreeView
{
    [Display(Name = "Modules")]
    public List<OutputModuleFeatureModule> Modules { get; set; } = new();

    [Display(Name = "Presets")]
    public List<OutputModuleFeaturePreset> Presets { get; set; } = new();

    [Display(Name = "Enabled features")]
    public List<string> EnabledFeatureCodes { get; set; } = new();
}

public sealed class InModuleFeatureSaveView
{
    [Display(Name = "Enabled features")]
    public List<string> EnabledFeatureCodes { get; set; } = new();
}

public sealed class InputModuleFeatureAssign
{
    public int ActorUserId { get; set; }
    public List<string> EnabledFeatureCodes { get; set; } = new();
}

public sealed class OutputModuleFeatureAssign
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int EnabledCount { get; set; }
    public List<string> EnabledFeatureCodes { get; set; } = new();
    public List<string> AddedFeatureCodes { get; set; } = new();
    public List<string> RemovedFeatureCodes { get; set; } = new();
    public int TotalLeaves { get; set; }
    public int EnabledLeaves { get; set; }
    public int DisabledLeaves { get; set; }
    public int CoreLeaves { get; set; }
    public bool Ok => ResponseCode == 0;
}

/// <summary>One catalog row used to build the tree and to decide what is enabled. Not a view model.</summary>
public sealed class OutputModuleFeatureRow
{
    public int IdFeature { get; set; }
    public int? ParentId { get; set; }
    public int IdModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleDescription { get; set; } = string.Empty;
    public bool ModuleIsCore { get; set; }
    public int ModuleOrder { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsGroup { get; set; }
}

public sealed class OutputFeatureGate
{
    public bool CatalogReady { get; set; }
    public List<string> EnabledModuleCodes { get; set; } = new();
    public List<string> EnabledFeatureCodes { get; set; } = new();
    public List<string> CompanyAdminCoreFeatureCodes { get; set; } = new();
}

#endregion
