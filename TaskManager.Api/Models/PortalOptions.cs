namespace TaskManager.Api.Models;

public class PortalOptions
{
    public string Title { get; set; } = "";
    public string Semester { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public string CampusName { get; set; } = "";
    public string Dean { get; set; } = "";
    public string Building { get; set; } = "";
    public AdminOptions Admin { get; set; } = new();
    public List<string> Modules { get; set; } = new();
}
