using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Permission;

public class ListPermissions
{
    public string Title { get; init; }
    public string Id { get; init; }
    public bool IsSelected { get; init; }
    public bool IsActive { get; init; }
    public List<ListPermissions> Childrens { get; init; }
    // public PermissionType Type { get; set; }
    public double Priority { get; init; }
}