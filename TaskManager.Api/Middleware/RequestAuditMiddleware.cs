using TaskManager.Api.Services;

namespace TaskManager.Api.Middleware;

public class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppInfoService _appInfo;

    public RequestAuditMiddleware(RequestDelegate next, IAppInfoService appInfo)
    {
        _next = next;
        _appInfo = appInfo;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestContextService requestContext,
        ITransientMarkerService transientMarker)
    {
        if (context.Request.Path.StartsWithSegments("/diag"))
        {
            Console.WriteLine($"diag start: {context.Request.Path}, request={requestContext.RequestId}");
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-App-Instance"] = _appInfo.AppInstanceId.ToString();
            context.Response.Headers["X-Request-Id"] = requestContext.RequestId.ToString();
            context.Response.Headers["X-Transient-Id"] = transientMarker.MarkerId.ToString();
            return Task.CompletedTask;
        });

        await _next(context);

        if (context.Request.Path.StartsWithSegments("/diag"))
        {
            Console.WriteLine($"diag finish: {context.Request.Path}, status={context.Response.StatusCode}");
        }
    }
}
