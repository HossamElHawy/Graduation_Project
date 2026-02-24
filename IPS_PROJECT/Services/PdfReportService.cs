using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IPS_PROJECT.Models;

namespace IPS_PROJECT.Services
{
    public class PdfReportService
    {
        public byte[] GenerateExecutiveReport(List<EVENTS> events, int totalThreats, int benignCount)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                   
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("AI-BASED IPS REPORT").FontSize(22).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Report Period: Last 100 Events | Generated: {DateTime.Now:f}").FontSize(9).Italic();
                        });
                        row.ConstantItem(100).AlignCenter().Text("CONFIDENTIAL").FontColor(Colors.Red.Medium).Bold();
                    });

                    // 2. Content Area
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Summary Cards 
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Padding(5).Background(Colors.Grey.Lighten4).Column(c => {
                                c.Item().AlignCenter().Text("Total Analyzed").FontSize(9);
                                c.Item().AlignCenter().Text(events.Count.ToString()).FontSize(16).Bold();
                            });
                            row.RelativeItem().Padding(5).Background(Colors.Red.Lighten5).Column(c => {
                                c.Item().AlignCenter().Text("Threats Blocked").FontSize(9);
                                c.Item().AlignCenter().Text(totalThreats.ToString()).FontSize(16).Bold().FontColor(Colors.Red.Medium);
                            });
                            row.RelativeItem().Padding(5).Background(Colors.Green.Lighten5).Column(c => {
                                c.Item().AlignCenter().Text("Clean Traffic").FontSize(9);
                                c.Item().AlignCenter().Text(benignCount.ToString()).FontSize(16).Bold().FontColor(Colors.Green.Medium);
                            });
                        });

                        col.Item().PaddingTop(20).Text("Detailed Detection Logs").FontSize(14).SemiBold().Underline();

                        // 3. Table of Logs (الجدول التقني)
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Time
                                columns.RelativeColumn(3); // Source
                                columns.RelativeColumn(2); // Attack
                                columns.RelativeColumn(2); // Conf%
                                columns.RelativeColumn(2); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Timestamp");
                                header.Cell().Element(CellStyle).Text("Source IP");
                                header.Cell().Element(CellStyle).Text("Attack Type");
                                header.Cell().Element(CellStyle).Text("Confidence");
                                header.Cell().Element(CellStyle).Text("Status");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            foreach (var e in events)
                            {
                                table.Cell().Element(RowStyle).Text(e.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                                table.Cell().Element(RowStyle).Text(e.SourceIp ?? "N/A");
                                table.Cell().Element(RowStyle).Text(e.Prediction ?? "Unknown");
                                table.Cell().Element(RowStyle).Text($"{e.Confidence:0}%");
                                table.Cell().Element(RowStyle).Text(e.Status)
                                     .FontColor(e.Status == "Blocked" ? Colors.Red.Medium : Colors.Green.Medium);

                                static IContainer RowStyle(IContainer container) =>
                                    container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            }
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" | IPS System v3.6.1 Professional Report");
                    });
                });
            }).GeneratePdf();
        }
    }
}