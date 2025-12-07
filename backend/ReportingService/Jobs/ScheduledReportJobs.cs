using Quartz;
using ReportingService.Services;

namespace ReportingService.Jobs;

public class DailyProductionReportJob : IJob
{
    private readonly ILogger<DailyProductionReportJob> _logger;
    private readonly IPdfReportService _pdfService;
    private readonly IEmailService _emailService;

    public DailyProductionReportJob(
        ILogger<DailyProductionReportJob> logger,
        IPdfReportService pdfService,
        IEmailService emailService)
    {
        _logger = logger;
        _pdfService = pdfService;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Generating daily production report...");

            // Generate report for yesterday
            var reportDate = DateTime.UtcNow.AddDays(-1);
            var reportPath = $"reports/daily-production-{reportDate:yyyyMMdd}.pdf";

            // Generate PDF report
            var sampleData = new Dictionary<string, object>
            {
                ["ReportDate"] = reportDate,
                ["Production Metrics"] = "Sample data from database"
            };

            await _pdfService.GenerateReportAsync(sampleData, reportPath);

            // Send via email
            var recipients = context.JobDetail.JobDataMap.GetString("Recipients") ?? "manager@company.com";
            await _emailService.SendEmailAsync(
                recipients,
                $"Daily Production Report - {reportDate:yyyy-MM-dd}",
                $"<h2>Daily Production Report</h2><p>Report for {reportDate:yyyy-MM-dd} is attached.</p>",
                reportPath
            );

            _logger.LogInformation("Daily production report completed and sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily production report");
            throw;
        }
    }
}

public class WeeklyEnergyReportJob : IJob
{
    private readonly ILogger<WeeklyEnergyReportJob> _logger;
    private readonly IExcelReportService _excelService;
    private readonly IEmailService _emailService;

    public WeeklyEnergyReportJob(
        ILogger<WeeklyEnergyReportJob> logger,
        IExcelReportService excelService,
        IEmailService emailService)
    {
        _logger = logger;
        _excelService = excelService;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Generating weekly energy report...");

            var reportPath = $"reports/weekly-energy-{DateTime.UtcNow:yyyyMMdd}.xlsx";

            // Generate Excel report
            var sampleData = new Dictionary<string, object>
            {
                ["Week"] = DateTime.UtcNow.AddDays(-7),
                ["Energy Data"] = "Sample energy consumption data"
            };

            await _excelService.GenerateReportAsync(sampleData, reportPath);

            // Send via email
            var recipients = context.JobDetail.JobDataMap.GetString("Recipients") ?? "energy-team@company.com";
            await _emailService.SendEmailAsync(
                recipients,
                $"Weekly Energy Report - Week {DateTime.UtcNow:yyyy-MM-dd}",
                "<h2>Weekly Energy Report</h2><p>Energy consumption report for the past week is attached.</p>",
                reportPath
            );

            _logger.LogInformation("Weekly energy report completed and sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate weekly energy report");
            throw;
        }
    }
}
