using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Finance
{
    public class CustomValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //if (context.ActionArguments.TryGetValue("model", out var value) && value is MyCustomModel model)
            //{
            //    // Validate Name
            //    if (string.IsNullOrWhiteSpace(model.Name))
            //    {
            //        context.ModelState.AddModelError(nameof(model.Name), "Name cannot be empty or whitespace.");
            //    }
            //    // Validate Address
            //    if (string.IsNullOrWhiteSpace(model.Address))
            //    {
            //        context.ModelState.AddModelError(nameof(model.Address), "Address cannot be empty or whitespace.");
            //    }
            //}
            //if (!context.ModelState.IsValid)
            //{
            //    // Assuming the controller action expects a return of the same view with the model
            //    context.Result = new ViewResult
            //    {
            //        ViewName = context.RouteData.Values["action"].ToString(),  // Gets the action name
            //        ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
            //        {
            //            Model = context.ActionArguments.FirstOrDefault(arg => arg.Value is MyCustomModel).Value
            //        }
            //    };
            //}
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Custom logic after the action executes
            // For this filter, we do nothing here, but you could add post-processing logic if needed.
        }
    }
}