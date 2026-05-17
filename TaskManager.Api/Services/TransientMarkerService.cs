namespace TaskManager.Api.Services;

public class TransientMarkerService : ITransientMarkerService
{
    public Guid MarkerId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;
}
