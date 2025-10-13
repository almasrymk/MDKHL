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
using iText.Layout.Properties;
using Microsoft.Extensions.Options;
using QRCoder;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.SvgQRCode;
using Path = System.IO.Path;

namespace SmartParking.PDF;

public abstract class BasePDFGeneratorAppService<DTO> 
{
    private readonly PdfSettings _pdfSettings;
    protected PdfFont Font { get; private set; }
    protected CultureInfo Culture => CultureInfo.CurrentCulture;
    protected string CultureName => CultureInfo.CurrentCulture.Name;
    protected BasePDFGeneratorAppService(IOptions<PdfSettings> pdfSettings)
    {
        _pdfSettings = pdfSettings.Value;
    }

    public virtual void ImagesSection(Document doc, string OrganizationPicture)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(3).UseAllAvailableWidth();

        var rightLogo = CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.RIGHT, doc, 120);

        string orgPicture = Path.GetFileName(OrganizationPicture);
        string imagePath = Path.Combine(
            PathHelper.WwwRoot,
            PathHelper.OrganizationFolder,
            orgPicture);

        Image leftLogo;
        if (File.Exists(imagePath))
        {
            leftLogo = CreateImage(imagePath, HorizontalAlignment.LEFT, doc, 120 * scale);
        }
        else
        {
            leftLogo = CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.LEFT, doc, 120 * scale);

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
        float fontSize = 14 * scale;

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
        var paragraph = new Paragraph().SetFont(font).SetFontSize(12 * scale);
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
    protected Paragraph CreateCenteredParagraph(string text, int fontSize, bool bold = false, Document? doc = null)
    {
        var scale = doc != null ? ScaleFactor(doc) : 1f;

        var p = new Paragraph(text)
            .SetFont(Font)
            .SetFontSize(fontSize * scale)
            .SetTextAlignment(TextAlignment.CENTER);

        return p;
    }
    #region Arabic Layout Helpers
    protected void AddArabicCell(Table table, string text, PdfFont font, int fontSize = 9)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(text)));
    }

    protected void AddArabicLabelCell(Table table, string label, PdfFont font, int fontSize = 9)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(label).SimulateBold()));
    }

    protected void AddArabicEmptyCell(Table table)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(" ")));
    }
    #endregion


    #region English Layout Helpers
    protected void AddEnglishCell(Table table, string text, PdfFont font, int fontSize = 9)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .Add(new Paragraph(text)));
    }

    protected void AddEnglishLabelCell(Table table, string label, PdfFont font , int fontSize = 9)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .Add(new Paragraph(label).SimulateBold()));
    }

    protected void AddEnglishEmptyCell(Table table)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(" ")));
    }

    #endregion


}
