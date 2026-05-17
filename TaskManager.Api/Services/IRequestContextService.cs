namespace TaskManager.Api.Services;

public interface IRequestContextService
{
    Guid RequestId { get; }

    DateTime CreatedAt { get; }
}
