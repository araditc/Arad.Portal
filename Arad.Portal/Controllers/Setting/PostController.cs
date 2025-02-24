using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Controllers.Base;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;

namespace Arad.Portal.Controllers.Setting;

[Authorize(Policy = "Role")]
public class PostController : BaseController
{
    private readonly HttpClientHelper _httpClientHelper;
    private readonly IHttpContextAccessor _accessor;
    public PostController(HttpClientHelper httpClientHelper,
                          IHttpContextAccessor accessor,
                          IDomainRepository domRepository,
                          ILanguageRepository languageRepository,
                          ControllerHelper controllerHelper)
        : base(accessor, domRepository, languageRepository)
    {
        _httpClientHelper = httpClientHelper;
    }
    public IActionResult Index()
    {
        return View();
    }
}