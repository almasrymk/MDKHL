using Abp.Application.Services;
using SmartParking.Invoices;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using SmartParking.Entities.Invoices;
using System;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout.Font;
using iText.Html2pdf.Css.Apply.Impl;
using Volo.Abp.Domain.Entities;
using Volo.Abp;
using RemoteServiceAttribute = Volo.Abp.RemoteServiceAttribute;
using iText.IO.Image;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Colors;
using SmartParking.Organizations;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Colorspace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using static QRCoder.SvgQRCode;
using System.Drawing;
using SmartParking.Payments;
using SmartParking.Localization;
using System.Globalization;
using AraibcPdfUnicodeGlyphsResharper;
using iText.IO.Font;
using iText.Kernel.Font;
using System.Linq;
using SmartParking.Entities.Organizations;



namespace SmartParking.PDF;

public class PDFGeneratorAppService : SmartParkingAppService, IPDFGeneratorAppService
{
    private readonly IInvoiceAppService _invoicePdfAppService;
    private readonly IOrganizationsAppService _organizationPdfAppService;
    private readonly IPaymentWebAppService _paymentWebAppService;
    
    private string folderInvoiceImage = "Invoice";
    private string folderPdfs = "PDFs";
    private string imageDirectoryPath = null;

    public PDFGeneratorAppService(IInvoiceAppService invoicePdfAppService, IOrganizationsAppService organizationPdfAppService, IPaymentWebAppService paymentWebAppService)
    {
        _invoicePdfAppService = invoicePdfAppService;
        imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderInvoiceImage, folderPdfs);
        _organizationPdfAppService = organizationPdfAppService;
        _paymentWebAppService = paymentWebAppService;
    }
    [RemoteService(isEnabled:false)]
    public async Task<string> GenerateInvoiceHtmlAsync(int invoiceId, string commericialRegisteratio,string qrCode)
    {
        var invoice = await _invoicePdfAppService.GetAsync(invoiceId);

        string imageDirectory = "wwwroot/images/logo/MDHKL_01.png";

        var htmlContent = $@"
<div class=""mud-modal"">
    <div class=""title-content d-flex justify-content-center align-items-center"">
        <h2 class=""fw-bold"">Invoice {invoice.InvoiceNumber}</h2>
    </div>
    <div class=""dialog-content"">
        <div class=""main-layout-paper d-flex flex-column p-3"" style=""width: 100%; height: 100%; margin-top: 20px;"">
            <div class=""d-flex justify-content-around align-items-center"" style=""margin-top: 20px;"">
                <div style=""width: 120px;"">
                    <img src=""data:image/png;base64,{imageDirectory}"" alt=""Logo"" />
                </div>
                <div style=""width: 120px;"">
                    <img src=""data:image/png;base64,{imageDirectory}"" alt=""Logo"" />
                </div>
            </div>

            <div class=""invoice-header d-flex flex-column flex-grow-1 h-100 p-3 justify-content-center align-items-center"">
                <h3>***Simplified tax invoice***</h3>
                <div class=""d-flex flex-row"">
                    <h5 class=""fw-bold"">VAT:</h5>
                    <h5>{commericialRegisteratio}</h5>
                </div>
            </div>

            <div class=""invoice-body d-flex flex-column justify-content-between"">
                <h4>
                    This is the receipt for payment of {(int)(invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m))} SAR
                </h4>
                <div class=""row"">
                    <div class=""col-md-6 d-flex flex-column"">
                        <h4>Payment Number:</h4><p>{invoice.InvoiceNumber}</p>
                    </div>
                    <div class=""col-md-6 d-flex flex-column align-items-center"">
                        <h4>Hotel Name:</h4><p>{invoice.OrganizationArabicName}</p>
                    </div>
                    <div class=""col-md-6 d-flex flex-column"">
                        <h4>Cashier No.:</h4><p>{invoice.GarageArabicName}</p>
                    </div>
                    <div class=""col-md-6 d-flex flex-column align-items-center"">
                        <h4>Hotel Address:</h4>
                        <p></p>
                    </div>
                    <div class=""col-md-12 d-flex flex-column"">
                        <h4>Payment Date:</h4><p>{DateTime.Now:yyyy-MM-dd}</p>
                    </div>
                    <div class=""col-md-12 d-flex flex-column"">
                        <h4>Plate Number:</h4><p>{invoice.PlateNumber}</p>
                    </div>
                    <div class=""col-md-6 d-flex flex-column"">
                        <h4>Entry Date:</h4><p>{invoice.EntranceTime:yyyy-MM-dd HH:mm:ss}</p>
                    </div>
                    <div class=""col-md-6 d-flex flex-column align-items-center"">
                        <h4>Checkout Date:</h4><p>{invoice.InvoiceTime:yyyy-MM-dd HH:mm:ss}</p>
                    </div>
                </div>
            </div>

            <div class=""d-flex justify-content-center align-items-center flex-row"" style=""background-color: beige; margin-top: 2rem; border-radius: 15px;"">
                <div class=""col-md-4 d-flex flex-column align-items-center justify-content-center"">
                    <h5>Subscription</h5>
                    <h5>Hour</h5>
                </div>
                <div class=""col-md-4 d-flex flex-column align-items-center justify-content-center"">
                    <h5>Quota</h5>
                    <h5>{((int)invoice.InvoicePrice / invoice.TotalHours)} Hour SAR/Hour</h5>
                </div>
                <div class=""col-md-4 d-flex flex-column align-items-center justify-content-center"">
                    <h5>Interval</h5>
                    <h5>{invoice.TotalHours} Hour</h5>
                </div>
            </div>

            <div class=""total-info"" style=""margin-top: 2rem;"">
                <div class=""row"">
                    <div class=""col-md-6"">
                        <div class=""d-flex flex-row"">
                            <p><strong>Total Pre VAT:</strong> {invoice.InvoicePrice} SAR</p>
                        </div>
                        <div class=""d-flex flex-row"">
                            <p><strong>VAT 14%:</strong> {(int)(invoice.InvoicePrice * 0.15m)} SAR</p>
                        </div>
                        <div class=""d-flex flex-row"">
                            <p><strong>Total Including VAT:</strong> {(int)(invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m))} SAR</p>
                        </div>
                    </div>
                    <div class=""col-md-6"">
                        <div class=""d-flex flex-row justify-content-end"">
                            <div style=""width: 120px;"">
                                <img src=""data:image/png;base64,{qrCode}"" alt=""UnPaid"" style=""width:120px;"" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
