using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Middlewares
{
    public class LanguageMapperMiddleware
    {
        private RequestDelegate _next;
        private readonly DomainContext _domainContext;
        private readonly LanguageContext _languageContext;

        public LanguageMapperMiddleware(RequestDelegate next, DomainContext domainContext,
            LanguageContext languageContext)
        {
            _next = next;
            _domainContext = domainContext;
            _languageContext = languageContext;
        }
        public async Task Invoke(HttpContext context)
        {

            string defLangSymbol = "";
            string pathRequest = "";
            var langSymbolList = _languageContext.Collection.Find(_ => _.IsActive).Project(_ => _.Symbol.ToLower()).ToList();
            string newPath = "";
            var domainName = $"{context.Request.Host}";
            if (context.Request.Path.ToString().Contains("/FileManager/GetScaledImage") ||
                context.Request.Path.ToString().Contains("/FileManager/GetImage") ||
                context.Request.Path.ToString().Contains("/lib/") ||
                context.Request.Path.ToString().Contains("/css/") ||
                context.Request.Path.ToString().Contains("/js/") ||
                context.Request.Path.ToString().Contains("/plugins/"))
            {
                await _next.Invoke(context);
            }
            else
            {
                if (domainName.ToString().ToLower().StartsWith("localhost"))
                {
                    domainName = context.Request.Host.ToString().Substring(0, 9);
                }

                //first step checke whether this cookie exist or not
                var cookieName = $"{CookieRequestCultureProvider.DefaultCookieName}";
                if (context.Request.Cookies[cookieName] != null)
                {
                    var cookieValue = context.Request.Cookies[cookieName];
                    defLangSymbol = cookieValue.Split("|")[0].Substring(2);
                }
                else if (false)
                {
                    //check the culture of request based on
                    //IP Address and then check if we support this culture or not
                    //defLang =...

                }
                else
                {
                    var result = _domainContext.Collection.Find(_ => _.DomainName == $"{context.Request.Scheme}://{context.Request.Host}").First();
                    defLangSymbol = result.DefaultLangSymbol;
                }

                //if cookie is null we write the cookie
                if (context.Request.Cookies[cookieName] == null)
                {
                    context.Response.Cookies.Append(cookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(defLangSymbol)),
                        new CookieOptions() { Domain = domainName, Expires = DateTime.Now.AddDays(7) });
                }

                var lang = _languageContext.Collection.Find(_ => _.Symbol == defLangSymbol).First();
                //var redirectUrl = $"{domainName}/{lang.Symbol}";
                if (context.Request.Path.Value.Length == 1)
                {
                    newPath = context.Request.Path + lang.Symbol.ToLower();
                    context.Response.Redirect(newPath, true);
                }
                else
                if (!context.Request.Path.Value.StartsWith($"/{lang.Symbol.ToLower()}"))
                {
                    if (langSymbolList.Contains(context.Request.Path.Value.Substring(1)))
                    {
                        foreach (var symbol in langSymbolList)
                        {
                            if (context.Request.Path.Value.StartsWith($"/{symbol}"))
                            {
                                if (symbol.Length + 2 > context.Request.Path.Value.Length)
                                {
                                    pathRequest = "/";
                                }
                                else
                                {
                                    pathRequest = "/" + context.Request.Path.Value.Remove(symbol.Length + 2);
                                }
                                break;
                            }
                        }
                    }
                    newPath = $"/{lang.Symbol.ToLower()}" +
                   $"{(!string.IsNullOrWhiteSpace(pathRequest) ? pathRequest : context.Request.Path.Value) + (context.Request.QueryString.Value != "/" ? context.Request.QueryString : "")}";
                    if (newPath.EndsWith("/"))
                    {
                        newPath = newPath.Substring(0, newPath.Length - 1);
                    }
                    context.Response.Redirect(newPath, true);
                }
                else
                {
                    await _next.Invoke(context);
                }
            }
        }
    }
}
