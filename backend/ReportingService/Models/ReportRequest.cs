namespace ReportingService.Models;

public class ReportRequest
{
    public string ReportType { get; set; } = "General";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> TagIds { get; set; } = new();
}
