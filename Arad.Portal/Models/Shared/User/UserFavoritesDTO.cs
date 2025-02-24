namespace Arad.Portal.Models.Shared.User;

public class UserFavoritesDto
{
    public string Id { get; init; }
    public string Url { get; init; }
    public string ImagePath { get; set; }
    public bool NoImage { get; set; }
    public string Name { get; set; }
}