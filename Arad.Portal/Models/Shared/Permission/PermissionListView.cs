using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Permission;

public class PermissionListView
{
    public PermissionListView()
    {
        Pers = new();
    }
    public string Title { get; set; }
    public string Id { get; set; }
    public bool IsSelected { get; set; }
    public bool IsActive { get; set; }
    public List<PermissionListView> Pers { get; set; }
}