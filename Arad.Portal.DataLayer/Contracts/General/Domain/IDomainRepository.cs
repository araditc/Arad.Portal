using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Domain;

namespace Arad.Portal.DataLayer.Contracts.General.Domain
{
    public interface IDomainRepository
    {
        Task<RepositoryOperationResult> AddDomain(DomainDTO dto);
        Task<RepositoryOperationResult> EditDomain(DomainDTO dto);
        Task<RepositoryOperationResult> DomainChangePrice(DomainPrice dto);
        Task<PagedItems<DomainViewModel>> AllDomainList(string queryString);
        Task<RepositoryOperationResult> DeleteDomain(string domainId, string modificationReason);
        RepositoryOperationResult<DomainDTO> FetchDomain(string domainId);
        RepositoryOperationResult<DomainDTO> FetchByName(string domainName);
        RepositoryOperationResult<DomainDTO> GetDefaultDomain();

       Task<RepositoryOperationResult> Restore(string id);
        List<SelectListModel> GetAllActiveDomains();
    }
}
