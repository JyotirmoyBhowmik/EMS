using Microsoft.AspNetCore.Mvc;
using ReportingService.Services;
using ReportingService.Models;

namespace ReportingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IPdfReportService _pdfReportService;
    private readonly IExcelReportService _excelReportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IPdfReportService pdfReportService, IExcelReportService excelReportService, ILogger<ReportsController> logger)
    {
        _pdfReportService = pdfReportService;
        _excelReportService = excelReportService;
        _logger = logger;
    }

    [HttpPost("generate/pdf")]
    public async Task<IActionResult> GeneratePdfReport([FromBody] ReportRequest request)
    {
        try
        {
            var pdfBytes = await _pdfReportService.GenerateReportAsync(request);
            return File(pdfBytes, "application/pdf", $"report_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate/excel")]
    public async Task<IActionResult> GenerateExcelReport([FromBody] ReportRequest request)
    {
        try
        {
            var excelBytes = await _excelReportService.GenerateReportAsync(request);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel report");
            return StatusCode(500, "Internal server error");
        }
    }
}
