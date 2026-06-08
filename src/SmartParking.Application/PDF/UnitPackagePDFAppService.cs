using AraibcPdfUnicodeGlyphsResharper;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Options;
using SmartParking.CustomerUnitPackages;
using SmartParking.Payments;
using SmartParking.Payments.PaymentRelations;
using SmartParking.PDF.Contracts;
using SmartParking.PDF.Dtos;
using SmartParking.QRCode;
using SmartParking.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SmartParking.PDF;

public class UnitPackagePDFAppService : BasePDFGeneratorAppService<UnitPackagePdfData>,
    IPDFContractsAppService<UnitPackagePdfData>,
    IPDFGeneratorAppService
{
    private readonly ILocalizationFactoryAppService _localizationFactoryAppService;
    private readonly IQRCodeAppService _qrCodeAppService;
    private readonly IPaymentDetailsAppService _paymentDetailsAppService;
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly PdfSettings _pdfOpt;

    public UnitPackagePDFAppService(
        ILocalizationFactoryAppService localizationFactoryAppService,
        IQRCodeAppService qrCodeAppService,
        IPaymentDetailsAppService paymentDetailsAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IOptions<PdfSettings> pdfOptions) : base(pdfOptions)
    {
        _localizationFactoryAppService = localizationFactoryAppService;
        _qrCodeAppService = qrCodeAppService;
        _paymentDetailsAppService = paymentDetailsAppService;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _pdfOpt = pdfOptions.Value;
    }

    protected override void BuildArabicLayout(Document doc, UnitPackagePdfData data)
    {
        ImagesSection(doc, data.Organization.OrganizationPicture);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddOrganizationSection(doc, data, true);
        AddSummarySection(doc, data, true);
        AddQrAndTotalsSection(doc, data, true);
    }

    protected override void BuildEnglishLayout(Document doc, UnitPackagePdfData data)
    {
        ImagesSection(doc, data.Organization.OrganizationPicture);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddOrganizationSection(doc, data, false);
        AddSummarySection(doc, data, false);
        AddQrAndTotalsSection(doc, data, false);
    }

    public void AddHeaderSection(Document doc, UnitPackagePdfData data)
    {
        doc.Add(CreateCenteredParagraph($"{data.Localization.UnitPackageNumberView.ArabicWithFontGlyphsToPDF()}", _pdfOpt.fontSize, true, doc));
    }

    public void AddSubHeader(Document doc, UnitPackagePdfData data)
    {
        var text = CultureName == "ar"
            ? $"***{data.Localization.SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{data.Localization.SimplifiedTaxInvoice}***";
        doc.Add(CreateCenteredParagraph(text, _pdfOpt.fontSize, true, doc));
    }

    public void AddVatAndTicket(Document doc, UnitPackagePdfData data)
    {
        var text = CultureName == "ar"
            ? $"{data.Payment.PaymentNumber.ArabicWithFontGlyphsToPDF()}:{data.Localization.PaymentNumber.ArabicWithFontGlyphsToPDF()}"
            : $"{data.Localization.PaymentNumber}: {data.Payment.PaymentNumber}";
        doc.Add(CreateCenteredParagraph(text, 13, true, doc));
    }

    public void AddPaymentHeader(Document doc, UnitPackagePdfData data)
    {
        var currencyImg = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, doc, 10);
        var p = new Paragraph()
            .SetFont(Font)
            .SetFontSize(_pdfOpt.fontSize * ScaleFactor(doc))
            .SetTextAlignment(TextAlignment.CENTER);

        if (CultureName == "ar")
            p.Add(currencyImg).Add(" ").Add(data.Payment.Amount.ToString("F2")).Add(" ")
             .Add(data.Localization.HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF());
        else
            p.Add(data.Localization.HeaderVatReceiptPayment).Add(" ")
             .Add(data.Payment.Amount.ToString("F2")).Add(" ").Add(currencyImg);

        doc.Add(p);
    }

    public void AddOrganizationSection(Document doc, UnitPackagePdfData data, bool isArabic)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(UnitValue.CreatePercentArray(new float[] { 4, 4, 0.11f, 4, 4 }))
            .UseAllAvailableWidth()
            .SetMarginRight(7)
            .SetBorder(Border.NO_BORDER);
        var addressSection = new Table(UnitValue.CreatePercentArray(new float[] { 12, 3 }))
            .UseAllAvailableWidth()
            .SetMarginRight(7)
            .SetBorder(Border.NO_BORDER);

        var fontsize = (int)(_pdfOpt.fontSize * scale);

        var pkg = data.Data;
        var loc = data.Localization;
        var pay = data.Payment;
        var org = data.Organization;

        if (isArabic)
        {
            AddArabicCell(table, pay.CashierName.ArabicWithFontGlyphsToPDF(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.CashierNo.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, org.ArabicName.ArabicWithFontGlyphsToPDF(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.OrganizationName.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            AddArabicCell(table, pkg.DectivationDate.ToString(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.ExpirationDate.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, pkg.ActivationDate.ToString(), Font, fontsize);
            AddArabicLabelCell(table, $":{loc.SubscriptionDate.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            AddArabicCell(addressSection, pkg.UnitCode.ArabicWithFontGlyphsToPDF(), Font, fontsize);
            AddArabicLabelCell(addressSection, $":{loc.UnitCode.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicCell(addressSection, org.Address.ArabicWithFontGlyphsToPDF(), Font, fontsize);
            AddArabicLabelCell(addressSection, $":{loc.OrganizationAddress.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
        }
        else
        {
            AddEnglishLabelCell(table, $"{loc.CashierNo}:", Font, fontsize);
            AddEnglishCell(table, pay.CashierName, Font, fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationName}:", Font, fontsize);
            AddEnglishCell(table, org.EnglishName, Font, fontsize);

            AddEnglishLabelCell(table, $"{loc.UnitCode}:", Font, fontsize);
            AddEnglishCell(table, pkg.UnitCode, Font, fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationAddress}:", Font, fontsize);
            AddEnglishCell(table, org.Address, Font, fontsize);

            for (int i = 0; i < 5; i++) AddEnglishEmptyCell(table);

            AddEnglishLabelCell(table, $"{loc.SubscriptionDate}:", Font, fontsize);
            AddEnglishCell(table, pkg.ActivationDate.ToString(), Font, fontsize);
            AddEnglishLabelCell(table, $"{loc.ExpirationDate}:", Font, fontsize);
            AddEnglishCell(table, pkg.DectivationDate.ToString(), Font, fontsize);
        }

        doc.Add(table);
        doc.Add(addressSection);
    }

    public void AddSummarySection(Document doc, UnitPackagePdfData data, bool isArabic)
    {
        var table = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc));
        var days = (data.Data.DectivationDate - data.Data.ActivationDate).Days;

        if (isArabic)
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Days.ArabicWithFontGlyphsToPDF()} {days.ToString().ArabicWithFontGlyphsToPDF()}",
                $"{data.Localization.Subscription.ArabicWithFontGlyphsToPDF()}\n{data.Localization.UnitPackageNumberView.ArabicWithFontGlyphsToPDF()}",
                Font, true, doc);
        }
        else
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration}\n{data.Localization.Days} {days}",
                $"{data.Localization.Subscription}\n{data.Localization.UnitPackageNumberView}",
                Font, false, doc);
        }

        doc.Add(table);
    }

    public void AddQrAndTotalsSection(Document doc, UnitPackagePdfData data, bool isArabic)
    {
        var container = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc)).SetBorder(Border.NO_BORDER);
        var qr = CreateImage(Path.Combine(PathHelper.WwwRoot, data.Localization.QRCodePath),
            isArabic ? HorizontalAlignment.LEFT : HorizontalAlignment.RIGHT, doc, 153);

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

    public Table BuildTotalsTable(UnitPackagePdfData data, bool isArabic, Document doc)
    {
        var table = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var currency = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, doc, _pdfOpt.fontSize);
        var loc = data.Localization;

        // Prefer details (payment relation) because that's the authoritative source for unit packages.
        var price = data.PaymentDetails?.Price ?? data.Data.Price;
        var discount = data.PaymentDetails?.Discount ?? data.Data.Discount;
        var vat = data.PaymentDetails?.Vat ?? data.Data.Vat;

        AddSummaryRow(table, loc.TotalPreVAT, price.ToString("F2"), Font, isArabic, currency, doc);
        if (discount != default)
            AddSummaryRow(table, $"{loc.Discount}", discount.ToString("F2"), Font, isArabic, currency, doc);
        AddSummaryRow(table, $"{loc.Vats}", vat.ToString("F2"), Font, isArabic, currency, doc);
        AddSummaryRow(table, loc.TotalIncludeVAT, data.Payment.Amount.ToString("F2"), Font, isArabic, currency, doc);
        return table;
    }

    public async Task<Stream> CreatePdf(PaymentGetResponseDto payment, string PageSize)
    {
        var arabicCulture = new CultureInfo("ar");
        CultureInfo.CurrentCulture = arabicCulture;
        CultureInfo.CurrentUICulture = arabicCulture;

        var paymentDetails = await _paymentDetailsAppService.GetByPaymentIdAsync(payment.Id);
        var unitPackage = await _customerUnitPackageAppService.GetAsync(paymentDetails.PayableId);
        var qrCode = await _qrCodeAppService.CreateQRCOdeImageAsync(payment.QrCodeInvoice);

        var localization = _localizationFactoryAppService.CreateUnitPackageLocalization(new UnitPackageLocalizationCreateDto
        {
            QRCodePath = qrCode.Value,
            Price = paymentDetails.Price,
            PaymentAmount = payment.Amount
        });

        var data = new UnitPackagePdfData
        {
            Data = unitPackage,
            Payment = payment,
            PaymentDetails = paymentDetails,
            Organization = payment.Organization,
            Localization = localization
        };

        return Build(data, PageSize);
    }
}
