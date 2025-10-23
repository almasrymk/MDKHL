using AraibcPdfUnicodeGlyphsResharper;
using Humanizer;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.Extensions.Options;
using Microsoft.Reporting.NETCore;
using OfficeOpenXml;
using QRCoder;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Utilities;
using SmartParking.ZATCATaxInvoice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.SvgQRCode;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Path = System.IO.Path;

namespace SmartParking.PDF;

public abstract class BasePDFGeneratorAppService<DTO>
{
    private readonly PdfSettings _pdfSettings;
    protected PdfFont Font { get; private set; }
    protected CultureInfo Culture => CultureInfo.CurrentCulture;
    protected string CultureName => CultureInfo.CurrentCulture.Name;

    public virtual void ImagesSection(Document doc, string OrganizationPicture)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(3).UseAllAvailableWidth();

        var rightLogo = CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.RIGHT, doc, 180);

        string orgPicture = Path.GetFileName(OrganizationPicture);
        string imagePath = Path.Combine(
            PathHelper.WwwRoot,
            PathHelper.OrganizationFolder,
            orgPicture);

        Image leftLogo;
        if (File.Exists(imagePath))
        {
            leftLogo = CreateImage(imagePath, HorizontalAlignment.LEFT, doc, 180);
        }
        else
        {
            leftLogo = CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.LEFT, doc, 180);

        }
        table.AddCell(new Cell().Add(leftLogo)
            .SetBorder(Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetHorizontalAlignment(HorizontalAlignment.LEFT));

        table.AddCell(new Cell().Add(rightLogo)
            .SetBorder(Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetHorizontalAlignment(HorizontalAlignment.RIGHT));

        doc.Add(table);
    }

    public Stream Build(DTO data, string PageSize)
    {
        var stream = new MemoryStream();

        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, GetPageSize(PageSize));
        try
        {
            InitializeFont();
            document.SetFont(Font);
            pdf.GetDocumentInfo().SetTitle("Invoice");
            pdf.GetCatalog().SetViewerPreferences(
                new PdfViewerPreferences().SetDisplayDocTitle(true)
            );
            BuildLayout(document, data);
            document.Close();
        }
        finally
        {
            pdf.Close();
            writer.Close();
        }
        return stream;
    }

    protected virtual PageSize GetPageSize(string Size)
    {
        switch (Size.ToLower())
        {
            case "a4": return PageSize.A4;
            case "a5": return PageSize.A5;
            case "thermal7cm": return new PageSize(7 * 28.346f, 100 * 28.346f);
            case "receipt8cm": return new PageSize(8 * 28.346f, 100 * 28.346f);
            default: return PageSize.A4;
        }
    }

    public Stream Buildrdlc(PackagePdfData dt)
    {
        // Path to your RDLC file
        string reportPath = Path.Combine(PathHelper.WwwRoot, "Reportrdlc", "PackagePDF.rdlc");

        // Create LocalReport instance
        LocalReport report = new LocalReport();
        report.LoadReportDefinition(File.OpenRead(reportPath));

        // Add data source
        report.DataSources.Add(new ReportDataSource("DataSet1", CreateDataTable(dt)));

        // Render the report to PDF
        byte[] pdf = report.Render("PDF");

        var stream = new MemoryStream(pdf);
        stream.Position = 0;

        return stream;
    }

    private System.Data.DataTable CreateDataTable(PackagePdfData dt)
    {
        byte[] ImgMadkhalBytes = File.ReadAllBytes(PathHelper.imageDirectoryPdf);
        var ImgOrg = Path.Combine(
            PathHelper.WwwRoot,
            PathHelper.OrganizationFolder,
            Path.GetFileName(dt.Organization.OrganizationPicture));

        byte[] ImgOrgBytes = File.ReadAllBytes(ImgOrg);

        var QRCodePath = Path.Combine(
              PathHelper.WwwRoot,
              "QRCodeImage\\image",
              Path.GetFileName(dt.Localization.QRCodePath));

        byte[] ImgQrBytes = File.ReadAllBytes(QRCodePath);

        byte[] curr = File.ReadAllBytes(PathHelper.imageDirectoryPdf);

        var Days = (dt.Data.ExpiryDate - dt.Data.ActivationDateOfPackage!.Value).Days;

        // Example dataset
        System.Data.DataTable table = new System.Data.DataTable("InvoicerdlcData");
        table.Columns.Add("ImgMadkhal", typeof(byte[]));
        table.Columns.Add("ImgOrg", typeof(byte[]));
        table.Columns.Add("ImgQr", typeof(byte[]));
        table.Columns.Add("Title1", typeof(string));
        table.Columns.Add("SimplifiedTaxInvoice", typeof(string));
        table.Columns.Add("PaymentNumber", typeof(string));
        table.Columns.Add("OrganizationName", typeof(string));
        table.Columns.Add("CashierName", typeof(string));
        table.Columns.Add("PlateNumber", typeof(string));
        table.Columns.Add("OrganizationAddress", typeof(string));
        table.Columns.Add("Subscription", typeof(string));
        table.Columns.Add("Duration", typeof(string));
        table.Columns.Add("TotalPreVAT", typeof(string));
        table.Columns.Add("Vats", typeof(string));
        table.Columns.Add("Discount", typeof(string));
        table.Columns.Add("TotalIncludeVAT", typeof(string));
        table.Columns.Add("ExpiryDate", typeof(string));
        table.Columns.Add("ActivationDateOfPackage", typeof(string));

        var row = table.NewRow();
        row["ImgMadkhal"] = ImgMadkhalBytes;
        row["ImgOrg"] = ImgOrgBytes;
        row["ImgQr"] = ImgQrBytes;
        row["Title1"] = dt.Localization.PackageNumberView;
        row["SimplifiedTaxInvoice"] = "***" + dt.Localization.SimplifiedTaxInvoice + "***";
        row["PaymentNumber"] = $"{dt.Payment.PaymentNumber}:{dt.Localization.PaymentNumber}";
        row["OrganizationName"] = dt.Organization.ArabicName;
        row["CashierName"] = dt.Payment.CashierName;
        row["PlateNumber"] = dt.Data.EnPlateNumber;
        row["OrganizationAddress"] = dt.Organization.Address;
        row["Subscription"] = dt.Localization.PackageNumberView;
        row["Duration"] = $"{Days}{dt.Localization.Days}";
        row["TotalPreVAT"] = dt.Data.PriceOfPackage!.Value.ToString("F2");
        row["Vats"] = dt.Data.Vat.ToString("F2");
        row["Discount"] = dt.Data.Discount.ToString("F2");
        row["TotalIncludeVAT"] = dt.Payment.Amount.ToString("F2");
        row["ExpiryDate"] = dt.Data.ExpiryDate;
        row["ActivationDateOfPackage"] = dt.Data.ActivationDateOfPackage!.Value;
        table.Rows.Add(row);
        return table;
    }

    public Stream Buildrdlc(InvoicePdfData dt)
    {
        // Path to your RDLC file
        string reportPath = Path.Combine(PathHelper.WwwRoot, "Reportrdlc", "InvoicePDF.rdlc");

        // Create LocalReport instance
        LocalReport report = new LocalReport();
        report.LoadReportDefinition(File.OpenRead(reportPath));

        // Add data source
        report.DataSources.Add(new ReportDataSource("DataSet1", CreateDataTable(dt)));

        // Render the report to PDF
        byte[] pdf = report.Render("PDF");

        var stream = new MemoryStream(pdf);
        stream.Position = 0;

        return stream;
    }

    private System.Data.DataTable CreateDataTable(InvoicePdfData dt)
    {
        byte[] ImgMadkhalBytes = File.ReadAllBytes(PathHelper.imageDirectoryPdf);
        var ImgOrg = Path.Combine(
            PathHelper.WwwRoot,
            PathHelper.OrganizationFolder,
            Path.GetFileName(dt.Organization.OrganizationPicture));

        byte[] ImgOrgBytes = File.ReadAllBytes(ImgOrg);

        var QRCodePath = Path.Combine(
              PathHelper.WwwRoot,
              "QRCodeImage\\image",
              Path.GetFileName(dt.Localization.QRCodePath));

        byte[] ImgQrBytes = File.ReadAllBytes(QRCodePath);

        byte[] curr = File.ReadAllBytes(PathHelper.imageDirectoryPdf);

        // Example dataset
        System.Data.DataTable table = new System.Data.DataTable("InvoicerdlcData");
        table.Columns.Add("ImgMadkhal", typeof(byte[]));
        table.Columns.Add("ImgOrg", typeof(byte[]));
        table.Columns.Add("ImgQr", typeof(byte[]));
        table.Columns.Add("Title1", typeof(string));
        table.Columns.Add("SimplifiedTaxInvoice", typeof(string));
        table.Columns.Add("PaymentNumber", typeof(string));
        table.Columns.Add("OrganizationName", typeof(string));
        table.Columns.Add("CashierName", typeof(string));
        table.Columns.Add("PlateNumber", typeof(string));
        table.Columns.Add("OrganizationAddress", typeof(string));
        table.Columns.Add("Subscription", typeof(string));
        table.Columns.Add("Duration", typeof(string));
        table.Columns.Add("TotalPreVAT", typeof(string));
        table.Columns.Add("Vats", typeof(string));
        table.Columns.Add("Discount", typeof(string));
        table.Columns.Add("TotalIncludeVAT", typeof(string));
        table.Columns.Add("ExitTime", typeof(string));
        table.Columns.Add("ActivationDateOfPackage", typeof(string));

        var row = table.NewRow();
        row["ImgMadkhal"] = ImgMadkhalBytes;
        row["ImgOrg"] = ImgOrgBytes;
        row["ImgQr"] = ImgQrBytes;
        row["Title1"] = $"{dt.Data.InvoiceId.ToString()} : {dt.Localization.InvoiceNumber}";
        row["SimplifiedTaxInvoice"] = "***" + dt.Localization.SimplifiedTaxInvoice + "***";
        row["PaymentNumber"] = $"{dt.Payment.PaymentNumber} : {dt.Localization.InvoiceNumber}";
        row["OrganizationName"] = dt.Organization.ArabicName;
        row["CashierName"] = dt.Payment.CashierName;
        row["PlateNumber"] = dt.Data.PlateNumber;
        row["OrganizationAddress"] = dt.Organization.Address;
        row["Subscription"] = dt.Localization.Hours;
        row["Duration"] = $"{dt.Data.TotalHours}{dt.Localization.Hours}";
        row["TotalPreVAT"] = dt.Data.TotalPriceWithVatAndDiscount.ToString("F2");
        row["Vats"] = Math.Round(dt.Data.InvoicePrice * 0.15m, 2).ToString("F2");
        row["Discount"] = dt.Data.Discount.ToString("F2");
        row["TotalIncludeVAT"] = dt.Payment.Amount.ToString("F2");
        row["ExitTime"] = dt.Data.InvoiceTime;
        row["ActivationDateOfPackage"] = dt.Data.EntranceTime;
        table.Rows.Add(row);
        return table;
    }

    /// <summary>
    /// Template method: defines the overall PDF construction algorithm.
    /// </summary>
    protected virtual void BuildLayout(Document doc, DTO data)
    {
        if (CultureName == "ar")
            BuildArabicLayout(doc, data);
        else
            BuildEnglishLayout(doc, data);
    }

    /// <summary>
    /// Each derived builder implements its own Arabic layout.
    /// </summary>
    protected abstract void BuildArabicLayout(Document doc, DTO data);

    /// <summary>
    /// Each derived builder implements its own English layout.
    /// </summary>
    protected abstract void BuildEnglishLayout(Document doc, DTO data);


    protected virtual void InitializeFont()
    {
        string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), PathHelper.PDFFont);
        Font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
    }

    // -----------------------------
    // SCALING SUPPORT
    // -----------------------------
    protected float ScaleFactor(Document doc, float referenceWidth = 595f)
    {
        float currentWidth = doc.GetPdfDocument().GetDefaultPageSize().GetWidth()
                             - doc.GetLeftMargin() - doc.GetRightMargin();
        return currentWidth / referenceWidth;
    }
    protected void AddGrayBoxRow(
     Table table,
     string rightText,
     string leftText,
     PdfFont font,
     bool isArabic,
     Document? doc = null)
    {
        float scale = doc != null ? ScaleFactor(doc) : 1f;
        float fontSize = (float)Math.Round(14 * scale);
        if (fontSize < 5)
            fontSize = 5;
        if (isArabic)
        {
            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(rightText).SetFontSize(fontSize)));

            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(leftText).SetFontSize(fontSize)));
        }
        else
        {
            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(leftText).SetFontSize(fontSize)));

            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(rightText).SetFontSize(fontSize)));
        }
    }

    protected void AddSummaryRow(Table table, string label, string value, PdfFont font, bool isArabic, Image currency, Document doc, int colspan = 1)
    {
        var scale = ScaleFactor(doc);
        var fontSize = (int)Math.Round(12 * scale);
        if (fontSize < 4)
            fontSize = 4;
        var paragraph = new Paragraph().SetFont(font).SetFontSize(fontSize);
        if (isArabic)
            paragraph.Add(currency).Add($" {value} ").Add(new Text($": {label.ArabicWithFontGlyphsToPDF()}"));
        else
            paragraph.Add(new Text($"{label}: ")).Add(new Text($"{value} ")).Add(currency);

        table.AddCell(new Cell(1, colspan)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(isArabic ? TextAlignment.RIGHT : TextAlignment.LEFT)
            .Add(paragraph));
    }



    protected Image CreateImage(string path, HorizontalAlignment alignment, Document doc, float baseWidth = 120f)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Image path cannot be null or empty.", nameof(path));

        float scale = ScaleFactor(doc);

        try
        {
            var imageData = ImageDataFactory.Create(path);
            return new iText.Layout.Element.Image(imageData)
                .SetHorizontalAlignment(alignment)
                .SetWidth(baseWidth * scale);
        }
        catch (Exception ex)
        {
            throw new IOException($"❌ Failed to load image: {path}", ex);
        }
    }

    protected Image CreateImage(string path, HorizontalAlignment alignment, float baseWidth = 120f)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Image path cannot be null or empty.", nameof(path));

        try
        {
            var imageData = ImageDataFactory.Create(path);
            return new iText.Layout.Element.Image(imageData)
                .SetHorizontalAlignment(alignment)
                .SetWidth(baseWidth);
        }
        catch (Exception ex)
        {
            throw new IOException($"❌ Failed to load image: {path}", ex);
        }
    }


    protected Paragraph CreateCenteredParagraph(string text, int fontSize, bool bold = false, Document? doc = null)
    {
        var scale = doc != null ? ScaleFactor(doc) : 1f;
        fontSize = (int)Math.Round(fontSize * scale);
        if (fontSize < 6)
            fontSize = 6;
        var p = new Paragraph(text)
            .SetFont(Font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.CENTER);

        return p;
    }
    #region Arabic Layout Helpers
    protected void AddArabicCell(Table table, string text, PdfFont font, int fontSize = 9, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
            .SetPadding(0)
            .SetMargin(0)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(text)));
    }

    protected void AddArabicLabelCell(Table table, string label, PdfFont font, int fontSize = 9, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
             .SetPadding(0)
            .SetMargin(0)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(label).SimulateBold()));
    }

    protected void AddArabicEmptyCell(Table table, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(" ")));
    }
    #endregion


    #region English Layout Helpers
    protected void AddEnglishCell(Table table, string text, PdfFont font, int fontSize = 9, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .Add(new Paragraph(text)));
    }

    protected void AddEnglishLabelCell(Table table, string label, PdfFont font, int fontSize = 9, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .Add(new Paragraph(label).SimulateBold()));
    }

    protected void AddEnglishEmptyCell(Table table, int rowSpan = 1, int colSpan = 1)
    {
        table.AddCell(new Cell(rowSpan, colSpan)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(" ")));
    }

    #endregion


}