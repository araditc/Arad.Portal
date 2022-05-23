using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Middlewares
{
    public static class LanguageMapperExtension
    {
        public static IApplicationBuilder ApplyLanguageMapper(this IApplicationBuilder app)
        {
            app.UseMiddleware<UseLanguageMapperMiddleware>();
            return app;
        }
    }
}
