using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Menu;

public class MenuDto
{
    public MenuDto()
    {
        MenuTitles = new();
    }
    public string Id { get; init; }

    /// <summary>
    /// LanguageId and Name will be filled here
    /// </summary>
    public List<MultiLingualProperty> MenuTitles { get; init; }

    public string? MenuTitle { get; init; }

    public string? LanguageId { get; init; }

    public MenuType MenuType { get; set; }

    public string MenuTypeId { get; init; }

    public int? Order { get; init; }

    public string? ParentId { get; init; }

    public string? ParentName { get; init; }

    public string? Icon { get; init; }

    public string? Url { get; init; }

    public long? MenuCode { get; init; }

    public string? CreatorUserName { get; init; }

    public string? CreatorUserId { get; init; }

    public string? SubId { get; init; }

    public string? SubName { get; init; }

    public string? SubGroupId { get; init; }

    public string AssociatedDomainId { get; set; }

    public string? SubGroupName { get; init; }

    public bool IsDeleted { get; init; }
}