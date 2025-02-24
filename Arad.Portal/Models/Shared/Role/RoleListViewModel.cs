using System;

namespace Arad.Portal.Models.Shared.Role;

public class RoleListViewModel
{
    public string Id { get; init; }
    public string RoleName { get; set; }
    public string CreatorId { get; set; }
    public string CreatorUserName { get; init; }
    public DateTime CreationDateTime { get; init; }
    public bool HasModifications { get; set; }
    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }
}