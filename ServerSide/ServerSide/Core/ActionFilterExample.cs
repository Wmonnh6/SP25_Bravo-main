using Microsoft.AspNetCore.Mvc.Filters;

namespace ServerSide.Core;

public class ActionFilterExample : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // our code before action executes
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // our code after action executes
    }
}
