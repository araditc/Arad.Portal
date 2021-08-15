using Arad.Portal.DataLayer.Models.Comment;
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

        Task<RepositoryOperationResult> Add(ProductInputDTO dto);

        Task<ProductOutputDTO> ProductFetch(string productId);

        Task<RepositoryOperationResult> AddCommentToProduct(string productId, Comment comment);

        Task<RepositoryOperationResult> ChangeUnitOfProduct(string productId,
            string unitId, string modificationReason);

        Task<RepositoryOperationResult> AddMultilingualProperty(string productId,
            MultiLingualProperty multiLingualProperty);


        Task<RepositoryOperationResult> AddPictureToProduct(string productId,
            Picture picture);

        Task<RepositoryOperationResult> Restore(string productId);
        Task<RepositoryOperationResult> AddProductSpecifications(string productId,
            SpecificationValueDTO specValueDto);
        Task<RepositoryOperationResult> UpdateProduct(ProductInputDTO dto);

        Task<RepositoryOperationResult> DeleteProduct(string productId, string modificationReason);

        List<Picture> GetPictures(string productId);

        List<string> GetProductGroups(string productId);

        //Task<bool> SetProductPic(string path, string productId);
        List<ProductSpecificationValue> GetProductSpecifications(string productId);
        //ProductBasket GetProductBasket(string id);
        //ProductFav GetProductFav(string id);
        //Task<bool> UpdateViewersCount(string id);
        //Task<bool> UpdateSaleCount(string id);
        Task<RepositoryOperationResult> ChangeActivation(string productId, string modificationReason);

        //Pagination<ProductViewGallery> ListProductsGroup(GalleryPagination modelPagination);
        int GetInventory(string productId);
        
        //Task<ProductOutputDTO> GetByUrlFriend(string urlFriend);
        
    }
}
