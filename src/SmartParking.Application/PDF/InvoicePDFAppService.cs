using AraibcPdfUnicodeGlyphsResharper;
using Humanizer;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class InvoicePDFAppService : BasePDFGeneratorAppService<InvoicePdfData>, IPDFContractsAppService<InvoicePdfData> , IPDFGeneratorAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly IOrganizationsAppService _organizationAppService;
    private readonly ILocalizationFactoryAppService _localizationAppService;


    public InvoicePDFAppService(IInvoiceAppService invoiceAppService, IPaymentWebAppService paymentWebAppService, IOrganizationsAppService organizationAppService, ILocalizationFactoryAppService localizationAppService)
    {
        _invoiceAppService = invoiceAppService;
        _paymentWebAppService = paymentWebAppService;
        _organizationAppService = organizationAppService;
        _localizationAppService = localizationAppService;
    }
    public void ImagesSection(Document doc ,InvoicePdfData localizationData)
    {
        Table tableimages = new Table(3).UseAllAvailableWidth();

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
           .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(localizationData.Localization.CurrentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.Localization.CurrentPath))
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

            iText.Layout.Element.Image OrgLogo = (File.Exists(localizationData.Localization.CurrentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(localizationData.Localization.CurrentPath))
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

        doc.Add( tableimages);
    }

    public void AddHeaderSection(Document doc, InvoicePdfData data)
    {
        var header = CreateCenteredParagraph(data.Localization.InvoiceNumber.ToString().ArabicWithFontGlyphsToPDF(), 20, true);
        doc.Add(header);
    }

    public void AddOrganizationSection(Document doc, InvoicePdfData data, bool isArabic)
    {
        var table = new Table(5).UseAllAvailableWidth().SetWidth(UnitValue.CreatePercentValue(100));
        var invoice = data.Data;
        var loc = data.Localization;
        var payment = data.Payment;

        if (isArabic)
        {
            AddRow(table, invoice.OrganizationArabicName.ArabicWithFontGlyphsToPDF(), $":{loc.OrganizationName.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
            AddRow(table, payment.CashierName.ArabicWithFontGlyphsToPDF(), $":{loc.CashierNo.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
            AddRow(table, invoice.PlateNumber, $":{loc.PlateNumber.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
            AddRowWithSpan(table, invoice.OrganizationAddress.ArabicWithFontGlyphsToPDF(), $":{loc.OrganizationAddress.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
            AddSpacer(table, 10);
            AddRow(table, invoice.InvoiceTime.ToString(), $":{loc.ExitTime.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
            AddRow(table, invoice.EntranceTime.ToString(), $":{loc.EntranceTime.ArabicWithFontGlyphsToPDF()}", Font, TextAlignment.RIGHT);
        }
        else
        {
            AddRow(table, $"{loc.CashierNo}:", payment.CashierName, Font, TextAlignment.LEFT);
            AddRow(table, $"{loc.OrganizationName}:", invoice.OrganizationEnglishName, Font, TextAlignment.LEFT);
            AddRowWithSpan(table, invoice.OrganizationAddress, $"{loc.OrganizationAddress}:", Font, TextAlignment.LEFT);
            AddSpacer(table, 10);
            AddRow(table, $"{loc.EntranceTime}:", invoice.EntranceTime.ToString(), Font, TextAlignment.LEFT);
            AddRow(table, $"{loc.ExitTime}:", invoice.InvoiceTime.ToString(), Font, TextAlignment.LEFT);
        }

        doc.Add(table);
    }


    public void AddPaymentHeader(Document doc, InvoicePdfData data)
    {
        var currencyImg = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, 10);
        var p = new Paragraph().SetFont(Font).SetFontSize(13).SetTextAlignment(TextAlignment.CENTER);

        if (CultureName == "ar")
            p.Add(currencyImg).Add(" ").Add(new Text(data.Payment.Amount.ToString()))
             .Add(" ").Add(new Text(data.Localization.HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF()));
        else
            p.Add(new Text(data.Localization.HeaderVatReceiptPayment))
             .Add(" ").Add(new Text(data.Payment.Amount.ToString()))
             .Add(" ").Add(currencyImg);

        doc.Add(p);
    }

    public void AddQrAndTotalsSection(Document doc, InvoicePdfData data, bool isArabic)
    {
        var container = new Table(2).UseAllAvailableWidth().SetMarginTop(20).SetBorder(Border.NO_BORDER);
        var qrTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var qr = CreateImage(data.Localization.QRCodePath, isArabic ? HorizontalAlignment.LEFT : HorizontalAlignment.RIGHT, 120);

        qrTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(qr));

        var totals = BuildTotalsTable(data, isArabic);

        if (isArabic)
        {
            container.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(qrTable));
            container.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(totals));
        }
        else
        {
            container.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(totals));
            container.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(qrTable));
        }

        doc.Add(container);
    }


    public void AddSubHeader(Document doc, InvoicePdfData data)
    {
        var text = CultureName == "ar"
            ? $"***{data.Localization.SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{data.Localization.SimplifiedTaxInvoice}***";
        doc.Add(CreateCenteredParagraph(text, 13, true));
    }

    public void AddSummarySection(Document doc, InvoicePdfData data, bool isArabic)
    {
        var table = new Table(2).UseAllAvailableWidth().SetMarginTop(20);

        if (isArabic)
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Hours.ArabicWithFontGlyphsToPDF()} {data.Data.TotalHours}",
                $"{data.Localization.Subscription.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Hour.ArabicWithFontGlyphsToPDF()}",
                Font,
                true,
                data.Localization.Subscription.ArabicWithFontGlyphsToPDF(),
                data.Localization.Hour.ArabicWithFontGlyphsToPDF());
        }
        else
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Subscription}\n{data.Localization.Hour}",
                $"{data.Localization.Duration}\n{data.Data.TotalHours} {data.Localization.Hours}",
                Font,
                false,
                data.Localization.Subscription,
                data.Localization.Hour);
        }

        doc.Add(table);
    }


    public void AddVatAndTicket(Document doc, InvoicePdfData data)
    {
        var vatText = CultureName == "ar"
               ? $"{data.Organization.TaxNumber.ArabicWithFontGlyphsToPDF()}:{data.Localization.InvoiceNumberView.ArabicWithFontGlyphsToPDF()}"
               : $"{data.Localization.InvoiceNumberView}: {data.Organization.TaxNumber}";
        doc.Add(CreateCenteredParagraph(vatText, 13, true));

        if (data.Data.TicketId != null)
        {
            var tText = CultureName == "ar"
                ? $"{data.Data.TicketId.ArabicWithFontGlyphsToPDF()}:{data.Localization.TicketId.ArabicWithFontGlyphsToPDF()}"
                : $"{data.Localization.TicketId}: {data.Data.TicketId}";
            doc.Add(CreateCenteredParagraph(tText, 13, true));
        }
    }

    public Table BuildTotalsTable(InvoicePdfData data, bool isArabic)
    {
        var table = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var currency = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, 10);
        var inv = data.Data;
        var loc = data.Localization;

        AddSummaryRow(table, loc.TotalPreVAT, inv.InvoicePrice.ToString("F2"), Font, isArabic, currency);
        var vat = Math.Round(inv.InvoicePrice * 0.15m, 2).ToString("F2");
        AddSummaryRow(table, $"15% {loc.Vats}", vat, Font, isArabic, currency);
        AddSummaryRow(table, loc.TotalIncludeVAT, loc.Price.ToString("F2"), Font, isArabic, currency);

        if (inv.InvoicePrice != data.Payment.Amount)
            AddSummaryRow(table, $"{loc.After} {loc.Discount}", loc.PaymentPrice.ToString("F2"), Font, isArabic, currency);

        return table;
    }

    public async Task<Stream> CreatePdf(int EntityId)
    {
        // Switch to Arabic for localization
        var arabicCulture = new CultureInfo("ar");
        CultureInfo.CurrentCulture = arabicCulture;
        CultureInfo.CurrentUICulture = arabicCulture;

        // Fetch domain data
        var invoice = await _invoiceAppService.GetAsync(EntityId);
        var image = await _invoiceAppService.GetInvoiceQRCodeForOrganization(EntityId);
        var payment = await _paymentWebAppService.GetPaymentForInvoice(EntityId);
        var organization = await _organizationAppService.GetAsync(payment.OrganizationId);

        // Generate localized text resources
        var localization = _localizationAppService.Create(new InvoiceLocalizationCreateDto
        {
            OrganizationPicture = organization.OrganizationPicture,
            ImagePath = image,
            InvoicePrice = invoice.InvoicePrice,
            PaymentAmount = payment.Amount
        });

        var pdfData = new InvoicePdfData
        {
            Data = invoice,
            Payment = payment,
            Organization = organization,
            Localization = localization
        };

        var pdfStream = Build(pdfData);
        return pdfStream;
    }

    protected override void BuildArabicLayout(Document doc, InvoicePdfData data)
    {
        ImagesSection(doc, data);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddPaymentHeader(doc, data);
        AddOrganizationSection(doc, data, true);
        AddSummarySection(doc, data, true);
        AddQrAndTotalsSection(doc, data, true);
    }

    protected override void BuildEnglishLayout(Document doc, InvoicePdfData data)
    {
        ImagesSection(doc, data);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddPaymentHeader(doc, data);
        AddOrganizationSection(doc, data, false);
        AddSummarySection(doc, data, false);
        AddQrAndTotalsSection(doc, data, false);
    }
}
