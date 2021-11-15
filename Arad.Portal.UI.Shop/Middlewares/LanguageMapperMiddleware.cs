using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
            string defLang = "";
            string newPath = "";
            var domainName = $"{context.Request.Host}";
            //if(context.Request.Path.ToString().Contains("images"))
            //{
            //    await _next.Invoke(context);
            //} 
            //else
            //{
                if (domainName.ToString().ToLower().StartsWith("localhost"))
                {
                    domainName = context.Request.Host.ToString().Substring(0, 9);
                }

                //first step checke whether this cookie exist or not
                var cookieName = $"defLang{domainName}";
                if (context.Request.Cookies[cookieName] != null)
                {
                    defLang = context.Request.Cookies[cookieName];
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
                    defLang = result.DefaultLanguageId;
                }

                //if cookie is null we write the cookie
                if (context.Request.Cookies[cookieName] == null)
                {
                    context.Response.Cookies.Append(cookieName, defLang, new CookieOptions() { Domain = domainName, Expires = DateTime.Now.AddDays(7) });
                }

                var lang = _languageContext.Collection.Find(_ => _.LanguageId == defLang).First();
                var redirectUrl = $"{domainName}/{lang.Symbol}";
                if (context.Request.Path.Value.Length == 1)
                {
                    newPath = context.Request.Path + lang.Symbol.ToLower();
                    context.Response.Redirect(newPath, true);
                }
                else
                if (!context.Request.Path.Value.StartsWith($"/{lang.Symbol.ToLower()}"))
                {
                    newPath = $"/{lang.Symbol.ToLower()}{context.Request.Path}";
                    context.Response.Redirect(newPath, true);
                }
                else
                {
                    await _next.Invoke(context);
                }
            //}
           
          
        }
    }
}
