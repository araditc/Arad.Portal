using System;

namespace Arad.Portal.Models.Shared.Permission;

public class ListPermissionViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    //public Enums.PermissionType Type { get; set; }
    //public Enums.PermissionMethod Method { get; set; }
    public string ClientAddress { get; set; }
    public string Routes { get; set; }
    public DateTime CreationDate { get; set; }
    public string CreatorName { get; set; }
    public bool HasModification { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}