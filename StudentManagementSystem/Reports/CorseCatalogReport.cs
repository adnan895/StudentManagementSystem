using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Reports
{
    public class CourseCatalogReport : IDocument
    {
        private readonly List<Course> _courses;

        public CourseCatalogReport(List<Course> courses)
        {
            _courses = courses;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Student Management System").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text("Official Course Catalog").FontSize(12).Italic().FontColor(Colors.Grey.Medium);
                    });

                    row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd")).FontSize(10);
                });

                // Body Content Table
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Title
                            columns.RelativeColumn(1); // Credits
                            columns.RelativeColumn(2); // Department
                            columns.RelativeColumn(2); // Instructor
                            columns.RelativeColumn(1); // Enrolled
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Course Title").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Credits").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Department").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Instructor").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Enrolled").FontColor(Colors.White).Bold();
                        });

                        // Table Rows
                        foreach (var course in _courses)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(course.Title);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{course.Credits} Cr.");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(course.Department ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(course.Instructor?.Name ?? "Unassigned");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text((course.Enrollments?.Count ?? 0).ToString());
                        }
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }
    }
}