";



        return htmlContent;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, string commericialRegisteratio, string qrCode)
    {
        var htmlContent = await GenerateInvoiceHtmlAsync(invoiceId, commericialRegisteratio, qrCode);
         var result = GeneratePdfFromHtmlAsync(htmlContent, invoiceId);
        var file =await GetInvoicePDF(invoiceId);
        if (!Directory.Exists(imageDirectoryPath))
        {
           Directory.CreateDirectory(imageDirectoryPath);
        }
        string fileName = $"{invoiceId}.{"pdf"}";


        string fullPath = Path.Combine(imageDirectoryPath, fileName);
        await SaveInvoicePdf(invoiceId);


        return result;
    }
    [RemoteService(isEnabled: false)]


    public byte[] GeneratePdfFromHtmlAsync(string htmlContent,int invoiceId)
    {

        string cssContent = @"
.mud-modal {
    font-family: Arial, sans-serif;
color:black;
}

.title-content {
    display: flex;
    justify-content: center;
    align-items: center;
}

.fw-bold {
    font-weight: bold;
}

.dialog-content {
    padding: 10px;
}

.main-layout-paper {
    width: 100%;
    height: 100%;
    margin-top: 20px;
    display: flex;
    flex-direction: column;
    padding: 1rem;
}

.d-flex {
    display: flex;
}

.justify-content-around {
    justify-content: space-around;
}

.align-items-center {
    align-items: center;
}

.invoice-header {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
    height: 100%;
    padding: 1rem;
    justify-content: center;
    align-items: center;
}

.invoice-body {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
}

.row {
    display: flex;
    flex-wrap: wrap;
}

.col-md-6 {
    flex: 0 0 50%;
    max-width: 50%;
    padding: 5px;
}

.col-md-12 {
    flex: 0 0 100%;
    max-width: 100%;
    padding: 5px;
}

.p-3 {
    padding: 1rem;
}

.h-100 {
    height: 100%;
}

.invoice-header h3 {
    margin-bottom: 10px;
}

.d-flex.flex-row {
    display: flex;
    flex-direction: row;
}

.d-flex.flex-column {
    display: flex;
    flex-direction: column;
}

.total-info {
    margin-top: 2rem;
}

.total-info .row {
    display: flex;
    flex-wrap: wrap;
}

.total-info .col-md-6 {
    flex: 0 0 50%;
    max-width: 50%;
    padding: 10px;
}

.d-flex.flex-row.justify-content-end {
    display: flex;
    justify-content: flex-end;
}

.d-flex.flex-row.justify-content-center {
    display: flex;
    justify-content: center;
}

.bg-beige {
    background-color: beige;
}

.border-radius-15 {
    border-radius: 15px;
}

.col-md-4 {
    flex: 0 0 33.33%;
    max-width: 33.33%;
    padding: 5px;
}

img {
    max-width: 100%;
    height: auto;
}

img[style*='width:120px;'] {
    width: 120px;
}
";


        try
        {
            var combinedHtml = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <style>
                {cssContent}
            </style>
        </head>
        <body>
            {htmlContent}
        </body>
        </html>";


            using (var memoryStream = new MemoryStream())
            {
                // Create PDF writer using the memory stream
                var pdfWriter = new PdfWriter(memoryStream);

                // Initialize the PDF document
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    // Create a converter properties object
                    var converterProperties = new ConverterProperties();

                    // Set inline CSS as part of the properties
                    converterProperties.SetCssApplierFactory(new DefaultCssApplierFactory());
                    converterProperties.SetCharset("utf-8");

                    // Embed the CSS in the HTML content
                    string combinedHtmlWithCss = $@"
                                                <style>
                                                    {cssContent}
                                                </style>
                                                {combinedHtml}";

                    HtmlConverter.ConvertToPdf(combinedHtmlWithCss, pdfDocument, converterProperties);
                }

                return memoryStream.ToArray();
            }

        }
        catch (IOException ioEx)
        {
            Console.WriteLine("IO exception occurred: " + ioEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred: " + ex.Message);
            throw;
        }
      

    }
    public string GetPdfDirectory(int InvoiceId)
    {
        if(InvoiceId < 0)
        {
            throw new Exception("Invoice Id can't be null");
        }
        var invoice = _invoicePdfAppService.GetAsync(InvoiceId);
        if (invoice == null)
        {
            throw new EntityNotFoundException(typeof(Invoice),InvoiceId);
        }
        string fileName = $"{InvoiceId}.{"pdf"}";
        string fullPath = "invoice/PDFs/"+ fileName;

        return fullPath;

    }
    [RemoteServiceAttribute(isEnabled: true)]
    public async Task<Stream> SaveInvoicePdf(int invoiceId)
    {
        var invoice = await _invoicePdfAppService.GetAsync(invoiceId);
        var image = await _invoicePdfAppService.GetInvoiceQRCodeForOrganization(invoiceId);
        string imageDirectoryPdf = "wwwroot/images/logo/MDHKL_01.png";
        string fileName = $"{invoiceId}.{"pdf"}";
        string fullPath = Path.Combine(imageDirectoryPath, fileName);
        var payment = await _paymentWebAppService.GetPaymentForInvoice(invoiceId);
        var organization = await _organizationPdfAppService.GetAsync(payment.OrganizationId);
        string currentPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{organization.OrganizationPicture}");
        string QRCodePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{image.Value}");
        string InvoiceNumber = LocalizationHelper.GetLocalizedString("Invoice");
        string SimplifiedTaxInvoice = LocalizationHelper.GetLocalizedString("SimplifiedTaxInvoice");
        string HeaderVatReceiptPayment = LocalizationHelper.GetLocalizedString("HeaderVatReceiptPayment");
        string Vats = LocalizationHelper.GetLocalizedString("VAT");
        string Sar = LocalizationHelper.GetLocalizedString("SAR");
        string CashierNo = LocalizationHelper.GetLocalizedString("Cashier No");
        string OrganizationName = LocalizationHelper.GetLocalizedString("OrganizationNameInvoicePopUp");
        string OrganizationAddress = LocalizationHelper.GetLocalizedString("OrganizationAddressInvoicePopUp");
        string PlateNumber = LocalizationHelper.GetLocalizedString("PlateNumber");
        string EntranceTime = LocalizationHelper.GetLocalizedString("EntranceTime");
        string ExitTime = LocalizationHelper.GetLocalizedString("ExitTime");
        string Subscription = LocalizationHelper.GetLocalizedString("Subscription");
        string Hour = LocalizationHelper.GetLocalizedString("Hour");
        string Duration = LocalizationHelper.GetLocalizedString("TotalHours");
        string Hours = LocalizationHelper.GetLocalizedString("Hours");
        string TotalPreVAT = LocalizationHelper.GetLocalizedString("TotalPreVAT");
        string TotalIncludeVAT = LocalizationHelper.GetLocalizedString("TotalIncludeVAT");
        string Discount = LocalizationHelper.GetLocalizedString("Discount");
        string After = LocalizationHelper.GetLocalizedString("After");
        decimal price = (Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2));
        decimal paymentPrice = payment.Amount;
        MemoryStream stream = new MemoryStream();
        PdfWriter writer = new PdfWriter(stream);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);
        string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//fonts//NotoNaskhArabic-Regular.ttf");
        PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        FontSet set = new FontSet();
        set.AddFont(fontPath);
        Style styleBold = new Style();

        Style Center = new Style();


        Paragraph header = new Paragraph(invoice.InvoiceNumber.ToString()).
          AddStyle(styleBold)
         .SetTextAlignment(TextAlignment.CENTER)
         .SetFontSize(20);
        Table tableimages = new Table(3).UseAllAvailableWidth();

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
           .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(currentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(currentPath))
          : new iText.Layout.Element.Image(ImageDataFactory.Create(imageDirectoryPdf)))
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
            iText.Layout.Element.Image imageMdkhlLogo = new iText.Layout.Element.Image(ImageDataFactory.Create(imageDirectoryPdf))
           .SetHorizontalAlignment(HorizontalAlignment.LEFT)
           .SetWidth(120);

            iText.Layout.Element.Image OrgLogo = (File.Exists(currentPath)
          ? new iText.Layout.Element.Image(ImageDataFactory.Create(currentPath))
          : new iText.Layout.Element.Image(ImageDataFactory.Create(imageDirectoryPdf)))
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


        Paragraph paymentNumber = new Paragraph(CultureInfo.CurrentCulture.Name == "ar" ? $"{payment.PaymentNumber}{" "}{InvoiceNumber.ArabicWithFontGlyphsToPDF()}" : $"{InvoiceNumber}{" "}{payment.PaymentNumber}")
      .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
      .SetFontSize(13);

        Paragraph subheader = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
            ? $"***{SimplifiedTaxInvoice.ArabicWithFontGlyphsToPDF()}***"
            : $"***{SimplifiedTaxInvoice}***")
    .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
    .SetFontSize(13);

        Paragraph Vat = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
            ? $"{organization.TaxNumber.ArabicWithFontGlyphsToPDF()}:{Vats.ArabicWithFontGlyphsToPDF()}"
            : $"{Vats}: {organization.TaxNumber}")
            .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
            .SetFontSize(13);

        Paragraph RecieptPayemntDetails = new Paragraph(CultureInfo.CurrentCulture.Name == "ar"
            ? $"{Sar.ArabicWithFontGlyphsToPDF()}{paymentPrice}{" "}{HeaderVatReceiptPayment.ArabicWithFontGlyphsToPDF()}"
            : $"{HeaderVatReceiptPayment} {paymentPrice}{Sar}")
            .SimulateBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font)
            .SetFontSize(13);

        Table table = new Table(5).SetWidth(UnitValue.CreatePercentValue(100));
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
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .Add(new Paragraph($"{invoice.OrganizationArabicName.ArabicWithFontGlyphsToPDF()}")));
            table.AddCell(new Cell(1, 1)
                .SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($":{OrganizationName.ArabicWithFontGlyphsToPDF()}").SimulateBold()));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.GarageArabicName.ArabicWithFontGlyphsToPDF()}")));
            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($":{CashierNo.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.PlateNumber}")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($":{PlateNumber.ArabicWithFontGlyphsToPDF()}").SimulateBold()));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                  .Add(new Paragraph($"{invoice.OrganizationAddress.ArabicWithFontGlyphsToPDF()}")));
            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($":{OrganizationAddress.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.InvoiceTime.ToString()}")));
            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($":{ExitTime.ArabicWithFontGlyphsToPDF()}").SimulateBold()));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .Add(new Paragraph($"{invoice.EntranceTime.ToString()}")));
            table.AddCell(new Cell(1, 1).SetFont(font)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($":{EntranceTime.ArabicWithFontGlyphsToPDF()}").SimulateBold()));

            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopLeftRadius(new BorderRadius(10))
            .SetBorderBottomLeftRadius(new BorderRadius(10))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetFont(font)
             .Add(new Paragraph($"{Duration.ArabicWithFontGlyphsToPDF()}\n {Hours.ArabicWithFontGlyphsToPDF()}{" "}{invoice.TotalHours}").SetFontSize(14)));


            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopRightRadius(new BorderRadius(10))
                .SetBorderBottomRightRadius(new BorderRadius(10))
                .SetFont(font)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph($"{Subscription.ArabicWithFontGlyphsToPDF()}\n {Hour.ArabicWithFontGlyphsToPDF()}").SetFontSize(14)));
            iText.Layout.Element.Image QrCodeImage = new iText.Layout.Element.Image(ImageDataFactory.Create(QRCodePath))
           .SetHorizontalAlignment(HorizontalAlignment.LEFT)
           .SetWidth(120);

            table22.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(QrCodeImage));


            table21.AddCell(new Cell()
      .SetBorder(Border.NO_BORDER)
      .SetFont(font)
      .SetTextAlignment(TextAlignment.RIGHT)
      .SetVerticalAlignment(VerticalAlignment.TOP)
      .Add(new Paragraph()
          .Add(new Text($"{Sar.ArabicWithFontGlyphsToPDF()} {invoice.InvoicePrice.ToString("F2")}"))
          .Add(new Text($": {TotalPreVAT.ArabicWithFontGlyphsToPDF()}").SimulateBold())
          .SetFontSize(12)));

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetFont(font)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(new Paragraph()
                    .Add(new Text($"{Sar.ArabicWithFontGlyphsToPDF()} {Math.Round((invoice.InvoicePrice * 0.15m), 2).ToString("F2")}"))
                    .Add(new Text($": 15% {Vats.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                    .SetFontSize(12)));

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetFont(font)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(new Paragraph()
                    .Add(new Text($"{Sar.ArabicWithFontGlyphsToPDF()} {price.ToString("F2")}"))
                    .Add(new Text($": {TotalIncludeVAT.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                    .SetFontSize(12)));
            if(price!=paymentPrice)
            {
                table21.AddCell(new Cell()
              .SetBorder(Border.NO_BORDER)
              .SetFont(font)
              .SetTextAlignment(TextAlignment.RIGHT)
              .SetVerticalAlignment(VerticalAlignment.TOP)
              .Add(new Paragraph()
                  .Add(new Text($"{Sar.ArabicWithFontGlyphsToPDF()} {paymentPrice.ToString("F2")}"))
                  .Add(new Text($": {Discount.ArabicWithFontGlyphsToPDF() +" "+ After.ArabicWithFontGlyphsToPDF()}").SimulateBold())
                  .SetFontSize(12)));

            }

            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table22));
            table2.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table21));
        }

        else
        {
            table.AddCell(new Cell(1, 1)
                  .SetBorder(Border.NO_BORDER)
                  .SetTextAlignment(TextAlignment.LEFT)
                  .Add(new Paragraph($"{CashierNo}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.GarageEnglishName}")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{OrganizationName}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .SetBorder(Border.NO_BORDER)
                 .Add(new Paragraph($"{invoice.OrganizationEnglishName}")));

            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{OrganizationAddress}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .SetBorder(Border.NO_BORDER)
                  .Add(new Paragraph($"{invoice.OrganizationAddress}")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{PlateNumber}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.PlateNumber}")));

            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{EntranceTime}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.EntranceTime.ToString()}")));
            table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("   ")));

            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{ExitTime}:")).SimulateBold());
            table.AddCell(new Cell(1, 1)
                 .SetBorder(Border.NO_BORDER)
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph($"{invoice.InvoiceTime.ToString()}")));

            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopLeftRadius(new BorderRadius(10))
                .SetBorderBottomLeftRadius(new BorderRadius(10))
                .SetFont(font)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph($"{Subscription}\n {Hour}").SetFontSize(14)));



            table1.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTopRightRadius(new BorderRadius(10))
            .SetBorderBottomRightRadius(new BorderRadius(10))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetFont(font)
             .Add(new Paragraph($"{Duration}\n {invoice.TotalHours}{" "}{Hours}").SetFontSize(14)));

            table21.AddCell(new Cell()
             .SetBorder(Border.NO_BORDER)
             .SetTextAlignment(TextAlignment.LEFT)
             .SetVerticalAlignment(VerticalAlignment.TOP)
              .Add(new Paragraph()
                .Add(new Text($"{TotalPreVAT}: ").SimulateBold())
                .Add(new Text(invoice.InvoicePrice.ToString("F2") + $"{Sar} "))
                .SetFontSize(12)));

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(new Paragraph()
                    .Add(new Text($"{Vats} 15%: ").SimulateBold())
                    .Add(new Text((Math.Round((invoice.InvoicePrice * 0.15m), 2)).ToString("F2") + $"{Sar}"))
                    .SetFontSize(12)));

            table21.AddCell(new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .Add(new Paragraph()
                    .Add(new Text($"{TotalIncludeVAT}: ").SimulateBold())
                    .Add(new Text(price.ToString("F2") + $" {Sar}"))
                    .SetFontSize(12)));
            if (price != paymentPrice)
            {
                table21.AddCell(new Cell()
              .SetBorder(Border.NO_BORDER)
              .SetFont(font)
              .SetTextAlignment(TextAlignment.LEFT)
              .SetVerticalAlignment(VerticalAlignment.TOP)
              .Add(new Paragraph()
                  .Add(new Text($"{After.ArabicWithFontGlyphsToPDF()+ " " +Discount.ArabicWithFontGlyphsToPDF()}:").SimulateBold())
                  .Add(new Text($" {paymentPrice.ToString("F2")} {Sar.ArabicWithFontGlyphsToPDF()}"))
                  .SetFontSize(12)));

            }
            iText.Layout.Element.Image QrCodeImage = new iText.Layout.Element.Image(ImageDataFactory.Create(QRCodePath))
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
        document.Add(RecieptPayemntDetails);
        document.Add(table);
        document.Add(table1);
        document.Add(table2);
        document.Close();
        return stream;
    }

    public async Task<IActionResult> GetInvoicePDF(int invoiceId)
    {
        var stream = await SaveInvoicePdf(invoiceId);
        return new FileContentResult(stream.GetAllBytes(), "application/pdf");
    }
}
