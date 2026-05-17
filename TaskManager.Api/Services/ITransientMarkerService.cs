namespace TaskManager.Api.Services;

public interface ITransientMarkerService
{
    Guid MarkerId { get; }

    DateTime CreatedAt { get; }
}
