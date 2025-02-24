using System;

namespace Arad.Portal.Models.Shared.User;

public class UserSearchParams
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string UserRoleId { get; set; }
    public bool? IsActive { get; set; }
    public bool IsVerify { get; set; }
    public DateTime? StartRegisterDate { get; set; }
    public DateTime? EndRegisterDate { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
}