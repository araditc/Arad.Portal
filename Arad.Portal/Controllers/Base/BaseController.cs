using System.Globalization;

using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;

namespace Arad.Portal.Controllers.Base;

public class BaseController : Controller
{
    protected readonly string DomainName;
    public readonly string CurrentUserName;
    protected readonly string DomainTitle;
    public readonly string CurrentUserId;

    public BaseController(IHttpContextAccessor accessor, IDomainRepository domainRepository, ILanguageRepository languageRepository)
    {
        if (accessor.HttpContext == null)
        {
            return;
        }

        DomainName = $"https://{accessor.HttpContext.Request.Host}";
        Language language = languageRepository.FirstOrDefault(c => c.Symbol == CultureInfo.CurrentCulture.Name);
        string title = "";
        Domain dbEntity = domainRepository.FirstOrDefault(d => d.DomainName == DomainName);

        if (dbEntity != null && !string.IsNullOrWhiteSpace(dbEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name))
        {
            title = dbEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = DomainName;
        }

        DomainTitle = title;

        //if (User.Identity is { IsAuthenticated: true })
        //{
        //    CurrentUserId = accessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    CurrentUserName = accessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        //}
        //else
        //{
            CurrentUserId = "";
            CurrentUserName = "";
        //}
    }
}