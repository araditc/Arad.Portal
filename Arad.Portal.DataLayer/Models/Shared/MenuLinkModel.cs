using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared;

public class MenuLinkModel
{
    public MenuLinkModel()
    {
        Children = new List<MenuLinkModel>();
    }
    public string MenuId { get; set; }
    public string MenuTitle { get; set; }
    public string Link { get; set; }
    public string Icon { get; set; }
    public bool IsActive { get; set; }
    public double Priority { get; set; }
    public List<MenuLinkModel> Children { get; set; }
}