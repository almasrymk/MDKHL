using Abp.Application.Services;
using SmartParking.Invoices;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Threading.Tasks;
using Abp.Dependency;
using SmartParking.Entities.Invoices;
using System;


namespace SmartParking.PDF;

public class PDFGeneratorAppService : SmartParkingAppService, IPDFGeneratorAppService
{
    private readonly IInvoiceAppService _invoicePdfAppService;
    private string folderQrImage = "Invoice";
    private string folderImages = "PDFs";

    public PDFGeneratorAppService(IInvoiceAppService invoicePdfAppService)
    {
        _invoicePdfAppService = invoicePdfAppService;
    }

    public async Task<string> GenerateInvoiceHtmlAsync(int invoiceId)
    {
        //var invoice = await _invoicePdfAppService.GetAsync(invoiceId);

        //var htmlContent = $@"
        //<div class=""mud-modal"">
        //    <div class=""title-content d-flex justify-content-center align-items-center"">
        //        <h2 class=""fw-bold"">Invoice {invoice.InvoiceNumber}</h2>
        //    </div>
        //    <div class=""dialog-content"">
        //        <div class=""main-layout-paper d-flex flex-column p-3"" style=""width: 100%; height: 100%; margin-top: 20px;"">
        //            <div class=""invoice-header d-flex flex-column flex-grow-1 h-100 p-3 justify-content-center align-items-center"">
        //                <h3>***Simplified tax invoice***</h3>
        //                <div class=""d-flex flex-row"">
        //                    <h5 class=""fw-bold"">VAT:</h5>
        //                    <h5>{invoice.GarageName}</h5>
        //                </div>
        //            </div>

        //            <div class=""invoice-body d-flex flex-column justify-content-between"">
        //                <h4>This is the receipt for payment of {invoice.InvoicePrice} SAR</h4>
        //                <div class=""row"">
        //                    <div class=""col-md-6 d-flex flex-column"">
        //                        <strong>Payment Number:</strong><p>{invoice.InvoiceNumber}</p>
        //                    </div>
        //                    <div class=""col-md-6 d-flex flex-column align-items-center"">
        //                        <strong>Hotel Name:</strong><p>{invoice.OrganizationArabicName}</p>
        //                    </div>
        //                    <div class=""col-md-6 d-flex flex-column"">
        //                        <strong>Plate Number:</strong><p>{invoice.PlateNumber}</p>
        //                    </div>
        //                    <div class=""col-md-12 d-flex flex-column"">
        //                        <strong>Payment Date:</strong><p>{invoice.InvoiceTime.ToString()}</p>
        //                    </div>
        //                </div>
        //            </div>

        //            <div class=""total-info"" style=""margin-top: 2rem;"">
        //                <div class=""row"">
        //                    <div class=""col-md-6"">
        //                        <div class=""d-flex flex-row"">
        //                            <p><strong>Total Pre VAT:</strong> {invoice.InvoicePrice * 0.85m} SAR</p>
        //                        </div>
        //                        <div class=""d-flex flex-row"">
        //                            <p><strong>VAT 15%:</strong> {invoice.InvoicePrice * 0.15m} SAR</p>
        //                        </div>
        //                        <div class=""d-flex flex-row"">
        //                            <p><strong>Total Include VAT:</strong> {invoice.InvoicePrice} SAR</p>
        //                        </div>
        //                    </div>
        //                </div>
        //            </div>
        //        </div>
        //    </div>
        //</div>";
        var simpleHtml = @"
<html>
    <body>
        <h1 style='color:black;'>Invoice #12345</h1>
        <p>Total Amount: 1500 SAR</p>
    </body>
</html>";

        return simpleHtml;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var htmlContent = await GenerateInvoiceHtmlAsync(invoiceId);
        var result = GeneratePdfFromHtmlAsync(htmlContent);
        string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderQrImage, folderImages);

        if (!Directory.Exists(imageDirectoryPath))
        {
            Directory.CreateDirectory(imageDirectoryPath);
        }
        string fileName = $"{invoiceId}.{"pdf"}";
        string fullPath = Path.Combine(imageDirectoryPath, fileName);
        File.WriteAllBytes(fileName, result);

