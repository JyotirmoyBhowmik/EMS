using ReportingService.Models;
using ClosedXML.Excel;

namespace ReportingService.Services;

public interface IExcelReportService
{
    Task<byte[]> GenerateReportAsync(ReportRequest request);
}

public class ExcelReportService : IExcelReportService
{
    public async Task<byte[]> GenerateReportAsync(ReportRequest request)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("SCADA Report");

        // Header
        worksheet.Cell(1, 1).Value = "Report Type";
        worksheet.Cell(1, 2).Value = request.ReportType;
        
        worksheet.Cell(2, 1).Value = "Start Date";
        worksheet.Cell(2, 2).Value = request.StartDate;

        worksheet.Cell(3, 1).Value = "End Date";
        worksheet.Cell(3, 2).Value = request.EndDate;

        // Data Table Header
        worksheet.Cell(5, 1).Value = "Timestamp";
        worksheet.Cell(5, 2).Value = "Tag Name";
        worksheet.Cell(5, 3).Value = "Value";
        worksheet.Row(5).Style.Font.Bold = true;

        // Placeholder Data
        worksheet.Cell(6, 1).Value = DateTime.Now;
        worksheet.Cell(6, 2).Value = "DemoTag";
        worksheet.Cell(6, 3).Value = 123.45;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
