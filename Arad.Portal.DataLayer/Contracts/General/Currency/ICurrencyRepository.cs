using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Currency;

namespace Arad.Portal.DataLayer.Contracts.General.Currency
{
    public interface ICurrencyRepository
    {
        /// <summary>
        /// add Or Update currency Entity
        /// </summary>
        /// <param name="dto"> dto referes to : Data Transfer Object</param>
        /// <returns></returns>
        Task<Result> SaveCurrency(CurrencyDTO dto);
        Task<PagedItems<CurrencyDTO>> AllCurrencyList(string queryString);
        Task<Result> DeleteCurrency(string currencyId);
        Result<CurrencyDTO> FetchCurrency(string currencyId);
        Result<CurrencyDTO> GetDefaultCurrency(string userId);
        CurrencyDTO GetCurrencyByItsPrefix(string prefix);
        List<SelectListModel> GetAllActiveCurrency();

    }
}
