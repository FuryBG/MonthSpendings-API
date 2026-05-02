using Serilog.Context;
using System.Security.Claims;

namespace MonthSpendings.Middleware;

public class UserIdEnricherMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is not null)
        {
            using (LogContext.PushProperty("UserId", userId))
            {
                await next(context);
                return;
            }
        }

        await next(context);
    }
}
