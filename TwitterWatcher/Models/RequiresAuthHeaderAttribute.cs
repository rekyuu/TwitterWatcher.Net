using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TwitterWatcher;

public class RequiresAuthHeaderAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        string headerValue = context.HttpContext.Request.Headers["X-ACCESS-TOKEN"];
        
        if (string.IsNullOrEmpty(headerValue))
        {
            context.Result = new UnauthorizedObjectResult("Missing authorization header");
            return;
        }

        if (headerValue != Config.Secrets.XAccessToken)
        {
            context.Result = new UnauthorizedObjectResult("Authorization header token is not correct");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}