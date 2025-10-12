using AraibcPdfUnicodeGlyphsResharper;
using Humanizer;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Options;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Invoices;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.QRCode;
using SmartParking.Utilities;
using SmartParking.ZATCATaxInvoice;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Path = System.IO.Path;


namespace SmartParking.PDF;

public class PackagePDFAppService : BasePDFGeneratorAppService<PackagePdfData>, IPDFContractsAppService<PackagePdfData>, IPDFGeneratorAppService
{
    private readonly ICustomerPackagesAppService _packageAppService;
    private readonly IPaymentWebAppService _paymentAppService;
    private readonly IOrganizationsAppService _organizationAppService;
    private readonly ILocalizationFactoryAppService _localizationFactoryAppService;
    private readonly IQRCodeAppService _qrCodeAppService;


    public PackagePDFAppService(
        IOptions<PdfSettings> pdfSettings,
        ICustomerPackagesAppService packageAppService,
        IPaymentWebAppService paymentAppService,
        IOrganizationsAppService organizationAppService,
        ILocalizationFactoryAppService localizationFactoryAppService,
        IQRCodeAppService qrCodeAppService) : base(pdfSettings)
    {
        _packageAppService = packageAppService;
        _paymentAppService = paymentAppService;
        _organizationAppService = organizationAppService;
        _localizationFactoryAppService = localizationFactoryAppService;
        _qrCodeAppService = qrCodeAppService;
    }

    // -----------------------------
    // PDF BUILDERS
    // -----------------------------
    public void ImagesSection(Document doc, PackagePdfData data)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(3).UseAllAvailableWidth();

