using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Reports;

public class StudentListReport(List<Student> students) : IDocument
{
    private readonly List<Student> _students = students;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape()); // Landscape gives extra width for student table
            page.Margin(2, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

            // Header Section
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Student Management System").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Official Student Roster").FontSize(11).Italic().FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd")).FontSize(10);
            });

            // Table Section
            page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);  // ID
                        columns.RelativeColumn(2); // Full Name
                        columns.RelativeColumn(3); // Email
                        columns.RelativeColumn(2); // Department
                        columns.RelativeColumn(2); // Enrollment Date
                        columns.RelativeColumn(1); // Status
                    });

                    // Table Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("ID").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Full Name").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Email").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Department").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Enrollment Date").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Status").FontColor(Colors.White).Bold();
                    });

                    // Rows
                    foreach (var student in _students)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.Id.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.FullName ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.Email ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.Major ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.EnrollmentDate.ToString("MMM dd, yyyy"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(student.Status ?? "Senior");
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