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



namespace SmartParking.PDF;

public class PDFGeneratorAppService : SmartParkingAppService, IPDFGeneratorAppService
{
    private readonly IInvoiceAppService _invoicePdfAppService;
    private string folderInvoiceImage = "Invoice";
    private string folderPdfs = "PDFs";
    private string imageDirectoryPath = null;

    public PDFGeneratorAppService(IInvoiceAppService invoicePdfAppService)
    {
        _invoicePdfAppService = invoicePdfAppService;
        imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderInvoiceImage, folderPdfs);

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
                        <h4>Cashier No.:</h4><p>{invoice.GarageName}</p>
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
    [RemoteServiceAttribute(isEnabled:true)]
    public async Task<Stream> SaveInvoicePdf(int invoiceId)
    {
        var invoice = await _invoicePdfAppService.GetAsync(invoiceId);
        var image = await _invoicePdfAppService.GetInvoiceQRCodeForOrganization(invoiceId);
        string imageDirectoryPdf = "wwwroot/images/logo/MDHKL_01.png";
        string fileName = $"{invoiceId}.{"pdf"}";
        string fullPath = Path.Combine(imageDirectoryPath, fileName);

        MemoryStream stream = new MemoryStream();
        PdfWriter writer = new PdfWriter(stream);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);

        Style styleBold = new Style();
        styleBold.SetBold();

        Style Center = new Style();

        Paragraph header = new Paragraph(invoice.InvoiceNumber.ToString()).
          AddStyle(styleBold)
         .SetTextAlignment(TextAlignment.CENTER)
         .SetFontSize(20);
        Image imageMdkhlLogo = new Image(ImageDataFactory
        .Create(imageDirectoryPdf)).SetHorizontalAlignment(HorizontalAlignment.CENTER).SetWidth(120);

        Paragraph subheader = new Paragraph(" * **Simplified tax invoice***")
         .SetTextAlignment(TextAlignment.CENTER)
         .SetFontSize(15);

        Paragraph Vat = new Paragraph("VAT : ")
            .AddStyle(styleBold)
         .SetTextAlignment(TextAlignment.CENTER)
         .SetFontSize(15).Add(@"1230000000");

        Paragraph RecieptPayemntDetails = new Paragraph($"This is the receipt for payment of This is the receipt for payment of {invoice.InvoicePrice} SAR")
            .AddStyle(styleBold)
         .SetTextAlignment(TextAlignment.LEFT)
         .SetFontSize(15);
        Table table = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Payment Number:")));
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph($"{invoice.InvoiceNumber}")));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Hotel Name:")));
        table.AddCell(new Cell(1, 1)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph($"{invoice.OrganizationArabicName}")));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Customer Name:")));
        table.AddCell(new Cell(1, 1)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph($"{invoice.CustomerEnName ?? "Unkown"}")));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Cashier No.:")));
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph($"{invoice.GarageName}")));

        table.AddCell(new Cell(1, 1)
             .SetBorder(Border.NO_BORDER)
             .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Payment Date:")));
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph(DateTime.UtcNow.ToString())));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Plate Number:")));
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph($"{invoice.PlateNumber}"))).SetBorder(Border.NO_BORDER);

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Entry Date:")));
        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph($"{invoice.EntranceTime.ToString()}")));

        table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph("Exit Date:")))
            .AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph($"{invoice.InvoiceTime.ToString()}")));


        Table table1 = new Table(3).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(25);


        table1.AddCell(new Cell(1, 1)
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .Add(new Paragraph("Subscription\nHour").SetFontSize(14).SetBold()));

        table1.AddCell(new Cell(1, 1)
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .Add(new Paragraph("Quota\n" + ((int)invoice.InvoicePrice / invoice.TotalHours) + " Hour SAR/Hour").SetFontSize(14).SetBold()));

        table1.AddCell(new Cell(1, 1)
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .Add(new Paragraph("Interval\n" + invoice.TotalHours + " Hour").SetFontSize(14).SetBold()));


        Table table2 = new Table(2).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(25);

        table2.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER) // No border
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .Add(new Paragraph("Total Pre VAT: " + invoice.InvoicePrice.ToString("F2") + " SAR").SetFontSize(12).SetBold()));

        table2.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .Add(new Paragraph("VAT 15%: " + (Math.Round((invoice.InvoicePrice * 0.15m), 2)).ToString("F2") + " SAR").SetFontSize(12).SetBold()));

        table2.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .Add(new Paragraph("Total Include VAT: " + (Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2)).ToString("F2") + " SAR").SetFontSize(12).SetBold()));



        document.Add(header);
        document.Add(imageMdkhlLogo);
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
