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
