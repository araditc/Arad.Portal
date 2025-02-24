using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Arad.Portal.Helpers.UI;

public interface IRazorPartialToStringRenderer
{
    Task<string> RenderToString(string partialName, object model);
}

public class RazorPartialToStringRenderer : IRazorPartialToStringRenderer
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHttpContextAccessor _contextAccessor;

    public RazorPartialToStringRenderer(IRazorViewEngine razorViewEngine,
                                        ITempDataProvider tempDataProvider,
                                        IHttpContextAccessor contextAccessor)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _contextAccessor = contextAccessor;
    }

    public async Task<string> RenderToString(string viewName, object model)
    {
        ActionContext actionContext = new ActionContext(_contextAccessor.HttpContext, _contextAccessor.HttpContext.GetRouteData(), new());

        await using StringWriter sw = new StringWriter();
        IView viewResult = FindView(actionContext, viewName);

        if (viewResult == null)
        {
            throw new ArgumentNullException($"{viewName} does not match any available view");
        }

        ViewDataDictionary viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new())
                                            {
                                                Model = model
                                            };

        ViewContext viewContext = new ViewContext(
            actionContext,
            viewResult,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            sw,
            new()
        );

        await viewResult.RenderAsync(viewContext);
        return sw.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        ViewEngineResult getViewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        ViewEngineResult findViewResult = _razorViewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        IEnumerable<string> searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        string errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }
}