using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.Models.UI;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Controllers.Product;

public class ProductGroupController : BaseController
{
    private readonly IProductGroupRepository _groupRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly ControllerHelper _controllerHelper;
    private readonly IProductRepository _productRepository;
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IHttpContextAccessor _accessor;
    private readonly string _domainName;
    public ProductGroupController(IProductGroupRepository groupRepository,
                                  IHttpContextAccessor accessor,
                                  ILanguageRepository lanRepository,
                                  IMenuRepository menuRepository, IDomainRepository domainRepository,
                                  ControllerHelper controllerHelper,
                                  IProductRepository productRepository,
                                  IProductGroupRepository productGroupRepository) : base(accessor, domainRepository, lanRepository)
    {
        _groupRepository = groupRepository;
        _accessor = accessor;
        _languageRepository = lanRepository;
        _menuRepository = menuRepository;
        _controllerHelper = controllerHelper;
        _productRepository = productRepository;
        _productGroupRepository = productGroupRepository;
        _domainName = DomainName;
    }

    [Route("{language}/group")]
    public IActionResult Index()
    {
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = GeneralLibrary.Utilities.UtilityLanguage.GetString("design_ProductGroups");
        return View();
    }
    //[Route("group/{**slug}")]
    [Route("{language}/group/{**slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        CommonViewModel model = new();
        string path = _accessor.HttpContext.Request.Path.ToString();
        string lanIcon = path.Split("/")[1].ToLower();
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = slug.Replace("-", " ");
        string langId = _controllerHelper.GetDefaultLanguage().Id;
        ViewData["CurLangId"] = langId;
        string result = "";
        long codeNumber;

        ProductGroup pgentity = new DataLayer.Entities.Shop.ProductGroup.ProductGroup();
        if (long.TryParse(slug, out codeNumber))
        {
            pgentity = await _groupRepository.FirstOrDefaultAsync(g => g.GroupCode == codeNumber && !g.IsDeleted, cancellationToken);
        }
        else
        {
            pgentity = await _groupRepository.FirstOrDefaultAsync(g => g.MultiLingualProperties.Any(a => a.UrlFriend == $"/group/{slug}") && !g.IsDeleted, cancellationToken);
        }

        if (pgentity != null)
        {
            result = pgentity.Id;
        }
        string id = result;
        if (!string.IsNullOrEmpty(id))
        {
            GroupSection grpSection = new()
                                      {
                                          ProductGroupId = id,
                                          CountToSkip = 0,
                                          CountToTake = 4,
                                          DefaultLanguageId = langId
                                      };

            List<string> lst;
            Result<Domain> currentDomain = _controllerHelper.FetchDomainByName(DomainName, false);
            Domain defDomain = _controllerHelper.GetCurrentUserDomain();
            if (currentDomain.ReturnValue.Id == defDomain.Id)
            {
                lst = (await _productRepository.GetAllAsync(cancellationToken))
                                        .Where(p => !p.IsDeleted && (p.AssociatedDomainId == currentDomain.ReturnValue.Id || p.IsPublishedOnMainDomain))
                                        .SelectMany(p => p.GroupIds)
                                        .Distinct()
                                        .ToList();
            }
            else
            {
                lst = (await _productRepository.GetAllAsync(cancellationToken))
                                        .Where(p => !p.IsDeleted && p.AssociatedDomainId == currentDomain.ReturnValue.Id)
                                        .SelectMany(p => p.GroupIds)
                                        .Distinct()
                                        .ToList();
            }

            List<string> finalList = [];
            foreach (string groupId in lst)
            {
                finalList.Add(groupId);
                ProductGroup entity = await _productGroupRepository.FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
            }

            grpSection.GroupsWithProducts = finalList;
            model.GroupSection = grpSection;

            ProductsInGroupSection proSection = new()
                                                {
                                                    ProductGroupId = id,
                                                    CountToSkip = 0,
                                                    CountToTake = 4,
                                                    DefaultLanguageId = langId
                                                };
            model.ProductInGroupSection = proSection;

            return View(model);
        }
        else
        {
            return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
        }
    }

    [HttpPost]
    public IActionResult GetMyGroupVc(GroupSection groupSection)
    {
        return ViewComponent("GroupSection", groupSection);
    }

    public IActionResult GetProductsInGroupVc(ProductsInGroupSection productsSection)
    {
        return ViewComponent("ProductsInGroupSection", productsSection);
    }

}