using Beacon.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beacon.Server.Filters
{
    public class BeaconAuthenticationFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check to see if the token is valid
            string tokenValue = filterContext.HttpContext.Request.Query["Token"];

            if (tokenValue == null)
            {
                filterContext.Result = new UnauthorizedResult();
                return;
            }

            // Validate Token
            using (var db = new BeaconContext())
            {
                var result = db.Token.FirstOrDefault(t => t.Value == tokenValue);

                if (result == null)
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {

        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {

        }
    }
}
