using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

namespace Arad.Portal.Models.Shared.DesignStructure;

public class ModuleWithParametersValue
{
    public ModuleWithParametersValue()
    {
        ParametersValue = new();
    }
    public string ModuleId { get; init; }

    public string ModuleName { get; init; }

    public ModuleParameters ParametersValue { get; set; }
}