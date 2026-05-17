namespace TaskManager.Api.Services;

public class EnvironmentReportService : IEnvironmentReportService
{
    private readonly IWebHostEnvironment _env;

    public EnvironmentReportService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string GetReport()
    {
        return $"""
Environment: {_env.EnvironmentName}
Application: {_env.ApplicationName}
Content root: {_env.ContentRootPath}
Web root: {_env.WebRootPath ?? "no wwwroot"}
""";
    }
}
