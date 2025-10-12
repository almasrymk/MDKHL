using AraibcPdfUnicodeGlyphsResharper;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
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

namespace SmartParking.PDF;

public abstract class BasePDFGeneratorAppService<DTO> 
{
    protected PdfFont Font { get; private set; }
    protected CultureInfo Culture => CultureInfo.CurrentCulture;
    protected string CultureName => CultureInfo.CurrentCulture.Name;

    public Stream Build(DTO data)
    {
        var stream = new MemoryStream();

        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);
        try
        {
            InitializeFont();
            BuildLayout(document, data);
            document.Close(); // This flushes but doesn't dispose the MemoryStream
        }
        finally
        {
            pdf.Close();
            writer.Close();
        }
        return stream;
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
        string fontPath = Path.Combine(Directory.GetCurrentDirectory(), PathHelper.PDFFont);
        Font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
    }
    protected void AddGrayBoxRow(
      Table table,
      string rightText,
      string leftText,
      PdfFont font,
      bool isArabic)
    {
        if (isArabic)
        {
            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(rightText).SetFontSize(14)));

            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(leftText).SetFontSize(14)));
        }
        else
        {
            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(leftText).SetFontSize(14)));

            table.AddCell(new Cell(1, 1)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(font)
                .Add(new Paragraph(rightText).SetFontSize(14)));
        }
    }


    protected void AddSummaryRow(Table table, string label, string value, PdfFont font, bool isArabic, Image currency, int colspan = 1)
    {
        var paragraph = new Paragraph().SetFont(font).SetFontSize(12);
        if (isArabic)
            paragraph.Add(currency).Add($" {value} ").Add(new Text($": {label.ArabicWithFontGlyphsToPDF()}"));
        else
            paragraph.Add(new Text($"{label}: ")).Add(new Text($"{value} ")).Add(currency);

        table.AddCell(new Cell(1, colspan)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(isArabic ? TextAlignment.RIGHT : TextAlignment.LEFT)
            .Add(paragraph));
    }

    protected Image CreateImage(string path, HorizontalAlignment alignment, float width = 120)
    {
   
         return  new iText.Layout.Element.Image(ImageDataFactory.Create(path))
               .SetHorizontalAlignment(alignment)
               .SetWidth(width);
    }

    protected Paragraph CreateCenteredParagraph(string text, int fontSize, bool bold = false)
    {
        var p = new Paragraph(text)
            .SetFont(Font)
            .SetFontSize(fontSize)
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

    protected void AddArabicLabelCell(Table table, string label, PdfFont font)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(9)
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

    protected void AddEnglishLabelCell(Table table, string label, PdfFont font)
    {
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetFont(font)
            .SetFontSize(9)
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
