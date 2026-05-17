namespace TaskManager.Api.Middleware;

public class TokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _token;

    public TokenMiddleware(RequestDelegate next, string token)
    {
        _next = next;
        _token = token;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query["token"] != _token)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }
}
