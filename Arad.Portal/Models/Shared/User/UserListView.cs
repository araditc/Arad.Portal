using System;

namespace Arad.Portal.Models.Shared.User;

public class UserListView
{
    public string Id { get; init; }
    public string UserName { get; init; }
    public string Name { get; init; }
    public string LastName { get; init; }
    public string PhoneNumber { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreationDate { get; init; }
    //public string persianCreateDate { get; set; }
    public bool IsVerify { get; init; }
    public bool IsDeleted { get; init; }
    public string UserRoleId { get; init; }
    public string? RoleName { get; set; }
}