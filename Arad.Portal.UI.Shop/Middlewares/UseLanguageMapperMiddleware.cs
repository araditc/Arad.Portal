using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using Flurl;

namespace Arad.Portal.UI.Shop.Middlewares
{
    public class UseLanguageMapperMiddleware
    {
        private RequestDelegate _next;
        private readonly DomainContext _domainContext;
        private readonly LanguageContext _languageContext;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public UseLanguageMapperMiddleware(RequestDelegate next, DomainContext domainContext,
            IWebHostEnvironment environment,
            LanguageContext languageContext,
            IConfiguration configuration)
        {
            _next = next;
            _domainContext = domainContext;
            _languageContext = languageContext;
            _env = environment;
            _configuration = configuration;

        }
        public async Task Invoke(HttpContext context)
        {
            //Log.Fatal($"In middleware context.Request.PathBase : {context.Request.PathBase}");
            //Log.Fatal($"In middleware context.Request.Path : {context.Request.Path}");
            //Log.Fatal($"domainName context.Request.Host : {context.Request.Host}");
            string defLangSymbol = "";
            string pathRequest = "";
            var langSymbolList = _languageContext.Collection.Find(_ => _.IsActive).Project(_ => _.Symbol.ToLower()).ToList();
            string newPath = "";
            string domainName = "";
            
            domainName = $"{context.Request.Host}";
            var baseAddressAdmin = _configuration["BaseAddress"];
            if(string.IsNullOrWhiteSpace(baseAddressAdmin))
            {
                baseAddressAdmin = "/Admin";
            }

            if (context.Request.Path.ToString().Contains(baseAddressAdmin, StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/GetScaledImage", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/GetImage", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/GetScaledImageOnWidth", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/CkEditor/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/ImageSlider/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/fonts/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/imgs", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/lib/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/css/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/js/", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.ToString().Contains("/plugins/", StringComparison.OrdinalIgnoreCase))
            {
                //Log.Fatal("skiiiiiiiiiiiiiiip");
                await _next.Invoke(context);
            }
            else
            {


                //first step checke whether this cookie exist or not
                var cookieName = CookieRequestCultureProvider.DefaultCookieName;
                //Log.Fatal($"cookiename is:{cookieName}");
                if (context.Request.Cookies[cookieName] != null)
                {
                    //Log.Fatal("context.Request.Cookies[cookieName] isnt null;");
                    var cookieValue = context.Request.Cookies[cookieName];
                    //Log.Fatal($"cookieValue : {cookieValue}");
                    defLangSymbol = cookieValue.Split("|")[0][2..];
                    //Log.Fatal($"find default lang symbole from cookie: {defLangSymbol}");
                }
                else if (false)
                {
                    //check the culture of request based on
                    //IP Address and then check if we support this culture or not
                    //defLang =...
                }
                else
                if (defLangSymbol == "")
                {
                    //Log.Fatal("defLangSymbol is empty");
                    DataLayer.Entities.General.Domain.Domain result = null;
                    if (_domainContext.Collection.Find(_ => _.DomainName == $"{domainName}").Any())
                    {
                        result = _domainContext.Collection.Find(_ => _.DomainName == $"{domainName}").FirstOrDefault();
                        //Log.Fatal("domainEntity with domainName found");
                    }
                    else
                    {
                        result = _domainContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                        //Log.Fatal("first default domain Found");
                    }
                    var lanEntity = _languageContext.Collection.Find(_ => _.LanguageId == result.DefaultLanguageId).FirstOrDefault();
                    defLangSymbol = lanEntity.Symbol;
                    //Log.Fatal($"defLangSymbol is: {defLangSymbol}");
                }

                //if cookie is null we write the cookie
                if (context.Request.Cookies[cookieName] == null)
                {
                    context.Response.Cookies.Append(cookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(defLangSymbol)),
                        new CookieOptions() { Domain = domainName, Expires = DateTime.Now.AddDays(7) });
                    //Log.Fatal("coockie set because it was null");
                }
                
                
                if (context.Request.Path.Value.Length == 1)
                {
                    newPath = context.Request.Path + defLangSymbol.ToLower();
                    //Log.Fatal($"FirstRedirect redirecting to newpath pathlength equal one :{newPath}");
                    context.Response.Redirect(newPath, true);
                }
                else
                if (!context.Request.Path.Value.StartsWith($"/{defLangSymbol.ToLower()}"))
                {
                    if (langSymbolList.Contains(context.Request.Path.Value.Split("/")[1])) //when it has another lang symble rathen than current symbol
                    {
                        foreach (var symbol in langSymbolList)
                        {
                            if (context.Request.Path.Value.StartsWith($"/{symbol}"))
                            {
                                if (symbol.Length + 2 > context.Request.Path.Value.Length)
                                {
                                    pathRequest = "/";
                                    Log.Fatal($"path request going to be '//////////'");
                                }
                                else
                                {
                                    pathRequest = context.Request.Path.Value.Remove(0, symbol.Length + 1);
                                    Log.Fatal($"remove another langsymble from path and path request is :{pathRequest}");
                                }
                                break;
                            }
                        }
                    }
                    newPath = $"/{defLangSymbol.ToLower()}" +
                   $"{(!string.IsNullOrWhiteSpace(pathRequest) ? pathRequest : context.Request.Path.Value) + (context.Request.QueryString.Value != "/" ? context.Request.QueryString : "")}";
                    Log.Fatal($"newpath is going to be : {newPath}");
                    if (newPath.EndsWith("/"))
                    {
                        newPath = newPath.Substring(0, newPath.Length - 1);
                    }

                    Log.Fatal($"Second Redirect : gonna redirect to newPath to be encoded:{newPath}");
                    var encoded = Url.EncodeIllegalCharacters(newPath);
                    context.Response.Redirect(encoded, true);
                }
                else
                {
                    //Log.Fatal($"Second Skiiiiiiiiiiiiiiip when it has langsymbol is path :{context.Request.Path}");
                    await _next.Invoke(context);
                }
            }
           // await _next.Invoke(context);

        }
    }
}
