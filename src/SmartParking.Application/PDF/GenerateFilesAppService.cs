using AraibcPdfUnicodeGlyphsResharper;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.CodeAnalysis.Elfie.Model;
using OfficeOpenXml;
using SmartParking.DataTable;
using SmartParking.Organizations;
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

public class GenerateFilesAppService (IOrganizationsAppService organizationsAppService) : IGenerateFilesAppService, ITransientDependency
{
    private readonly IOrganizationsAppService _organizationsAppService = organizationsAppService;

   
    public async Task ExportToExcel(TableData Data, string pathFileName)
    {
        await Task.Yield();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Data.Name);
            System.Data.DataTable table = new System.Data.DataTable();
            string leftImagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");

            var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
            string rightImagePath = organization != null ? System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{organization.OrganizationPicture}")
                                                         : System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");
            string title = Data.Name;
            foreach (var columns in Data.Columns)
            {
                if (columns.Name == "Id")
                    continue;

                table.Columns.Add(columns.DisplayName, typeof(string));
            }
            int columnCount = table.Columns.Count; 

            int leftImageStart = 1;  

            int rightImageStart = columnCount;  

            int titleStart = leftImageStart + 1;  

            worksheet.Cells[1, leftImageStart, 2, leftImageStart].Merge = true;

            worksheet.Cells[1, rightImageStart, 2, rightImageStart].Merge = true;

            worksheet.Cells[2, titleStart , 2, columnCount - 1].Merge = true;
            worksheet.Cells[2, titleStart].Value = title;
            worksheet.Cells[2, titleStart].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, titleStart].Style.Font.Size = 12;
            worksheet.Cells[2, titleStart].Style.Font.Bold = true;
            worksheet.Cells[2, titleStart].Style.WrapText = true;


            worksheet.Row(1).Height = 25;
            worksheet.Row(2).Height = 30;
            

            

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

            worksheet.Cells["A4"].LoadFromDataTable(table, true);

           

            worksheet.Cells.AutoFitColumns();

            
            worksheet.Cells[4, 1, 4 + table.Rows.Count, columnCount ]
               .Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            using (var headerRange = worksheet.Cells[4, 1, 4, columnCount])
            {
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); 
                headerRange.Style.Font.Bold = true;
            }

            if (File.Exists(leftImagePath))
            {
                var leftImage = worksheet.Drawings.AddPicture("LeftImage", new FileInfo(leftImagePath));
                leftImage.SetPosition(0, RowOffsetPixels: 0, leftImageStart - 1, 0); // Position at column 1
                leftImage.SetSize(PixelWidth: (int)((worksheet.Column(1).Width * 7)+3) , (int)(55 * 1.33));
            }


            if (File.Exists(rightImagePath))
            {
                var rightImage = worksheet.Drawings.AddPicture("RightImage", new FileInfo(rightImagePath));
                rightImage.SetPosition(0, 0, rightImageStart - 1, ColumnOffsetPixels: 0); // Position at last 2 columns
                rightImage.SetSize((int)((worksheet.Column(columnCount).Width * 7) + 3) , (int)(55 * 1.33));
            }
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
                Table headerTable = new Table(3);
                headerTable.SetWidth(UnitValue.CreatePercentValue(100));
               
                // Left image
                string leftImagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");
                Image leftImage = new Image(ImageDataFactory.Create(leftImagePath))
                    .SetHeight(75)
                    .SetWidth(75)
                    .SetMarginLeft(20)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                Cell leftCell = new Cell().Add(leftImage)
                                          .SetBorder(Border.NO_BORDER);
                headerTable.AddCell(leftCell);
                string displayTitle = "";
                // Centered title
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    displayTitle = Data.Name;
                }
                else
                {
                    displayTitle = Data.Name.ArabicWithFontGlyphsToPfd();
                }
                    Paragraph title = new Paragraph(displayTitle) // Report Title in Arabic
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(font)
                    .SetFontSize(25)
                    .SimulateBold()
                    .SetMarginTop(20);
                 
                Cell centerCell = new Cell().Add(title)
                                            .SetTextAlignment(TextAlignment.CENTER)
                                            .SetBorder(Border.NO_BORDER);
                headerTable.AddCell(centerCell);
                var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
                string rightImagePath = organization != null ? System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{organization.OrganizationPicture}")
                                                             : System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");
                Image rightImage = new Image(ImageDataFactory.Create(rightImagePath))
                    .SetHeight(75)
                    .SetWidth(75)
                    .SetMarginRight(20)
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                Cell rightCell = new Cell().Add(rightImage)
                                           .SetBorder(Border.NO_BORDER);
                headerTable.AddCell(rightCell);

                // Add the header table to the document
                doc.Add(headerTable);
                doc.SetBottomMargin(15);
               
                int NumberOfColumns = Data.Columns.Count;
                if (Data?.Columns?.FirstOrDefault()?.Name == "Id")
                    NumberOfColumns--;
                Table table = new Table(NumberOfColumns);

                table.SetWidth(UnitValue.CreatePercentValue(100));

                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    table.SetTextAlignment(TextAlignment.RIGHT);
                    table.SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
                    Data.Columns.Reverse();
                }
                
                foreach (var columns in Data.Columns)
                {
                    if (columns.Name == "Id")
                        continue;
                    var txt = new Text("");
                    if (CultureInfo.CurrentCulture.Name=="ar")
                    {
                        txt = new Text(columns.DisplayName.ArabicWithFontGlyphsToPfd());

                    }
                    else
                    {
                        txt = new Text(columns.DisplayName);

                    }
                    table.AddHeaderCell(new Cell().Add(new Paragraph(txt)))
                         .AddStyle(styleBold.SimulateBold())
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER);
                }
                foreach (var row in Data.Rows)
                {
                    foreach (var header in Data.Columns)
                    {
                        var cellValue = row.Cells?.FirstOrDefault(c => c.Key == header.DisplayName).Value ?? string.Empty;
                        if (row.Cells?.FirstOrDefault(c => c.Key == header.Name).Key != "Id")
                        {
                            var txt = new Text("");
                            if (CultureInfo.CurrentCulture.Name == "ar")
                            {
                                txt = new Text(cellValue.ArabicWithFontGlyphsToPfd()).SetFontSize(9);

                            }
                            else
                            {
                                txt = new Text(cellValue).SetFontSize(9);
                            }
                            table.AddCell(new Cell().Add(new Paragraph(txt))).SetTextAlignment(TextAlignment.CENTER); 
                        }
                    }
                }    
                doc.Add(table);

                doc.Close();
            }
        }

    }
}
