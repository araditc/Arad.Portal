
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Product
{
    public interface IProductRepository
    {
        Task<PagedItems<ProductViewModel>> List(string queryString);

        Task<Result> Add(ProductInputDTO dto);

        Task<ProductOutputDTO> ProductFetch(string productId);

        ProductOutputDTO FetchProductWithSlug(string slug, string domainName);

        Task<Result> AddCommentToProduct(string productId, Comment comment);

        Task<Result> ChangeUnitOfProduct(string productId,
            string unitId, string modificationReason);

        Task<Result> AddMultilingualProperty(string productId,
            MultiLingualProperty multiLingualProperty);


        Task<Result> AddPictureToProduct(string productId,
            Image picture);

        Task<Result> Restore(string productId);
        Task<Result> AddProductSpecifications(string productId,
            ProductSpecificationValue specValues);
        Task<Result> UpdateProduct(ProductInputDTO dto);

        Task<Result> DeleteProduct(string productId, string modificationReason);


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
        ProductOutputDTO FetchBySlug(string slug, string domainName);

        ProductOutputDTO FetchByCode(long productCode, DomainDTO dto, string userId);
        bool HasActiveProductPromotion(string productId);
        List<SelectListModel> GetAllActiveProductList(string langId, string currentUserId, string productGroupId, string vendorId);
        List<SelectListModel> GetAllProductList(ApplicationUser user,string productGroupId, string langId);
        List<SelectListModel> GetAlActiveProductGroup(string langId);

        List<SelectListModel> GetGroupsOfThisVendor(string vendorId, string domainId);
        List<SelectListModel> GetProductsOfThisVendor(string langId, string currentUserId);
    }
}
