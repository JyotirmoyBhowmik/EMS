using ReportingService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ReportingService.Services;

public interface IPdfReportService
{
    Task<byte[]> GenerateReportAsync(ReportRequest request);
}

public class PdfReportService : IPdfReportService
{
    public async Task<byte[]> GenerateReportAsync(ReportRequest request)
    {
        // Setup QuestPDF license (community)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"SCADA Report: {request.ReportType}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Item().Text($"Date Range: {request.StartDate:g} - {request.EndDate:g}");
                        x.Item().Text($"Generated At: {DateTime.Now:g}");
                        
                        x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Light);
                        
                        x.Item().PaddingTop(10).Text("Report Data Placeholder");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
