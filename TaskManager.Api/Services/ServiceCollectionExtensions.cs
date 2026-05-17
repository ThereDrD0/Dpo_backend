namespace TaskManager.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStudentPortalServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IEnvironmentReportService, EnvironmentReportService>();

        return services;
    }
}
