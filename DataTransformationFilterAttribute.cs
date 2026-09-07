using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Finance.Helpers;

namespace Finance
{
    public class DataTransformationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ViewResult viewResult)
            {
                if (viewResult.Model is AEDV model)
                {
                    // Directly modify the model data
                    
                }
            }
            base.OnActionExecuted(context);
        }
    }
}