using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared;

public class StoreMenuVm
{
    public StoreMenuVm()
    {
        Childrens = new();
        IsFull = true;
        MenuTitle = new();
    }
    public string MenuId { get; init; }
    /// <summary>
    /// LanguageId and Title will be filled here
    /// </summary>
    public MultiLingualProperty MenuTitle { get; init; }
    public MenuType MenuType { get; init; }
    public int Order { get; init; }
    public string ParentId { get; set; }
    public string Icon { get; set; }
    public string Url { get; set; }
    public string SubId { get; set; }
    public string SubName { get; set; }
    public string SubGroupId { get; init; }
    public string SubGroupName { get; set; }
    public long MenuCode { get; init; }
    public bool IsFull { get; set; }
    public List<StoreMenuVm> Childrens { get; init; }
}