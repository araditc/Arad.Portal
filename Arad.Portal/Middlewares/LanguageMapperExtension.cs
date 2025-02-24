using Microsoft.AspNetCore.Builder;

namespace Arad.Portal.Middlewares;

public static class LanguageMapperExtension
{
    public static IApplicationBuilder ApplyLanguageMapper(this IApplicationBuilder app)
    {
        app.UseMiddleware<UseLanguageMapperMiddleware>();
        return app;
    }
}