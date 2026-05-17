namespace TaskManager.Api.Services;

public class AppInfoService : IAppInfoService
{
    public Guid AppInstanceId { get; } = Guid.NewGuid();

    public DateTime StartedAt { get; } = DateTime.Now;
}
