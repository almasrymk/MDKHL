
using AraibcPdfUnicodeGlyphsResharper;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.CodeAnalysis.Elfie.Model;
using OfficeOpenXml;
using SmartParking.DataTable;
using SmartParking.Entities.Organizations;
using SmartParking.Localization;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Data;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            string rightImagePath = organization != null  && !string.IsNullOrEmpty(organization?.OrganizationPicture) ? System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{organization.OrganizationPicture}")
                                                         : System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");
            string title = Data.Name;
            foreach (var columns in Data.Columns)
            {
                if (columns.Name == "Id" || !columns.IsColumnVisible)
                    continue;

                table.Columns.Add(columns.DisplayName, typeof(string));
            }


            int columnCount = table.Columns.Count;
            if (columnCount < 4)
            {
                columnCount = 4;
            }
            int leftImageStart = 1;

            int rightImageStart = columnCount;

            int titleStart = leftImageStart + 1;
           

         


            worksheet.Cells[1, leftImageStart, 2, leftImageStart].Merge = true;

            worksheet.Cells[FromRow: 1, rightImageStart, 2, rightImageStart].Merge = true;

            worksheet.Cells[1, titleStart, 2, columnCount - 1].Merge = true;
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Value = title;
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center; 
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Style.Font.Size = 12;
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Style.Font.Bold = true;
            worksheet.Cells[1, titleStart, 2, columnCount - 1].Style.WrapText = true;
            worksheet.Row(1).Height = 30;
            worksheet.Row(2).Height = 30;
            
           

            

            foreach (var row in Data.Rows)
            {
                var dataRow = table.NewRow();

                foreach (var cell in row.Cells)
                {
                    if (cell.Key == "Id" || !Data.Columns.FirstOrDefault(_ => _.DisplayName == cell.Key).IsColumnVisible) 
                        continue;
                    if(CultureInfo.CurrentCulture.Name == "ar" && (cell.Key.Contains("وقت") || cell.Key.Contains("تاريخ")))
                    {
                        dataRow[cell.Key] = cell.Value.ToString();
                    }
                    else
                    {
                        dataRow[cell.Key] = cell.Value ?? string.Empty;
                    }
                    
                    
                }
                table.Rows.Add(dataRow);
            }

            worksheet.Cells["A4"].LoadFromDataTable(table, true);

           

            worksheet.Cells.AutoFitColumns();

            // Can Error ! 
            worksheet.Cells[4, 1, 4 + table.Rows.Count, columnCount ]
               .Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            using (var headerRange = worksheet.Cells[4, 1, 4, table.Columns.Count])
            {
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); 
                headerRange.Style.Font.Bold = true;
            }
            if (File.Exists(leftImagePath))
            {
                var leftImage = worksheet.Drawings.AddPicture("LeftImage", new FileInfo(leftImagePath));
                var width = worksheet.Column(leftImageStart).Width;
                worksheet.Column(leftImageStart).Width = Math.Max(width, 10);
                double leftColWidthPixels = worksheet.Column(leftImageStart).Width * 7;
                leftImage.SetSize(60,(int)(50 * 1.33));
                int leftOffsetX = (int)((leftColWidthPixels - 60) / 2);
                leftImage.SetPosition(0, 6, leftImageStart - 1, leftOffsetX);
            }
            if (File.Exists(rightImagePath))
            {
                var rightImage = worksheet.Drawings.AddPicture("RightImage", new FileInfo(rightImagePath));
                worksheet.Column(rightImageStart).Width = Math.Max(worksheet.Column(rightImageStart).Width, 10);
                double rightColWidthPixels = worksheet.Column(rightImageStart).Width * 7;
                rightImage.SetSize(60, (int)(50 * 1.33));
                int rightOffsetX = (int)((rightColWidthPixels - 60) / 2);
                rightImage.SetPosition(0, RowOffsetPixels: 6, rightImageStart - 1, rightOffsetX);
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
                string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/fonts/NotoNaskhArabic-Regular.ttf");
                PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                FontSet set = new FontSet();
                set.AddFont(fontPath);
                Style styleBold = new Style();
                styleBold.SetFont(font);
                doc.SetFontProvider(new FontProvider(set));
                var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
                var contactMail = organization == null ? "it@maienalmadina.com" : organization.ContactMail;
                var phoneNum = organization == null ? "0104593681" : organization.PhoneNum;
                var address = organization == null ? "عائشه بنت ابي بكر , 3111 ,حي بني خذره ,المدينة,42311, 6119" : organization.Address;
                Table headerTable = CreateHeaderTable(Data, font, organization);
                // Add the header table to the document
                doc.SetBottomMargin(15);
                int NumberOfColumns = Data.Columns.Where(_ => _.IsColumnVisible).Count();
                if (Data?.Columns?.FirstOrDefault()?.Name == "Id")
                    NumberOfColumns--;
                Table columntable = GetColumnHeader(Data, font, styleBold, NumberOfColumns);
                int pageNumber = 1;

                Table footerTable = AddFooter(pdfDoc, font, contactMail, phoneNum, address, pageNumber);
                int rowOfTable = 0;
                doc.Add(headerTable);
                int maxRows = 21;
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    maxRows = 21;
                    Data.Columns.Reverse();
                }
                Table table = GetColumnHeader(Data, font, styleBold, NumberOfColumns);
                foreach (var row in Data.Rows)
                {

                    if (rowOfTable == maxRows)
                    {
                        rowOfTable = 0;
                        doc.Add(table);
                        table = new Table(NumberOfColumns);
                        table.SetWidth(UnitValue.CreatePercentValue(100));
                        table = GetColumnHeader(Data, font, styleBold, NumberOfColumns);
                        if (CultureInfo.CurrentCulture.Name == "en")
                        {
                            new Canvas(pdfDoc.GetLastPage(), new Rectangle(863, 60))
                            .Add(footerTable);
                        }
                        else
                        {
                            new Canvas(pdfDoc.GetLastPage(), new Rectangle(863, 75))
                                .Add(footerTable);
                        }
                        doc.Add(areaBreak: new AreaBreak());
                        doc.Add(headerTable);
                        pageNumber++;
                        footerTable = AddFooter(pdfDoc, font, contactMail, phoneNum, address, pageNumber);
                    }

                    foreach (var header in Data.Columns)
                    {
                        var cellValue = row.Cells?.FirstOrDefault(c => c.Key == header.DisplayName).Value ?? string.Empty;
                        if (row.Cells?.FirstOrDefault(c => c.Key == header.Name).Key != "Id" && header.IsColumnVisible)
                        {
                            Text txt = CellStyling(header, cellValue, font ,  styleBold);
                            if (CultureInfo.CurrentCulture.Name == "ar")
                            {
                                table.AddCell(new Cell().Add(new Paragraph(txt)).SetHeight(24).SetTextAlignment(TextAlignment.CENTER));

                            }
                            else
                            {

                                table.AddCell(new Cell().Add(new Paragraph(txt)).SetHeight(24).SetTextAlignment(TextAlignment.CENTER));
                            }
                        }
                    }
                    rowOfTable++;
                }
                if (rowOfTable < maxRows)
                {
                    doc.Add(table);
                    if (CultureInfo.CurrentCulture.Name == "en")
                    {
                        new Canvas(pdfDoc.GetLastPage(), new Rectangle(863, 60))
                        .Add(footerTable);
                    }
                    else
                    {
                        new Canvas(pdfDoc.GetLastPage(), new Rectangle(863, 85))
                          .Add(footerTable);
                    }
                }
                doc.Close();
            }

        }

    }

    private static Table CreateHeaderTable(TableData Data, PdfFont font, Organization? organization)
    {
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
            displayTitle = Data.Name.ArabicWithFontGlyphsToPDF();
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


        string rightImagePath = organization != null && !string.IsNullOrEmpty(organization.OrganizationPicture) ? System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{organization.OrganizationPicture}")
                                                     : System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png");

        Image rightImage = new Image(ImageDataFactory.Create(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo/MDHKL_01.png")))
            .SetHeight(75)
            .SetWidth(75)
            .SetMarginRight(20)
            .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
        if (File.Exists(rightImagePath))
        {
            rightImage = new Image(ImageDataFactory.Create(rightImagePath))
           .SetHeight(75)
           .SetWidth(75)
           .SetMarginRight(20)
           .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
        }
        Cell rightCell = new Cell().Add(rightImage)
                                   .SetBorder(Border.NO_BORDER);
        headerTable.AddCell(rightCell);
        return headerTable;
    }

    private static Table GetColumnHeader(TableData Data, PdfFont font, Style styleBold, int NumberOfColumns)
    {
        Table columntable = new Table(NumberOfColumns);
        columntable.SetWidth(UnitValue.CreatePercentValue(100));

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            columntable.SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);  
        }

        foreach (var columns in Data.Columns)
        {
            if (columns.Name == "Id" || !columns.IsColumnVisible)
                continue;
            var txt = CultureInfo.CurrentCulture.Name == "ar" ?
                                           new Text(columns.DisplayName.ArabicWithFontGlyphsToPDF()).SetFont(font)
                                           : new Text(columns.DisplayName);

            var headerCell = new Cell()
                    .Add((new Paragraph(txt))
                    .SetFont(font)
                    .AddStyle(styleBold.SimulateBold())
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    )
                    .SetBackgroundColor(new DeviceRgb(211, 211, 211));
            
            columntable.AddHeaderCell(headerCell);
        }

        return columntable;
    }

    private static Table AddFooter(PdfDocument pdfDoc, PdfFont font, string contactMail, string phoneNum, string address, int pageNumber)
    {
        Table footerTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100))
            .SetBorder(Border.NO_BORDER)
            .SetBorderTop(new SolidBorder(2))
            .SetMarginLeft(36);
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            footerTable.AddCell(new Cell().Add(new Paragraph($"Email: {contactMail}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            footerTable.AddCell(new Cell().Add(new Paragraph($"Phone: {phoneNum}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
            footerTable.AddCell(new Cell().Add(new Paragraph( $"Address: {address.ArabicWithFontGlyphsToPDF()}")).SetFont(font).SetBorder(Border.NO_BORDER).SetFont(font).SetTextAlignment(TextAlignment.LEFT));
            footerTable.AddCell(new Cell().Add(new Paragraph($" {pageNumber}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
        }
        else
        {
            footerTable.SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
            footerTable.AddCell(new Cell().Add(new Paragraph($"رقم التواصل : {phoneNum}".ArabicWithFontGlyphsToPDF())).SetFont(font).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            footerTable.AddCell(new Cell().Add(new Paragraph($"البريد الالكتروني :  {contactMail}".ArabicWithFontGlyphsToPDF())).SetFont(font).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
            footerTable.AddCell(new Cell().Add(new Paragraph($" {pageNumber}".ArabicWithFontGlyphsToPDF()).SetFont(font)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            footerTable.AddCell(new Cell().Add(new Paragraph($"العنوان : {address}".ArabicWithFontGlyphsToPDF())).SetFont(font).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
        }
        return footerTable;
    }
    private static Text CellStyling(ColumnDef header, string cellValue , PdfFont font, Style  styleBold )
    {
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            if ((header.PropertyName.ToLower().Contains("date") == true || header.PropertyName.ToLower().Contains("time") == true) || header.PropertyName.ToLower().Contains("Published") == true || header.Name.ToLower().Contains("code"))
            {
                return new Text(cellValue)
                    .SetBaseDirection(BaseDirection.LEFT_TO_RIGHT).SetFont(font).SetFontSize(10);
            }
            else
            {
                return new Text(cellValue.ArabicWithFontGlyphsToPDF()).SetFont(font).SetFontSize(10);

            }
        }
        else
        {
            if (header.Name.Contains("ArabicName") || (header.Name.Contains("UserName") || header.Name.Contains("Description")))
            {
                return new Text(cellValue.ArabicWithFontGlyphsToPDF()).SetFont(font).SetFontSize(10);
            }
            return new Text(cellValue).SetFont(font).SetFontSize(10);
        }

    }
}
