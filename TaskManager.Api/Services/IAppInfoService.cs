namespace TaskManager.Api.Services;

public interface IAppInfoService
{
    Guid AppInstanceId { get; }

    DateTime StartedAt { get; }
}
