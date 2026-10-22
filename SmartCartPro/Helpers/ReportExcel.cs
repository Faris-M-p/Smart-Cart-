using ClosedXML.Excel;
using SmartCartPro.Models;

namespace SmartCartPro.Helpers;

public static class ReportExcel
{
    public static byte[] Build(OutputReportScreen screen)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Report");
        var row = 1;
        sheet.Cell(row, 1).Value = screen.CompanyName;
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        sheet.Cell(row, 1).Value = screen.Title;
        row++;
        sheet.Cell(row, 1).Value = "Store";
        sheet.Cell(row, 2).Value = screen.StoreLabel;
        row++;
        if (screen.ShowDates)
        {
            sheet.Cell(row, 1).Value = "From";
            sheet.Cell(row, 2).Value = screen.Filter.DateFrom?.ToString("dd MMM yyyy") ?? "";
            sheet.Cell(row, 3).Value = "To";
            sheet.Cell(row, 4).Value = screen.Filter.DateTo?.ToString("dd MMM yyyy") ?? "";
            row++;
        }

        sheet.Cell(row, 1).Value = "Printed by";
        sheet.Cell(row, 2).Value = screen.PrintedBy ?? "";
        sheet.Cell(row, 3).Value = screen.PrintedAtLocal.ToString("dd MMM yyyy HH:mm");
        row += 2;

        if (!string.IsNullOrWhiteSpace(screen.Notice))
        {
            sheet.Cell(row, 1).Value = screen.Notice;
            row += 2;
        }

        var colCount = Math.Max(screen.Columns.Count, 1);
        for (var c = 0; c < screen.Columns.Count; c++)
        {
            var cell = sheet.Cell(row, c + 1);
            cell.Value = screen.Columns[c].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F8");
        }

        row++;
        foreach (var line in screen.Rows)
        {
            for (var c = 0; c < line.Length && c < colCount; c++)
                sheet.Cell(row, c + 1).Value = line[c].Text;
            row++;
        }

        if (screen.Totals is { Length: > 0 })
        {
            for (var c = 0; c < screen.Totals.Length && c < colCount; c++)
            {
                var cell = sheet.Cell(row, c + 1);
                cell.Value = screen.Totals[c].Text;
                cell.Style.Font.Bold = true;
            }
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