        var rightLogo = CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.RIGHT, doc, 120 * scale);

        string orgPicture =Path.GetFileName(data.Organization.OrganizationPicture);
        string imagePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            PathHelper.WwwRoot,
            PathHelper.OrganizationFolder,
            orgPicture + ".png");

        var leftLogo = File.Exists(imagePath)
            ? CreateImage(imagePath, HorizontalAlignment.LEFT, doc, 120 * scale)
            : CreateImage(PathHelper.imageDirectoryPdf, HorizontalAlignment.LEFT, doc, 120 * scale);

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

    protected override void BuildArabicLayout(Document doc, PackagePdfData data)
    {
        ImagesSection(doc, data);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddOrganizationSection(doc, data, true);
        AddSummarySection(doc, data, true);
        AddQrAndTotalsSection(doc, data, true);
    }

    protected override void BuildEnglishLayout(Document doc, PackagePdfData data)
    {
        ImagesSection(doc, data);
        AddHeaderSection(doc, data);
        AddSubHeader(doc, data);
        AddVatAndTicket(doc, data);
        AddOrganizationSection(doc, data, false);
        AddSummarySection(doc, data, false);
        AddQrAndTotalsSection(doc, data, false);
    }

    // -----------------------------
    // HEADER & SUBHEADER
    // -----------------------------
    public void AddHeaderSection(Document doc, PackagePdfData data)
    {
        doc.Add(CreateCenteredParagraph($"{data.Localization.PackageNumberView.ArabicWithFontGlyphsToPDF()}", 20, true, doc));
    }

    public void AddSubHeader(Document doc, PackagePdfData data)
    {
        var text = CultureName == "ar"
            ? $"***{data.Localization.SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{data.Localization.SimplifiedTaxInvoice}***";
        doc.Add(CreateCenteredParagraph(text, 13, true, doc));
    }

    public void AddOrganizationSection(Document doc, PackagePdfData data, bool isArabic)
    {
        float scale = ScaleFactor(doc);
        var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 1, 2, 2 }))
            .UseAllAvailableWidth()
            .SetBorder(Border.NO_BORDER);

        var fontsize = (int)(10 * scale);

        var pkg = data.Data;
        var loc = data.Localization;
        var pay = data.Payment;
        var org = data.Organization;

        if (isArabic)
        {
            AddArabicCell(table, pay.CashierName.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.CashierNo.ArabicWithFontGlyphsToPDF()}", Font , fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, org.ArabicName.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.OrganizationName.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            AddArabicCell(table, pkg.EnPlateNumber.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.PlateNumber.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, org.Address.ArabicWithFontGlyphsToPDF(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.OrganizationAddress.ArabicWithFontGlyphsToPDF()}", Font, fontsize);

            for (int i = 0; i < 5; i++) AddArabicEmptyCell(table);

            AddArabicCell(table, pkg.ExpiryDate.ToString(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.ExpirationDate.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
            AddArabicEmptyCell(table);
            AddArabicCell(table, pkg.ActivationDateOfPackage!.Value.ToString(), Font,fontsize);
            AddArabicLabelCell(table, $":{loc.SubscriptionDate.ArabicWithFontGlyphsToPDF()}", Font, fontsize);
        }
        else
        {
            AddEnglishLabelCell(table, $"{loc.CashierNo}:", Font , fontsize);
            AddEnglishCell(table, pay.CashierName, Font,fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationName}:", Font, fontsize);
            AddEnglishCell(table, org.EnglishName, Font,fontsize);

            AddEnglishLabelCell(table, $"{loc.PlateNumber}:", Font, fontsize);
            AddEnglishCell(table, pkg.EnPlateNumber, Font,fontsize);
            AddEnglishEmptyCell(table);
            AddEnglishLabelCell(table, $"{loc.OrganizationAddress}:", Font, fontsize);
            AddEnglishCell(table, org.Address, Font,fontsize);

            for (int i = 0; i < 5; i++) AddEnglishEmptyCell(table);

            AddEnglishLabelCell(table, $"{loc.SubscriptionDate}:", Font, fontsize);
            AddEnglishCell(table, pkg.ActivationDateOfPackage!.Value.ToString(), Font,fontsize);
            AddEnglishLabelCell(table, $"{loc.ExpirationDate}:", Font, fontsize);
            AddEnglishCell(table, pkg.ExpiryDate.ToString(), Font,fontsize);
        }

        doc.Add(table);
    }
    public void AddSummarySection(Document doc, PackagePdfData data, bool isArabic)
    {
        var table = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc));
        var days = (data.Data.ExpiryDate - data.Data.ActivationDateOfPackage!.Value).Days;

        if (isArabic)
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Days.ArabicWithFontGlyphsToPDF()} {days.ToString().ArabicWithFontGlyphsToPDF()}",
                $"{data.Localization.Subscription.ArabicWithFontGlyphsToPDF()}\n{data.Localization.PackageNumberView.ArabicWithFontGlyphsToPDF()}",
                Font, true, doc);
        }
        else
        {
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration}\n{data.Localization.Days} {days}",
                $"{data.Localization.Subscription}\n{data.Localization.PackageNumberView}",
                Font, false, doc);
        }

        doc.Add(table);
    }
    public void AddQrAndTotalsSection(Document doc, PackagePdfData data, bool isArabic)
    {
        var container = new Table(2).UseAllAvailableWidth().SetMarginTop(20 * ScaleFactor(doc)).SetBorder(Border.NO_BORDER);
        var qr = CreateImage(Path.Combine(PathHelper.WwwRoot, data.Localization.QRCodePath),
            isArabic ? HorizontalAlignment.LEFT : HorizontalAlignment.RIGHT, doc, 120);

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

    public Table BuildTotalsTable(PackagePdfData data, bool isArabic, Document doc)
    {
        var table = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var currency = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, doc, 10);
        var pkg = data.Data;
        var loc = data.Localization;

        AddSummaryRow(table, loc.TotalPreVAT, pkg.PriceOfPackage!.Value.ToString("F2"), Font, isArabic, currency, doc);
        AddSummaryRow(table, $"15% {loc.Vats}", pkg.Vat.ToString("F2"), Font, isArabic, currency, doc);
        AddSummaryRow(table, loc.TotalIncludeVAT, data.Payment.Amount.ToString("F2"), Font, isArabic, currency, doc);

        if ((pkg.PriceOfPackage.Value + pkg.Vat) != data.Payment.Amount)
            AddSummaryRow(table, $"{loc.After} {loc.Discount}", pkg.Discount.ToString("F2"), Font, isArabic, currency, doc);

        return table;
    }
    public async Task<Stream> CreatePdf(int packageId, string PageSize)
    {
        var arabicCulture = new CultureInfo("ar");
        CultureInfo.CurrentCulture = arabicCulture;
        CultureInfo.CurrentUICulture = arabicCulture;

        var package = await _packageAppService.GetAsync(packageId);
        var payment = await _paymentAppService.GetPaymentForPackage(packageId);
        var organization = await _organizationAppService.GetAsync(payment.OrganizationId);
        var qrCode = await _qrCodeAppService.CreateQRCOdeImageAsync(payment.QrCodeInvoice);

        var localization = _localizationFactoryAppService.CreatePackageLocalization(new PackageLocalizationCreateDto
        {
            PackageName = package.PackageArabicName,
            QRCodePath = qrCode.Value,
            Price = package.PriceOfPackage.Value,
            PaymentAmount = payment.Amount
        });

        var data = new PackagePdfData
        {
            Data = package,
            Payment = payment,
            Organization = organization,
            Localization = localization
        };

        return Build(data , PageSize);
    }

    public void AddVatAndTicket(Document doc, PackagePdfData data)
    {
        var text = CultureName == "ar"
            ? $"{data.Payment.PaymentNumber.ArabicWithFontGlyphsToPDF()}:{data.Localization.PaymentNumber.ArabicWithFontGlyphsToPDF()}"
            : $"{data.Localization.PaymentNumber}: {data.Payment.PaymentNumber}";
        doc.Add(CreateCenteredParagraph(text, 13, true, doc));
    }

    public void AddPaymentHeader(Document doc, PackagePdfData data)
    {
        var currencyImg = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, doc, 10);
        var p = new Paragraph()
            .SetFont(Font)
            .SetFontSize(13 * ScaleFactor(doc))
            .SetTextAlignment(TextAlignment.CENTER);

        if (CultureName == "ar")
            p.Add(currencyImg).Add(" ").Add(data.Payment.Amount.ToString("F2")).Add(" ")
             .Add(data.Localization.HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF());
        else
            p.Add(data.Localization.HeaderVatReceiptPayment).Add(" ")
             .Add(data.Payment.Amount.ToString("F2")).Add(" ").Add(currencyImg);

        doc.Add(p);
    }

}
