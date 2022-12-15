using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.DesignStructure;

namespace Arad.Portal.DataLayer.Contracts.General.Domain
{
    public interface IDomainRepository
    {
        Task<Result> AddDomain(DomainDTO dto);
        Task<Result> EditDomain(DomainDTO dto);
        Task<Result> DomainUpdate(Entities.General.Domain.Domain dom);
        Task<Result> UpdateDynamicPagesOfDomain(DomainPageModel model);

        Task<Result> AddSamplePagetoDomainEntity(Entities.General.Domain.Domain dom, PageDesignContent model);
        Task<Result> DomainChangePrice(DomainPrice dto);
        Task<PagedItems<DomainViewModel>> AllDomainList(string queryString, ApplicationUser user);
        Task<Result> DeleteDomain(string domainId, string modificationReason);
        Result<DomainDTO> FetchDomain(string domainId);
        Result<DomainDTO> FetchByName(string domainName, bool isDef);
        Result<DomainDTO> FetchDefaultDomain();
        void InsertOne(Entities.General.Domain.Domain entity);
        string FetchDomainTitle(string domainName);
        string GetDomainName();
        bool HasDefaultDomain();
        Entities.General.Domain.Domain FetchDomainByName(string domainName);
        Result<DomainDTO> GetDefaultDomain();

        
        SMTP GetSMTPAccount(string domainName);
        Task<Result> Restore(string id);
        List<SelectListModel> GetAllActiveDomains();

        List<SelectListModel> GetAllEmailEncryptionType();
        List<SelectListModel> GetInvoiceNumberProcedureEnum();
        List<SelectListModel> GetPspTypesEnum();
        List<SelectListModel> GetOneColsTemplateWidthEnum();
        List<SelectListModel> GetTwoColsTemplateWidthEnum();
        List<SelectListModel> GetThreeColsTemplateWidthEnum();
        List<SelectListModel> GetFourColsTemplateWidthEnum();
        List<SelectListModel> GetFiveColsTemplateWidthEnum();
        List<SelectListModel> GetSixColsTemplateWidthEnum();

    }
}
