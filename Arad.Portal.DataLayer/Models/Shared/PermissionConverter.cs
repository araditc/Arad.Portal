using Arad.Portal.DataLayer.Entities.Abstractions;

using System.Collections.Generic;

using Arad.Portal.DataLayer.Entities.General.Modification;

namespace Arad.Portal.DataLayer.Models.Shared;

public class PermissionConverter
{
    public string Id { get; set; }
    public PermissionConverter()
    {
        Children = new();
        Actions = new();
    }
    public string Title { get; set; }

    public short LevelNo { get; set; }

    public bool IsUI { get; set; }

    public string ParentId { get; set; }

    public short? Priority { get; set; }

    public string Icon { get; set; }

    public List<string> Urls { get; set; }

    public string ClientAddress { get; set; }

    public List<PermissionConverter> Children { get; set; }

    public List<Action> Actions { get; set; }
}

public class Action
{
    public string PermissionId { get; set; }

    public string Title { get; set; }

    public string ClientAddress { get; set; }

    public List<string> Urls { get; set; }
}