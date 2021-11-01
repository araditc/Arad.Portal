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

            string newPath = "";
            var domainName = $"{context.Request.Scheme}://{context.Request.Host}";
            var result = _domainContext.Collection.Find(_ => _.DomainName == domainName).First();
            var defLang = result.DefaultLanguageId;

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
            
            //_next = new RequestDelegate((context) => {
            //    context.Request.Host = new HostString($"{domainName}/{lang.Symbol.ToLower()}");
            //    return Task.CompletedTask; });

            await _next.Invoke(context);
        }
    }
}
