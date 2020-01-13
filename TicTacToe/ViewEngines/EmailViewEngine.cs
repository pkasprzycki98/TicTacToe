using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe.ViewEngines
{
    public class EmailViewEngine
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public EmailViewEngine(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true); // szukanie widoku
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }
            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true); // szukanie widoku
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }
            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations); // lokalizacje które zostały przeszukane
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Nie znaleziono widoku '{viewName}'. Przeszukano następujące miejsca:" }.Concat(searchedLocations)); ; // zwrócenie inf

            throw new InvalidOperationException(errorMessage);
        }

        public async Task<string> RenderEmailToString<TModel>(string viewName, TModel model)
        {
            var actionContext = GetActionContext(); //A string value that specifies the context name.
			var view = FindView(actionContext, viewName); //znaleźienie widoku na podstawie pobrane actionContext i viewName
            if (view == null)
            {
                throw new InvalidOperationException(string.Format("Nie można znaleźć widoku '{0}'", viewName));
            }

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);
                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
