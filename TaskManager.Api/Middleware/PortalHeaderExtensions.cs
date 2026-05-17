namespace TaskManager.Api.Middleware;

public static class PortalHeaderExtensions
{
    public static IApplicationBuilder UsePortalHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PortalHeaderMiddleware>();
    }
}
