using Microsoft.Extensions.Options;
using TaskManager.Api.Models;

namespace TaskManager.Api.Middleware;

public class PortalHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PortalOptions _options;

    public PortalHeaderMiddleware(RequestDelegate next, IOptions<PortalOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Portal-Title"] = _options.Title;
        context.Response.Headers["X-Portal-Semester"] = _options.Semester;

        return _next(context);
    }
}
