namespace TaskManager.Api.Middleware;

public static class TokenExtensions
{
    public static IApplicationBuilder UseToken(this IApplicationBuilder app, string token)
    {
        return app.UseMiddleware<TokenMiddleware>(token);
    }
}
