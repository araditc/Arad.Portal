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
        Task<RepositoryOperationResult> AddNewLanguage(LanguageDTO dto);
        Task<RepositoryOperationResult> EditLanguage(LanguageDTO dto);
        Task<RepositoryOperationResult> Delete(string languageId, string modificationReason);
    }
}
