namespace Arad.Portal.Helpers.Shared;

public interface IRouteLocator
{
    string GetProductGroupId(string groupCode);
    string GetProductId(string productCode);
    string GetContentCategoryId(string categoryCode);
    string GetContentId(string contentCode);
    //string GetPageId(string urlFriend);
}