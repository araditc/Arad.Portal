using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.ViewComponents;

public class CommentSectionViewComponent(
    IProductRepository proRepository,
    IHttpContextAccessor accessor,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository)
    : ViewComponent
{

    public IViewComponentResult Invoke(CommentVm comment)
    {
        return View(comment);
    }
}