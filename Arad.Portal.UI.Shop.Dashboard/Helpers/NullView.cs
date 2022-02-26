using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class NullView : IView
    {
        public static readonly NullView Instance = new();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.CompletedTask;
        }
    }
}
