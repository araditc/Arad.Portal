using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;

using Serilog;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Models.UI;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Helpers.UI;

public class LayoutContentProcess
{
    private readonly IDomainRepository _domainRepository;
    private readonly IHttpContextAccessor _accessor;
    private readonly ControllerHelper _controllerHelper;
    private readonly IWebHostEnvironment _environment;
    private readonly ILanguageRepository _lanRepository;
    public LayoutModel LayoutModel = new();

    public LayoutContentProcess(IDomainRepository domainRepository,
                                IWebHostEnvironment env,
                                ILanguageRepository languageRepository,
                                IHttpContextAccessor accessor,
                                ControllerHelper controllerHelper)
    {
        _domainRepository = domainRepository;
        _accessor = accessor;
        _controllerHelper = controllerHelper;
        _environment = env;
        _lanRepository = languageRepository;
        CalculateLayoutContent();
    }

    private void CalculateLayoutContent()
    {

        string domainName = $"{_accessor.HttpContext.Request.Host}";
        Domain res = _domainRepository.FirstOrDefault(c => c.DomainName == "https://" + domainName);
        string languageId = res.DefaultLanguageId;
        if (res != null)
        {
            Domain domainEntity = res;
            LayoutModel.IsShop = domainEntity.IsShop;
            LayoutModel.IsMultiLingual = domainEntity.IsMultiLinguals;

            if (domainEntity.HomePageDesign.Count > 0 && domainEntity.HomePageDesign.Any(c => c.LanguageId == languageId))
            {
                if (!string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.HeaderPart.PriorFixedContent)
                    || !string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.HeaderPart.LatterFixedContent)
                    || domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)!.HeaderPart.CustomizedContent.Count > 0)
                {
                    LayoutModel.HasCustomizedHeader = true;
                    LayoutModel.HeaderPart = domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.HeaderPart;
                }

                if (!string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.FooterPart.PriorFixedContent)
                    || !string.IsNullOrEmpty(domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.FooterPart.LatterFixedContent)
                    || domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)!.FooterPart.CustomizedContent.Count > 0)
                {
                    LayoutModel.HasCustomizedFooter = true;
                    LayoutModel.FooterPart = domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == languageId)?.FooterPart;
                }
            }
        }
        else
        {
            LayoutModel.HeaderPart = new();
            LayoutModel.FooterPart = new();
        }
    }
}