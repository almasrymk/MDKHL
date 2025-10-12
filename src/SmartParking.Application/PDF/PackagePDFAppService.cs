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
    public void ImagesSection(Document doc, PackagePdfData localizationData)
    {
        Table tableimages = new Table(3).UseAllAvailableWidth();

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
           .SetWidth(120);
            var orgPicture = System.IO.Path.GetFileName(localizationData.Organization.OrganizationPicture);
            string imagePath =System.IO.Path.Combine(Directory.GetCurrentDirectory(), PathHelper.WwwRoot,PathHelper.OrganizationFolder ,orgPicture+".png");
            iText.Layout.Element.Image OrgLogo = (File.Exists(imagePath)
          ? CreateImage(imagePath, HorizontalAlignment.RIGHT, 120)
          : CreateImage(PathHelper.imageDirectoryPdf, true ? HorizontalAlignment.RIGHT : HorizontalAlignment.RIGHT, 120));

            tableimages.AddCell(new Cell().Add(OrgLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT));

            tableimages.AddCell(new Cell().Add(imageMdkhlLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT));


        }
        else
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(PathHelper.imageDirectoryPdf))
        .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
        .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"{PathHelper.WwwRoot}{localizationData.Organization.OrganizationPicture}.png"))
          ? CreateImage(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + localizationData.Organization.OrganizationPicture), true ? HorizontalAlignment.RIGHT : HorizontalAlignment.RIGHT, 120)
          : CreateImage(PathHelper.imageDirectoryPdf, true ? HorizontalAlignment.RIGHT : HorizontalAlignment.RIGHT, 120));

            tableimages.AddCell(new Cell().Add(OrgLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT));

            tableimages.AddCell(new Cell().Add(imageMdkhlLogo)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT));


        }

        doc.Add(tableimages);
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

    public void AddHeaderSection(Document doc, PackagePdfData data)
    {
        var header = CreateCenteredParagraph(data.Localization.PackageNumberView.ToString().ArabicWithFontGlyphsToPDF(), 20, true);
        doc.Add(header);
    }

    public void AddSubHeader(Document doc, PackagePdfData data)
    {
        var text = CultureName == "ar"
            ? $"***{data.Localization.SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{data.Localization.SimplifiedTaxInvoice}***";
        doc.Add(CreateCenteredParagraph(text, 13, true));
    }

    public void AddOrganizationSection(Document doc, PackagePdfData data, bool isArabic)
    {
        var table = new Table(5).UseAllAvailableWidth();
        var package = data.Data;
        var loc = data.Localization;
        var payment = data.Payment;
        var organization = data.Organization;

        if (isArabic)
        {
            // Row 1: Cashier + Organization name (aligned)
            AddArabicCell(table, payment.CashierName.ArabicWithFontGlyphsToPDF(), Font, 10);
            AddArabicLabelCell(table, $":{loc.CashierNo.ArabicWithFontGlyphsToPDF()}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, organization.ArabicName.ArabicWithFontGlyphsToPDF(), Font, 10);
            AddArabicLabelCell(table, $":{loc.OrganizationName.ArabicWithFontGlyphsToPDF()}", Font);

            // Row 2: Plate number + Address
            AddArabicCell(table, package.EnPlateNumber.ArabicWithFontGlyphsToPDF(), Font, 10);
            AddArabicLabelCell(table, $":{loc.PlateNumber.ArabicWithFontGlyphsToPDF()}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, organization.Address.ArabicWithFontGlyphsToPDF(), Font, 10);
            AddArabicLabelCell(table, $":{loc.OrganizationAddress.ArabicWithFontGlyphsToPDF()}", Font);

            // Spacer row for visual balance
            for (int i = 0; i < 5; i++)
                AddArabicEmptyCell(table);

            AddArabicCell(table, package.ExpiryDate.ToString(), Font, 10);
            AddArabicLabelCell(table, $":{loc.ExpirationDate.ArabicWithFontGlyphsToPDF()}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, package!.ActivationDateOfPackage.Value.ToString(), Font, 10);
            AddArabicLabelCell(table, $":{loc.SubscriptionDate.ArabicWithFontGlyphsToPDF()}", Font);
        }
        else
        {
            // Row 1: Cashier + Organization name (aligned)
            AddArabicCell(table, payment.CashierName, Font, 10);
            AddArabicLabelCell(table, $":{loc.CashierNo}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, organization.ArabicName, Font, 10);
            AddArabicLabelCell(table, $":{loc.OrganizationName}", Font);

            // Row 2: Plate number + Address
            AddArabicCell(table, package.EnPlateNumber, Font, 10);
            AddArabicLabelCell(table, $":{loc.PlateNumber}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, organization.Address, Font, 10);
            AddArabicLabelCell(table, $":{loc.OrganizationAddress}", Font);

            // Spacer row for visual balance
            for (int i = 0; i < 5; i++)
                AddArabicEmptyCell(table);

            AddArabicCell(table, package!.ActivationDateOfPackage.Value.ToString(), Font, 10);
            AddArabicLabelCell(table, $":{loc.SubscriptionDate}", Font);
            AddArabicEmptyCell(table);
            AddArabicCell(table, package.ExpiryDate.ToString(), Font, 10);
            AddArabicLabelCell(table, $":{loc.ExpirationDate
                }", Font);
        }

        doc.Add(table);
    }

    public void AddSummarySection(Document doc, PackagePdfData data, bool isArabic)
    {
        var table = new Table(2).UseAllAvailableWidth().SetMarginTop(20);
        var expiryDate = data.Data.ExpiryDate;
        var activationDate = data!.Data!.ActivationDateOfPackage!.Value;
        var daysDifference = (expiryDate - activationDate).Days;
        if (isArabic)
        {
        
            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration.ArabicWithFontGlyphsToPDF()}\n{data.Localization.Days.ArabicWithFontGlyphsToPDF()} {daysDifference.ToString().ArabicWithFontGlyphsToPDF()}",
                $"{data.Localization.Subscription.ArabicWithFontGlyphsToPDF()}\n{data.Localization.PackageNumberView.ArabicWithFontGlyphsToPDF()}",
                Font,
                true);
        }
        else
        {

            AddGrayBoxRow(
                table,
                $"{data.Localization.Duration}\n{data.Localization.Days} {daysDifference.ToString()}",
                $"{data.Localization.Subscription}\n{data.Localization.PackageNumberView}",
                Font,
                true);
        }

        doc.Add(table);
    }

    public void AddQrAndTotalsSection(Document doc, PackagePdfData data, bool isArabic)
    {
        var container = new Table(2).UseAllAvailableWidth().SetMarginTop(20).SetBorder(Border.NO_BORDER);
        var qrTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var qr = CreateImage(System.IO.Path.Combine(PathHelper.WwwRoot,data.Localization.QRCodePath), isArabic ? HorizontalAlignment.LEFT : HorizontalAlignment.RIGHT, 120);

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

    public Table BuildTotalsTable(PackagePdfData data, bool isArabic)
    {
        var table = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
        var currency = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, 10);
        var pkg = data.Data;
        var loc = data.Localization;
        AddSummaryRow(table, loc.TotalPreVAT, pkg.PriceOfPackage!.Value.ToString("F2"), Font, isArabic, currency);
        var vat = pkg.Vat.ToString("F2");
        AddSummaryRow(table, $"15% {loc.Vats}", vat, Font, isArabic, currency);
        AddSummaryRow(table, loc.TotalIncludeVAT, data.Payment.Amount.ToString("F2"), Font, isArabic, currency);
        if ((pkg.PriceOfPackage.Value + pkg.Vat) != data.Payment.Amount)
            AddSummaryRow(table, $"{loc.After} {loc.Discount}", pkg.Discount.ToString("F2"), Font, isArabic, currency);
        return table;
    }
    public async Task<Stream> CreatePdf(int packageId)
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

        return Build(data);
    }

    public void AddVatAndTicket(Document doc, PackagePdfData data)
    {
        var vatText = CultureName == "ar"
                ? $"{data.Payment.PaymentNumber.ArabicWithFontGlyphsToPDF()}:{data.Localization.PaymentNumber.ArabicWithFontGlyphsToPDF()}"
                : $"{data.Localization.PaymentNumber}: {data.Payment.PaymentNumber}";

        var vatParagraph = CreateCenteredParagraph(vatText, 13, true);
        doc.Add(vatParagraph);
    }
    public void AddPaymentHeader(Document doc, PackagePdfData data)
    {
        var currencyImg = CreateImage(PathHelper.CurrencyIcon, HorizontalAlignment.LEFT, 10);
        var p = new Paragraph()
            .SetFont(Font)
            .SetFontSize(13)
            .SetTextAlignment(TextAlignment.CENTER);

        var loc = data.Localization;
        var paymentAmount = data.Payment.Amount.ToString("F2");

        if (CultureName == "ar")
        {
            p.Add(currencyImg)
             .Add(" ")
             .Add(new Text(paymentAmount))
             .Add(" ")
             .Add(new Text(loc.HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF()));
        }
        else
        {
            p.Add(new Text(loc.HeaderVatReceiptPayment))
             .Add(" ")
             .Add(new Text(paymentAmount))
             .Add(" ")
             .Add(currencyImg);
        }

        doc.Add(p);
    }

}
