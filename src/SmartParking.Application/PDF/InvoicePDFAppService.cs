using AraibcPdfUnicodeGlyphsResharper;
using Humanizer;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Options;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Utilities;
using SmartParking.ZATCATaxInvoice;
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


    public InvoicePDFAppService(IOptions<PdfSettings> pdfSettings,IInvoiceAppService invoiceAppService, IPaymentWebAppService paymentWebAppService, IOrganizationsAppService organizationAppService, ILocalizationFactoryAppService localizationAppService) : base(pdfSettings)
    {
        _invoiceAppService = invoiceAppService;
        _paymentWebAppService = paymentWebAppService;
        _organizationAppService = organizationAppService;
        _localizationAppService = localizationAppService;
    }
    // -----------------------------
    // IMAGES HEADER SECTION
    // -----------------------------
  
    public void AddHeaderSection(Document doc, InvoicePdfData data)
    {
        doc.Add(CreateCenteredParagraph($"{data.Localization.InvoiceNumber.ArabicWithFontGlyphsToPDF()}", 20, true,doc));
    }

    public void AddOrganizationSection(Document doc, InvoicePdfData data, bool isArabic)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 1, 2, 2 }))
            .UseAllAvailableWidth()
            .SetBorder(Border.NO_BORDER);
        var fontsize = (int)(10 * scale);

        var inv = data.Data;
        var loc = data.Localization;
        var pay = data.Payment;

        if (isArabic)
        {
            AddArabicCell(table, pay.CashierName.ArabicWithFontGlyphsToPDF(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.CashierNo.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, inv.OrganizationArabicName.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.OrganizationName.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            AddArabicCell(table, inv.PlateNumber, Font,fontsize);
            AddArabicLabelCell(table, $":{loc.PlateNumber.ArabicWithFontGlyphsToPDF()}", Font,fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, inv.OrganizationAddress.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.OrganizationAddress.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            for (int i = 0; i < 5; i++) AddArabicEmptyCell(table);
            AddArabicCell(table, inv.InvoiceTime.ToString(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.ExitTime.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, inv.EntranceTime.ToString(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.EntranceTime.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
          
        }
        else
        {
            AddEnglishLabelCell(table, $"{loc.CashierNo}:", Font, fontsize);
            AddEnglishCell(table, pay.CashierName, Font,fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationName}:", Font, fontsize);
            AddEnglishCell(table, inv.OrganizationEnglishName, Font,fontsize);

            AddEnglishLabelCell(table, $"{loc.PlateNumber}:", Font, fontsize);
            AddEnglishCell(table, inv.PlateNumber, Font,fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationAddress}:", Font, fontsize);
            AddEnglishCell(table, inv.OrganizationAddress, Font,fontsize);

            for (int i = 0; i < 5; i++) AddEnglishEmptyCell(table);

            AddEnglishLabelCell(table, $"{loc.ExitTime}:", Font, fontsize);
            AddEnglishCell(table, inv.InvoiceTime.ToString(), Font,fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.EntranceTime}:", Font, fontsize);
            AddEnglishCell(table, inv.EntranceTime.ToString(), Font,fontsize);
        }

        doc.Add(table);
    }
    public void AddPaymentHeader(Document doc, InvoicePdfData data)
    {
        var currencyImg = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT,doc ,10);
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
        var container = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc)).SetBorder(Border.NO_BORDER);
        var qr = CreateImage(data.Localization.QRCodePath, isArabic ? HorizontalAlignment.LEFT : HorizontalAlignment.RIGHT, doc, 120);
        var qrTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER)
            .AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(qr));

        var totals = BuildTotalsTable(data, isArabic, doc);

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
        doc.Add(CreateCenteredParagraph(text, 13, true,doc));
    }

    public void AddSummarySection(Document doc, InvoicePdfData data, bool isArabic)
    {
        var table = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc));
        if (isArabic)
        {
            AddGrayBoxRow(table,
                $"{data.Localization.Duration.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Hours.ArabicWithFontGlyphsToPDF()} {data.Data.TotalHours}",
                $"{data.Localization.Subscription.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Hour.ArabicWithFontGlyphsToPDF()}",
                Font, true, doc);
        }
        else
        {
            AddGrayBoxRow(table,
                $"{data.Localization.Duration}\n{data.Localization.Hours} {data.Data.TotalHours}",
                $"{data.Localization.Subscription}\n{data.Localization.Hour}",
                Font, false, doc);
        }

        doc.Add(table);
    }

    public void AddVatAndTicket(Document doc, InvoicePdfData data)
    {
        var vatText = CultureName == "ar"
               ? $"{data.Payment.PaymentNumber.ArabicWithFontGlyphsToPDF()}:{data.Localization.InvoiceNumberView.ArabicWithFontGlyphsToPDF()}"
               : $"{data.Localization.InvoiceNumberView}: {data.Payment.PaymentNumber}";
        doc.Add(CreateCenteredParagraph(vatText, 13, true));

        if (data.Data.TicketId != null)
        {
            var tText = CultureName == "ar"
                ? $"{data.Data.TicketId.ArabicWithFontGlyphsToPDF()}:{data.Localization.TicketId.ArabicWithFontGlyphsToPDF()}"
                : $"{data.Localization.TicketId}: {data.Data.TicketId}";
            doc.Add(CreateCenteredParagraph(tText, 13, true));
        }
    }

    public Table BuildTotalsTable(InvoicePdfData data, bool isArabic, Document doc)
    {
        var table = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var currency = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, doc, 10);
        var inv = data.Data;
        var loc = data.Localization;

        AddSummaryRow(table, loc.TotalPreVAT, inv.InvoicePrice.ToString("F2"), Font, isArabic, currency, doc);
        var vat = Math.Round(inv.InvoicePrice * 0.15m, 2).ToString("F2");
        AddSummaryRow(table, $"{loc.Vats}", vat, Font, isArabic, currency, doc);
        if (inv.InvoicePrice != data.Payment.Amount)
            AddSummaryRow(table, $"{loc.Discount}", loc.PaymentPrice.ToString("F2"), Font, isArabic, currency, doc);
        AddSummaryRow(table, loc.TotalIncludeVAT, loc.Price.ToString("F2"), Font, isArabic, currency, doc);

        return table;
    }

    // -----------------------------
    // MAIN ENTRY POINT
    // -----------------------------
    public async Task<Stream> CreatePdf(int entityId, string PageSize)
    {
        var arabicCulture = new CultureInfo("ar");
        CultureInfo.CurrentCulture = arabicCulture;
        CultureInfo.CurrentUICulture = arabicCulture;

        var invoice = await _invoiceAppService.GetAsync(entityId);
        var image = await _invoiceAppService.GetInvoiceQRCodeForOrganization(entityId);
        var payment = await _paymentWebAppService.GetPaymentForInvoice(entityId);
        var org = await _organizationAppService.GetAsync(payment.OrganizationId);

        var localization = _localizationAppService.Create(new InvoiceLocalizationCreateDto
        {
            OrganizationPicture = org.OrganizationPicture,
            ImagePath = image,
            Price = invoice.InvoicePrice,
            PaymentAmount = payment.Amount
        });

        var pdfData = new InvoicePdfData
        {
            Data = invoice,
            Payment = payment,
            Organization = org,
            Localization = localization
        };

        return Build(pdfData, PageSize);
    }

    protected override void BuildArabicLayout(Document doc, InvoicePdfData data)
    {
        ImagesSection(doc, data.Organization.OrganizationPicture);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddOrganizationSection(doc, data, true);
        AddSummarySection(doc, data, true);
        AddQrAndTotalsSection(doc, data, true);
    }

    protected override void BuildEnglishLayout(Document doc, InvoicePdfData data)
    {
        ImagesSection(doc, data.Organization.OrganizationPicture);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddOrganizationSection(doc, data, false);
        AddSummarySection(doc, data, false);
        AddQrAndTotalsSection(doc, data, false);
    }
}
