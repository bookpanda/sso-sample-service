using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PreflightRequestFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Method == "OPTIONS")
        {
            IHeaderDictionary headers = context.HttpContext.Response.Headers;
            headers.Append("Access-Control-Allow-Origin", "https://sso-svc-1.bookpanda.dev");
            headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

            context.Result = new OkResult();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
