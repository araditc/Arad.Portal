using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using System.Collections.Generic;

using Arad.Portal.DataLayer.Entities.General.Domain;

using Flurl;

namespace Arad.Portal.Middlewares;

public class UseLanguageMapperMiddleware(
    RequestDelegate next,
    ILanguageRepository languageRepository,
    IDomainRepository domainRepository)
{
    public async Task Invoke(HttpContext context)
    {
        string domainName = $"{context.Request.Host}";
        List<string> langSymbolList = (await languageRepository.GetListAsync(l => l.IsActive)).Select(l => l.Symbol.ToLower()).ToList();
        //var baseAddressAdmin = _configuration["BaseAddress"] ?? "/Admin";
        if (langSymbolList.Any(item => context.ToString()!.Contains(item)))
        {
            await next.Invoke(context);
            return;
        }

        //if (ShouldSkipProcessing(context.Request.Path.ToString(), baseAddressAdmin))
        //{
        //    await _next.Invoke(context);
        //    return;
        //}
        string? defLangSymbol = GetDefaultLanguageSymbol(context, domainName, langSymbolList);

        if (context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName] == null)
        {
            if (defLangSymbol != null)
            {
                context.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new(defLangSymbol)),
                    new() { Domain = domainName, Expires = DateTime.Now.AddDays(7) });
            }
        }

        string newPath = BuildNewPath(context.Request.Path, context.Request.QueryString, defLangSymbol, langSymbolList);

        if (!string.IsNullOrEmpty(newPath))
        {
            context.Response.Redirect(Url.EncodeIllegalCharacters(newPath), true);
        }
        else
        {
            await next.Invoke(context);
        }
    }

    private bool ShouldSkipProcessing(string requestPath, string baseAddressAdmin)
    {
        return requestPath.Contains(baseAddressAdmin, StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/GetScaledImage", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/GetImage", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/CkEditor/", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/fonts/", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/imgs", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/lib/", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/css/", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/js/", StringComparison.OrdinalIgnoreCase) ||
               requestPath.Contains("/plugins/", StringComparison.OrdinalIgnoreCase);
    }

    private string? GetDefaultLanguageSymbol(HttpContext context, string domainName, List<string> langSymbolList)
    {
        string cookieName = CookieRequestCultureProvider.DefaultCookieName;
        string? defLangSymbol;

        if (context.Request.Cookies[cookieName] != null)
        {
            defLangSymbol = context.Request.Cookies[cookieName]?.Split("|")[0][2..];
        }
        else
        {
            Domain result = domainRepository.FirstOrDefault(d => d.DomainName == "https://" + domainName) ??
                            domainRepository.FirstOrDefault(d => d.IsDefault);
            defLangSymbol = languageRepository.FirstOrDefault(l => l.Id == result.DefaultLanguageId).Symbol;
        }

        return defLangSymbol?.ToLower();
    }

    private string BuildNewPath(PathString requestPath, QueryString queryString, string defLangSymbol, List<string> langSymbolList)
    {
        string newPath = "";
        string pathRequest = "";

        if (requestPath.Value is { Length: 1 })
        {
            newPath = $"/{defLangSymbol.ToLower()}{requestPath}{queryString}";
        }
        else if (requestPath.Value != null && !requestPath.Value.StartsWith($"/{defLangSymbol.ToLower()}"))
        {
            if (langSymbolList.Contains(requestPath.Value.Split("/")[1]))
            {
                foreach (string symbol in langSymbolList.Where(symbol => requestPath.Value.StartsWith($"/{symbol}")))
                {
                    pathRequest = symbol.Length + 1 > requestPath.Value.Length
                                      ? "/"
                                      : requestPath.Value.Remove(0, symbol.Length + 1);
                    break;
                }
            }

            newPath = $"/{defLangSymbol.ToLower()}{(!string.IsNullOrWhiteSpace(pathRequest) ? pathRequest : requestPath.Value)}{queryString}";
            if (newPath.EndsWith("/"))
            {
                newPath = newPath[..^1];
            }
        }

        return newPath;
    }
}