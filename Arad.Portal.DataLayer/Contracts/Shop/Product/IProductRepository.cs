
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Contracts.Shop.Product
{
    public interface IProductRepository
    {
        Task<PagedItems<ProductViewModel>> List(string queryString);

        Task<Result> Add(ProductInputDTO dto);

        Task<Result> InsertDownloadLimitation(string shoppingCartDetailId, string productId, string userId, string domainId);
        long GetProductCode(string productId);
        Task<ProductOutputDTO> ProductFetch(string productId);


        ProductOutputDTO EvaluateFinalPrice(string productId, List<Price> productPrices, List<string> productGroupIds, string defCurrenyId);
        ProductOutputDTO FetchProductWithSlug(string slug, string domainName);

        Task<Result> AddCommentToProduct(string productId, Comment comment);
        Task<Result> UpdateVisitCount(string productId);
        Task<Result> ChangeUnitOfProduct(string productId,
            string unitId, string modificationReason);

        Task<Result> AddMultilingualProperty(string productId,
            MultiLingualProperty multiLingualProperty);

        List<SelectListModel> GetAllImageRatio();
        Task<Result> AddPictureToProduct(string productId,
            Image picture);
        Task<Result> ImportFromExcel(List<ProductExcelImport> lst);
        Task<Result> Restore(string productId);
        Task<Result> AddProductSpecifications(string productId,
            ProductSpecificationValue specValues);
        Task<Result> UpdateProduct(ProductInputDTO dto);

        Task<Result> DeleteProduct(string productId, string modificationReason);

        Task<Result<List<ProductCompare>>> SearchProducts(string filter, string lanId, string currencyId, string domainId);

        Task<Result<List<GeneralSearchResult>>> GeneralSearch(string filter, string lanId, string CurrencyId, string domainId);

        Task<Result<List<ProductCompare>>> FindProductsInGroups(List<string> groupIds, string lanId, string currencyId, string domainId);

        bool IsCodeUnique(string code, string productId ="");

        Task<Result> UpdateDownloadLimitationCount(string userId, string productId);
        bool IsDownloadIconShowForCurrentUser(string userId, string productId);
        bool IsUniqueUrlFriend(string urlFriend, string productId = "");
        Task<Result<EntityRate>> RateProduct(string productId, int score, bool isNew, int prevScore);
        List<Image> GetPictures(string productId);

        List<string> GetProductGroups(string productId);

        //Task<bool> SetProductPic(string path, string productId);
        List<ProductSpecificationValue> GetProductSpecifications(string productId);
        //ProductBasket GetProductBasket(string id);
        //ProductFav GetProductFav(string id);
        //Task<bool> UpdateViewersCount(string id);
        //Task<bool> UpdateSaleCount(string id);
        Task<Result> ChangeActivation(string productId, string modificationReason);

        //Pagination<ProductViewGallery> ListProductsGroup(GalleryPagination modelPagination);
        int GetInventory(string productId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="isIncreament">if equals to false then it is decreament</param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<Result> UpdateProductInventory(string productId, bool isIncreament, int count);
        //ProductOutputDTO FetchBySlug(string slug, string domainName);

        ProductOutputDTO FetchByCode(string slugOrCode, DomainDTO dto, string userId);

        string FetchIdByCode(long productCode);

        string FetchUrlFriendById(string productId);

        List<ProductOutputDTO> GetSpecialProducts(int count, string currencyId, ProductOrContentType type);
        bool HasActiveProductPromotion(string productId);
        List<SelectListModel> GetAllActiveProductList(string langId, string currentUserId, string productGroupId, string vendorId);
        List<SelectListModel> GetAllProductList(ApplicationUser user,string productGroupId, string langId);
        List<SelectListModel> GetAlActiveProductGroup(string langId);

        List<SelectListModel> GetGroupsOfThisVendor(string vendorId, string domainId);
        List<SelectListModel> GetProductsOfThisVendor(string langId, string currentUserId);

        string GetProductSpecificationName(string specificationId, string languageId);
    }
}
