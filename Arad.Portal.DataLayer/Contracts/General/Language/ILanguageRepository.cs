using Arad.Portal.DataLayer.Models.Language;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Language
{
    public interface ILanguageRepository
    {
        Task<PagedItems<LanguageDTO>> List(string queryString);
        Task<Result> AddNewLanguage(LanguageDTO dto);
        Task<Result> EditLanguage(LanguageDTO dto);
        Task<Result> Delete(string languageId, string modificationReason);

        LanguageDTO GetDefaultLanguage(string currentUserId);
        List<SelectListModel> GetAllActiveLanguage();
        LanguageDTO FetchLanguage(string LanguageId);
        string FetchBySymbol(string symbol);


    }
}
