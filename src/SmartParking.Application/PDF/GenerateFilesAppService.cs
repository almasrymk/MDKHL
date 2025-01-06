using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using OfficeOpenXml;
using SmartParking.DataTable;
using System;
using System.Collections.Generic;
using System.Data;
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
            System.Data.DataTable table = new System.Data.DataTable();
            foreach (var columns in Data.Columns)
            {
                if (columns.DisplayName == "Id")
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
                    Document doc = new Document(pdfDoc);


                    string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//fonts//NotoNaskhArabic-Regular.ttf");
                    PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                    FontSet set = new FontSet();

                    set.AddFont(fontPath);
                    Style styleBold = new Style();

                    styleBold.SetFont(font).SetBaseDirection(BaseDirection.RIGHT_TO_LEFT).SetFontSize(10);

                    doc.SetFontProvider(new FontProvider(set));
                    var table = new Table(Data.Columns.Count - 1);

                    foreach (var columns in Data.Columns)
                    {
                        if (columns.DisplayName == "Id")
                            continue;
                        var txt = new Text(columns.DisplayName).SetFont(font) .SetFontSize(10).SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
                        table.AddHeaderCell(new Cell().Add(new Paragraph(txt)))
                             .AddStyle(styleBold.SetBold())
                             .SetFont(font)
                             .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                            .SetFontSize(10);
                    }

                    foreach (var row in Data.Rows)
                    {

                        foreach (var header in Data.Columns)
                        {

                            var cellValue = row.Cells?.FirstOrDefault(c => c.Key == header.DisplayName).Value ?? string.Empty;
                            if (row.Cells?.FirstOrDefault(c => c.Key == header.DisplayName).Key != "Id")
                            {
                                var txt = new Text(cellValue).SetFont(font).SetFontSize(10).SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
                                table.AddCell(new Cell().Add(new Paragraph(txt)
                                    .SetFont(font).SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)

                                    )

                                    );
                            }
                            

                        }
                    }

                    doc.Add(table);

                    doc.Close();
                }
            }

    }
}
