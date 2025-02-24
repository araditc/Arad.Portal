using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.Shop.Announcement;

//not have been implemented yet
public class Announcement : BaseEntity
{
    public string ProductId { get; set; }

    public bool IsNew { get; set; }
}