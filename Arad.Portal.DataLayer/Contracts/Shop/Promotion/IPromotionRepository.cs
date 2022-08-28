using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Promotion
{
    public interface IPromotionRepository
    {

        Task<Result> InsertPromotion(PromotionDTO dto);

        Task<Result> UpdatePromotion(PromotionDTO dto);

        Task<Result> AssingPromotionToProductGroup(string promotionId, string ProductGroupId);

        Task<Result> AssignPromotionToProduct(string promotionId, string productId);

        Task<Result> SetPromotionExpirationDate(string promotionId, DateTime? dateTime);

        Task<Result> DeletePromotion(string promotionId, string modificationReason);

        Task<Result> DeleteUserCoupon(string userCouponId);

        Task<PagedItems<PromotionDTO>> ListPromotions(string queryString);

        PromotionDTO FetchPromotion(string id);

        List<SelectListModel> GetActivePromotionsOfCurrentUser(string userId, PromotionType type);

        List<SelectListModel> GetAvailableCouponsOfDomain(string domainName);

        List<SelectListModel> GetAllPromotionType();

        List<SelectListModel> GetAllDiscountType(bool asCoupon);

        Task<Result> Restore(string promotionId);

        Result CheckCouponCodeUniqueness(string couponCode, string domainId);

        UserCouponDTO FetchUserCoupon(string userCouponId);

        Task<Result> InsertUserCoupon(UserCouponDTO model);

        Task<Result> UpdateUserCoupon(UserCouponDTO model);

        Task<PagedItems<UserCouponDTO>> ListUserCoupon(string queryString);

        Result<NewVal> CheckCode(string userId, string code, string domainId, long price);
    }
}