        return result;
    }

    public byte[] GeneratePdfFromHtmlAsync(string htmlContent)
    {
        byte[] bytes;

        //Boilerplate iTextSharp setup here
        //Create a stream that we can write to, in this case a MemoryStream
        using (var ms = new MemoryStream())
        {

            //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
            using (var doc = new Document())
            {

                //Create a writer that's bound to our PDF abstraction and our stream
                using (var writer = PdfWriter.GetInstance(doc, ms))
                {

                    //Open the document for writing
                    doc.Open();

                    //Our sample HTML and CSS
                    var example_html = @"<p>This <em>is </em><span class=""headline"" style=""text-decoration: underline;"">some</span> <strong>sample <em> text</em></strong><span style=""color: red;"">!!!</span></p>";
                    var example_css = @".headline{font-size:200%}";

                    /**************************************************
                     * Example #1                                     *
                     *                                                *
                     * Use the built-in HTMLWorker to parse the HTML. *
                     * Only inline CSS is supported.                  *
                     * ************************************************/

                    //Create a new HTMLWorker bound to our document
                    using (var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(doc))
                    {

                        //HTMLWorker doesn't read a string directly but instead needs a TextReader (which StringReader subclasses)
                        using (var sr = new StringReader(example_html))
                        {

                            //Parse the HTML
                            htmlWorker.Parse(sr);
                        }
                    }

                    /**************************************************
                     * Example #2                                     *
                     *                                                *
                     * Use the XMLWorker to parse the HTML.           *
                     * Only inline CSS and absolutely linked          *
                     * CSS is supported                               *
                     * ************************************************/

                    //XMLWorker also reads from a TextReader and not directly from a string
                    using (var srHtml = new StringReader(example_html))
                    {

                        //Parse the HTML
                        iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                    }

                    /**************************************************
                     * Example #3                                     *
                     *                                                *
                     * Use the XMLWorker to parse HTML and CSS        *
                     * ************************************************/

                    //In order to read CSS as a string we need to switch to a different constructor
                    //that takes Streams instead of TextReaders.
                    //Below we convert the strings into UTF8 byte array and wrap those in MemoryStreams
                    using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_css)))
                    {
                        using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_html)))
                        {

                            //Parse the HTML
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, msCss);
                        }
                    }


                    doc.Close();
                }
            }

            //After all of the PDF "stuff" above is done and closed but **before** we
            //close the MemoryStream, grab all of the active bytes from the stream
            bytes = ms.ToArray();
        }
        return bytes;
    }

        
}

/*
  public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var htmlContent = await GenerateInvoiceHtmlAsync(invoiceId);

        // Step 2: Convert HTML to PDF
        return await GeneratePdfFromHtmlAsync(htmlContent);
    }

    public async Task<string> GenerateInvoiceHtmlAsync(int invoiceId)
    {
        var invoice = await _invoicePdfAppService.GetAsync(invoiceId);

        var htmlContent = $@"
        <div class=""mud-modal"">
            <div class=""title-content d-flex justify-content-center align-items-center"">
                <h2 class=""fw-bold"">Invoice {invoice.InvoiceNumber}</h2>
            </div>
            <div class=""dialog-content"">
                <div class=""main-layout-paper d-flex flex-column p-3"" style=""width: 100%; height: 100%; margin-top: 20px;"">
                    <div class=""invoice-header d-flex flex-column flex-grow-1 h-100 p-3 justify-content-center align-items-center"">
                        <h3>***Simplified tax invoice***</h3>
                        <div class=""d-flex flex-row"">
                            <h5 class=""fw-bold"">VAT:</h5>
                            <h5>{invoice.GarageName}</h5>
                        </div>
                    </div>

                    <div class=""invoice-body d-flex flex-column justify-content-between"">
                        <h4>This is the receipt for payment of {invoice.InvoicePrice} SAR</h4>
                        <div class=""row"">
                            <div class=""col-md-6 d-flex flex-column"">
                                <strong>Payment Number:</strong><p>{invoice.InvoiceNumber}</p>
                            </div>
                            <div class=""col-md-6 d-flex flex-column align-items-center"">
                                <strong>Hotel Name:</strong><p>{invoice.OrganizationArabicName}</p>
                            </div>
                            <div class=""col-md-6 d-flex flex-column"">
                                <strong>Plate Number:</strong><p>{invoice.PlateNumber}</p>
                            </div>
                            <div class=""col-md-12 d-flex flex-column"">
                                <strong>Payment Date:</strong><p>{invoice.InvoiceDate.ToString("yyyy-MM-dd")}</p>
                            </div>
                        </div>
                    </div>

                    <div class=""total-info"" style=""margin-top: 2rem;"">
                        <div class=""row"">
                            <div class=""col-md-6"">
                                <div class=""d-flex flex-row"">
                                    <p><strong>Total Pre VAT:</strong> {invoice.InvoicePrice * 0.85} SAR</p>
                                </div>
                                <div class=""d-flex flex-row"">
                                    <p><strong>VAT 15%:</strong> {invoice.InvoicePrice * 0.15} SAR</p>
                                </div>
                                <div class=""d-flex flex-row"">
                                    <p><strong>Total Include VAT:</strong> {invoice.InvoicePrice} SAR</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>";

        return htmlContent;
    }
    private Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Create iTextSharp Document object
            Document document = new Document(PageSize.A4);

            // PdfWriter will write the PDF content into the MemoryStream
            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            // Open the document to write content
            document.Open();

            // Use XMLWorkerHelper to parse the HTML content
            using (StringReader sr = new StringReader(htmlContent))
            {
                // If you have specific external resources like CSS, use `baseUri` parameter to resolve them.
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr, Encoding.UTF8);
            }

            // Close the document
            document.Close();

            // Return the byte array of the generated PDF
            return ms.ToArray();
        }
    }
 
 
 
 
 */