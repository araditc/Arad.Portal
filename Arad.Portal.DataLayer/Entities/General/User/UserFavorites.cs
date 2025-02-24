using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.User;

public class UserFavorites : BaseEntity
{
    public FavoriteType FavoriteType { get; set; }

    public string EntityId { get; set; }

    public string Url { get; set; }
}

public enum FavoriteType
{
    Product = 1,
    Content = 2
}