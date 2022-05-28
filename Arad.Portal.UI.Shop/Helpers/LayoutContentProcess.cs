using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.General.Language;
using System.Linq;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class LayoutContentProcess
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly IWebHostEnvironment _environment;
        private readonly ILanguageRepository _lanRepository;
        public LayoutModel LayoutModel = new LayoutModel();

        public LayoutContentProcess(IDomainRepository domainRepository,
            IWebHostEnvironment env,
            ILanguageRepository languageRepository,
            IHttpContextAccessor accessor)
        {
            _domainRepository = domainRepository;
            _accessor = accessor;
            _environment = env;
            _lanRepository = languageRepository;
            CalculateLayoutContent();
        }

        public void CalculateLayoutContent()
        {

            string domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];

            var languageId = _lanRepository.FetchBySymbol(lanIcon);
            var res = _domainRepository.FetchByName(domainName, false);
            if (res.Succeeded)
            {
                var domainEntity = res.ReturnValue;
                LayoutModel.IsShop = domainEntity.IsShop;
                LayoutModel.IsMultiLingual = domainEntity.IsMultiLinguals;

                if(domainEntity.HomePageDesign.Count > 0 && domainEntity.HomePageDesign.Any(_=>_.LanguageId == languageId))
                {
                    if (!string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).HeaderPart.PriorFixedContent) 
                        || !string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).HeaderPart.LatterFixedContent)
                                      || domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).HeaderPart.CustomizedContent.Count > 0)
                    {
                        LayoutModel.HasCustomizedHeader = true;
                        LayoutModel.HeaderPart = domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).HeaderPart;
                    }

                    if (!string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).FooterPart.PriorFixedContent) 
                        || !string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).FooterPart.LatterFixedContent)
                        || domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).FooterPart.CustomizedContent.Count > 0)
                    {
                        LayoutModel.HasCustomizedFooter = true;
                        LayoutModel.FooterPart = domainEntity.HomePageDesign.FirstOrDefault(_ => _.LanguageId == languageId).FooterPart;
                    }
                }
            }
            else
            {
                LayoutModel.HeaderPart = new PageHeaderPart();
                LayoutModel.FooterPart = new PageFooterPart();
            }
        }
    }
}
