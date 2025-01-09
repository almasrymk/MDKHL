using AraibcPdfUnicodeGlyphsResharper;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.CodeAnalysis.Elfie.Model;
using OfficeOpenXml;
using SmartParking.DataTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.PDF;

public class GenerateFilesAppService : IGenerateFilesAppService, ITransientDependency
{
    public async Task ExportToExcel(TableData Data, string pathFileName)
    {
        await Task.Yield();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Data.Name);
            System.Data.DataTable table = new System.Data.DataTable();
            foreach (var columns in Data.Columns)
            {
                if (columns.Name == "Id")
                    continue;

                table.Columns.Add(columns.DisplayName, typeof(string));
            }

            foreach (var row in Data.Rows)
            {
                var dataRow = table.NewRow();
                foreach (var cell in row.Cells)
                {
                    if (cell.Key == "Id") continue;
                    dataRow[cell.Key] = cell.Value;
                }
                table.Rows.Add(dataRow);
            }

            worksheet.Cells["A1"].LoadFromDataTable(table, true);

            worksheet.Cells.AutoFitColumns();

            FileInfo excelFile = new FileInfo(pathFileName);
            package.SaveAs(excelFile);
        }
    }

    public async Task ExportToPdf(TableData Data, string fileName)
    {

        await Task.Yield();
        FileInfo file = new FileInfo(fileName);

        file.Directory.Create();

        using (PdfWriter writer = new PdfWriter(file.FullName))
        {
            using (PdfDocument pdfDoc = new PdfDocument(writer))
            {
                Document doc = new Document(pdfDoc, new PageSize(900, 842));
                string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//fonts//NotoNaskhArabic-Regular.ttf");
                PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                FontSet set = new FontSet();

                set.AddFont(fontPath);
                Style styleBold = new Style();

                styleBold.SetFont(font);

                doc.SetFontProvider(new FontProvider(set));
                int NumberOfColumns = Data.Columns.Count;
                if (Data?.Columns?.FirstOrDefault()?.Name == "Id")
                    NumberOfColumns--;
                Table table = new Table(NumberOfColumns);

                table.SetWidth(UnitValue.CreatePercentValue(100));
                if(CultureInfo.CurrentCulture.Name == "ar")
                {
                    table.SetTextAlignment(TextAlignment.RIGHT);
                    table.SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
                }
                
                foreach (var columns in Data.Columns)
                {
                    if (columns.Name == "Id")
                        continue;
                    var txt = new Text(columns.DisplayName.ArabicWithFontGlyphsToPfd());
                    table.AddHeaderCell(new Cell().Add(new Paragraph(txt)))
                         .AddStyle(styleBold.SimulateBold())
                        .SetFontSize(9);
                }

                foreach (var row in Data.Rows)
                {

                    foreach (var header in Data.Columns)
                    {

                        var cellValue = row.Cells?.FirstOrDefault(c => c.Key == header.DisplayName).Value ?? string.Empty;
                        if (row.Cells?.FirstOrDefault(c => c.Key == header.Name).Key != "Id")
                        {
                            var txt = new Text(cellValue.ArabicWithFontGlyphsToPfd()).SetFontSize(9);
                            table.AddCell(new Cell().Add(new Paragraph(txt)));
                        }


                    }
                }

                doc.Add(table);

                doc.Close();
            }
        }

    }
}
