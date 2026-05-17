namespace TaskManager.Api.Services;

public class DateTimeService : IDateTimeService
{
    public string GetDate() => DateTime.Now.ToShortDateString();

    public string GetTime() => DateTime.Now.ToShortTimeString();
}
