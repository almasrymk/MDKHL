using AraibcPdfUnicodeGlyphsResharper;
using iText.Html2pdf;
using iText.Html2pdf.Css.Apply.Impl;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using RemoteServiceAttribute = Volo.Abp.RemoteServiceAttribute;

namespace SmartParking.PDF;

public class PDFGeneratorAppService : SmartParkingAppService, IPDFGeneratorAppService
{
    private readonly IInvoiceAppService _invoicePdfAppService;
    private readonly IOrganizationsAppService _organizationPdfAppService;
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly ILocalizationFactoryAppService _localizationAppService;
    public PDFGeneratorAppService(IInvoiceAppService invoicePdfAppService, IOrganizationsAppService organizationPdfAppService, IPaymentWebAppService paymentWebAppService, ILocalizationFactoryAppService localizationAppService)
    {
        _invoicePdfAppService = invoicePdfAppService;
        _organizationPdfAppService = organizationPdfAppService;
        _paymentWebAppService = paymentWebAppService;
        _localizationAppService = localizationAppService;
    }

    #region Public Methods


    /// <summary>
    /// Saves the generated PDF for an invoice to a file system or storage.
    /// </summary>
    /// <returns>A <see cref="Stream"/> representing the saved PDF file.</returns>
    [RemoteServiceAttribute(isEnabled: true)]
    public async Task<Stream> CreatePdf(int invoiceId)
    {
        var arabicCulture = new CultureInfo("ar");
        CultureInfo.CurrentCulture = arabicCulture;
        CultureInfo.CurrentUICulture = arabicCulture;

        var invoice = await _invoicePdfAppService.GetAsync(invoiceId);
        var image = await _invoicePdfAppService.GetInvoiceQRCodeForOrganization(invoiceId);
        var payment = await _paymentWebAppService.GetPaymentForInvoice(invoiceId);
        var organization = await _organizationPdfAppService.GetAsync(payment.OrganizationId);
        var InvoiceLocalizationDto = new InvoiceLocalizationCreateDto
        {
            OrganizationPicture = organization.OrganizationPicture,
            ImagePath = image,
            InvoicePrice = invoice.InvoicePrice,
            PaymentAmount = payment.Amount
        };
        var localizationData = _localizationAppService.Create(InvoiceLocalizationDto);
        MemoryStream stream = new MemoryStream();
        PdfWriter writer = new PdfWriter(stream);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);
        string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), PathHelper.PDFFont);
        PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        FontSet set = new FontSet();
        set.AddFont(fontPath);
        Table tableimages = ImagesSection(localizationData);

        Paragraph header = new Paragraph(invoice.InvoiceNumber.ToString())
         .SetTextAlignment(TextAlignment.CENTER)
         .SetFontSize(20);

        iText.Layout.Element.Image currency = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.CurrencyIcon))
      .SetHorizontalAlignment(HorizontalAlignment.LEFT)
      .SetWidth(10);
        Paragraph paymentNumber = PaymentNumberSection(payment, localizationData, font);

        Paragraph subheader = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
            ? $"***{localizationData.SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{localizationData.SimplifiedTaxInvoice}***")
    .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
    .SetFontSize(13);

        Paragraph Vat = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
            ? $"{organization.TaxNumber.ArabicWithFontGlyphsToPDF()}:{localizationData.InvoiceNumberView.ArabicWithFontGlyphsToPDF()}"
            : $"{localizationData.InvoiceNumberView}: {organization.TaxNumber}")
            .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
            .SetFontSize(13);

        Paragraph Ticket = new Paragraph();
        if (invoice?.TicketId != null)
        {
            Ticket = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
                                  ? $"{invoice?.TicketId.ArabicWithFontGlyphsToPDF()}:{localizationData.TicketId.ArabicWithFontGlyphsToPDF()}"
                                  : $"{localizationData.TicketId}: {invoice?.TicketId}")
                                  .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
                                  .SetFontSize(13);
        }


        Paragraph RecieptPayemntDetails = new Paragraph().SetFont(font).SetFontSize(13).SimulateBold().SetTextAlignment(TextAlignment.CENTER);
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            RecieptPayemntDetails
                 .Add(currency)
                 .Add(" ")
                 .Add(new Text(localizationData.PaymentPrice.ToString()))
                 .Add(" ")
                .Add(new Text(localizationData.HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF()));
        }
        else
        {
            RecieptPayemntDetails
                  .Add(new Text(localizationData.HeaderVatReceiptPayment))
                 .Add(" ")
                .Add(new Text(localizationData.PaymentPrice.ToString()))
                 .Add(" ")
                .Add(currency);
        }

        Table table = new Table(5).SetWidth(UnitValue.CreatePercentValue(100)).SetWidth(UnitValue.CreatePercentValue(100));
        Table table1 = new Table(3).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(20);

        Table table2 = new Table(2)
            .UseAllAvailableWidth()
            .SetMarginTop(20)
            .SetBorder(Border.NO_BORDER);

        Table table21 = new Table(1)
            .UseAllAvailableWidth()
             .SetMarginTop(20)
            .SetBorder(Border.NO_BORDER);

        Table table22 = new Table(1)
            .UseAllAvailableWidth()
            .SetBorder(Border.NO_BORDER);

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            table.AddCell(new Cell(1, 1)

                  .SetFont(font)
                  .SetFontSize(10)
                  .SetBorder(Border.NO_BORDER)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .Add(new Paragraph($"{invoice.OrganizationArabicName.ArabicWithFontGlyphsToPDF()}")));

            table.AddCell(new Cell(1, 1)
                .SetFont(font)
                 .SetFontSize(9)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($":{localizationData.OrganizationName.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(10)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{payment.CashierName.ArabicWithFontGlyphsToPDF()}")));
            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(9)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($":{localizationData.CashierNo.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(10)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.PlateNumber}")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetFontSize(9)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($":{localizationData.PlateNumber.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 2).SetFont(font)
                .SetFontSize(10)
               .SetBorder(Border.NO_BORDER)
               .SetTextAlignment(TextAlignment.RIGHT)
                .Add(new Paragraph($"{invoice.OrganizationAddress.ArabicWithFontGlyphsToPDF()}")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                                .SetFontSize(9)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($":{localizationData.OrganizationAddress.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                .SetFontSize(10)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.InvoiceTime.ToString()}")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(9)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($":{localizationData.ExitTime.ArabicWithFontGlyphsToPDF()}").SimulateBold()));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(10)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.EntranceTime.ToString()}")));
            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(9)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($":{localizationData.EntranceTime.ArabicWithFontGlyphsToPDF()}").SimulateBold()));



            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopLeftRadius(new BorderRadius(10))
            .SetBorderBottomLeftRadius(new BorderRadius(10))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetFont(font)
             .Add(new Paragraph($"{localizationData.Duration.ArabicWithFontGlyphsToPDF()}\n {localizationData.Hours.ArabicWithFontGlyphsToPDF()}{" "}{invoice.TotalHours}").SetFontSize(14)));


            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopRightRadius(new BorderRadius(10))
                .SetBorderBottomRightRadius(new BorderRadius(10))
                .SetFont(font)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph($"{localizationData.Subscription.ArabicWithFontGlyphsToPDF()}\n {localizationData.Hour.ArabicWithFontGlyphsToPDF()}").SetFontSize(14)));
            iText.Layout.Element.Image QrCodeImage = new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.QRCodePath))
           .SetHorizontalAlignment(HorizontalAlignment.LEFT)
           .SetWidth(120);

            table22.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(QrCodeImage));


            Paragraph paragraph = new Paragraph()
                .Add(currency)
                .Add(new Text($" {invoice.InvoicePrice.ToString("F2")}"))
                .Add(new Text($": {localizationData.TotalPreVAT.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                .SetFontSize(12);

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetFont(font)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph));

            string vatAmount = Math.Round((invoice.InvoicePrice * 0.15m), 2).ToString("F2");

            Paragraph paragraph2 = new Paragraph()
                .Add(currency)
                .Add(new Text($" {vatAmount} "))
                .Add(new Text($": 15% {localizationData.Vats.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                .SetFontSize(12);

            // Add to table cell
            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetFont(font)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph2));

            Paragraph paragraph3 = new Paragraph()
                 .Add(currency)
                 .Add(new Text($" {localizationData.Price.ToString("F2")}"))
                 .Add(new Text($": {localizationData.TotalIncludeVAT.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                 .SetFontSize(12);

            // Add to table
            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetFont(font)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph3));
            if (invoice.InvoicePrice != payment.Amount)
            {
                Paragraph paragraph4 = new Paragraph()
                  .Add(currency)
                  .Add(new Text($" {localizationData.PaymentPrice.ToString("F2")}"))
                  .Add(new Text($": {localizationData.Discount.ArabicWithFontGlyphsToPDF()} {localizationData.After.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                  .SetFontSize(12);

                table21.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .Add(paragraph4));

            }

            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table22));
            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table21));
        }

        else
        {
            table.AddCell(new Cell(1, 1)
                .SetFont(font)
                .SetFontSize(7)
                  .SetBorder(Border.NO_BORDER)
                  .SetTextAlignment(TextAlignment.LEFT)
                  .Add(new Paragraph($"{localizationData.CashierNo}:")).SimulateBold());

            table.AddCell(new Cell(1, 1)
                .SetFont(font)
                .SetFontSize(8)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.GarageEnglishName}")));

            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(7)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{localizationData.OrganizationName}:")).SimulateBold());

            table.AddCell(new Cell(1, 1)
                .SetFont(font)
                .SetFontSize(8)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($"{invoice.OrganizationEnglishName}")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(7)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{localizationData.OrganizationAddress}:")).SimulateBold());

            table.AddCell(new Cell(1, 2)
                .SetFont(font)
                .SetFontSize(8)
              .SetTextAlignment(TextAlignment.LEFT)
              .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph($"{invoice.OrganizationAddress.ArabicWithFontGlyphsToPDF()}")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(7)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{localizationData.PlateNumber}:")).SimulateBold());
            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(8)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.PlateNumber}")));

            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(7)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{localizationData.EntranceTime}:")).SimulateBold());
            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(8)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.EntranceTime.ToString()}")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(7)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{localizationData.ExitTime}:")).SimulateBold());
            table.AddCell(new Cell(1, 1).SetFont(font).SetFontSize(8)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.InvoiceTime.ToString()}")));

            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopLeftRadius(new BorderRadius(10))
                .SetBorderBottomLeftRadius(new BorderRadius(9))
                .SetFont(font)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph($"{localizationData.Subscription}\n {localizationData.Hour}").SetFontSize(14)));



            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopRightRadius(new BorderRadius(10))
            .SetBorderBottomRightRadius(new BorderRadius(10))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetFont(font)
             .Add(new Paragraph($"{localizationData.Duration}\n {invoice.TotalHours}{" "}{localizationData.Hours}").SetFontSize(14)));

            Paragraph paragraph = new Paragraph()
                .Add(new Text($"{localizationData.TotalPreVAT}: ").SimulateBold())
                .Add(new Text(invoice.InvoicePrice.ToString("F2") + " "))
                .Add(currency)
                .SetFontSize(12);

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph));

            Paragraph paragraph2 = new Paragraph()
               .Add(new Text($"{localizationData.Vats} 15%: ").SimulateBold())
               .Add(new Text((Math.Round((invoice.InvoicePrice * 0.15m), 2)).ToString("F2") + " "))
               .Add(currency)
               .SetFontSize(12);

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph2));

            Paragraph paragraph3 = new Paragraph()
                .Add(new Text($"{localizationData.TotalIncludeVAT}: ").SimulateBold())
                .Add(new Text(localizationData.Price.ToString("F2") + " "))
                .Add(currency)
                .SetFontSize(12);

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(paragraph3));
            if (invoice.InvoicePrice != payment.Amount)
            {
                table21.AddCell(new Cell()
              .SetBorder(Border.NO_BORDER)
              .SetFont(font)
              .SetTextAlignment(TextAlignment.LEFT)
              .SetVerticalAlignment(VerticalAlignment.TOP)
              .Add(new Paragraph()
                  .Add(new Text($"{localizationData.After.ArabicWithFontGlyphsToPDF() + " " + localizationData.Discount.ArabicWithFontGlyphsToPDF()}:").SimulateBold())
                  .Add(new Text($" {localizationData.PaymentPrice.ToString("F2")} {localizationData.Sar.ArabicWithFontGlyphsToPDF()}"))
                  .Add(currency)
                  .SetFontSize(12)));

            }
            iText.Layout.Element.Image QrCodeImage = new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.QRCodePath))
        .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
        .SetWidth(120);

            table22.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(QrCodeImage));

            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table21));
            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table22));

        }


        document.Add(header);
        document.Add(paymentNumber);
        document.Add(tableimages);
        document.Add(subheader);
        document.Add(Vat);
        if (invoice.TicketId != null)
        {
            document.Add(Ticket);
        }
        document.Add(RecieptPayemntDetails);
        document.Add(table);
        document.Add(table1);
        document.Add(table2);
        document.Close();
        return stream;
    }

    private static Paragraph PaymentNumberSection(PaymentDto payment, InvoiceLocalizationData localizationData, PdfFont font)
    {
        return new Paragraph(CultureInfo.CurrentCulture.Name == "ar" ? $"{payment.PaymentNumber}{" "}{localizationData.InvoiceNumber.ArabicWithFontGlyphsToPDF()}" : $"{localizationData.InvoiceNumber}{" "}{payment.PaymentNumber}")
              .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
              .SetFontSize(13);
    }

    private static Table ImagesSection(InvoiceLocalizationData localizationData)
    {
        Table tableimages = new Table(3).UseAllAvailableWidth();

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
           .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(localizationData.CurrentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.CurrentPath))
          : new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf)))
              .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT)
              .SetWidth(120);

            tableimages.AddCell(new Cell().Add(OrgLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));

            tableimages.AddCell(new Cell().Add(imageMdkhlLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT));


        }
        else
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.LEFT)
           .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(localizationData.CurrentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.CurrentPath))
          : new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf)))
              .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT)
              .SetWidth(120);

            tableimages.AddCell(new Cell().Add(imageMdkhlLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT));

            tableimages.AddCell(new Cell().Add(OrgLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));

        }

        return tableimages;
    }


  
    #endregion

}
