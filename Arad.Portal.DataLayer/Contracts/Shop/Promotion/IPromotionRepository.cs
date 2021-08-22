using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Promotion
{
    public interface IPromotionRepository
    {

        Task<RepositoryOperationResult> InsertPromotion(PromotionDTO dto);

        Task<RepositoryOperationResult> UpdatePromotion(PromotionDTO dto);

        Task<RepositoryOperationResult> AssingPromotionToProductGroup(string promotionId, string ProductGroupId);

        Task<RepositoryOperationResult> AssignPromotionToProduct(string promotionId, string productId);

        Task<RepositoryOperationResult> SetPromotionExpirationDate(string promotionId, DateTime? dateTime);

        Task<RepositoryOperationResult> DeletePromotion(string promotionId, string modificationReason);

        Task<PagedItems<PromotionDTO>> ListPromotions(string queryString);

        List<SelectListModel> GetActivePromotionsOfCurrentUser(string userId, PromotionType type);
    }
}
