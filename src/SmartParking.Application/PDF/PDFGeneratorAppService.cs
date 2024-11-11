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
                                <strong>Payment Date:</strong><p>{invoice.InvoiceTime.ToString()}</p>
                            </div>
                        </div>
                    </div>

                    <div class=""total-info"" style=""margin-top: 2rem;"">
                        <div class=""row"">
                            <div class=""col-md-6"">
                                <div class=""d-flex flex-row"">
                                    <p><strong>Total Pre VAT:</strong> {invoice.InvoicePrice * 0.85m} SAR</p>
                                </div>
                                <div class=""d-flex flex-row"">
                                    <p><strong>VAT 15%:</strong> {invoice.InvoicePrice * 0.15m} SAR</p>
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

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var htmlContent = await GenerateInvoiceHtmlAsync(invoiceId);
        var result = GeneratePdfFromHtmlAsync(htmlContent);
        return result;
    }

    public byte[] GeneratePdfFromHtmlAsync(string htmlContent)
    {

        var cssContent = @"
            .mud-modal { font-family: Arial, sans-serif; }
            .fw-bold { font-weight: bold; }
            .d-flex { display: flex; }
            .flex-column { flex-direction: column; }
            .align-items-center { align-items: center; }
            .justify-content-center { justify-content: center; }";
        try
        {
            // Using a MemoryStream to store the generated PDF
            using (var ms = new MemoryStream())
            {
                using (var document = new Document())
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    var cssBytes = System.Text.Encoding.UTF8.GetBytes(cssContent);
                    var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
                    using (var cssStream = new MemoryStream(cssBytes))
                    using (var htmlReader = new MemoryStream(htmlBytes))
                    {
                        // Parse HTML and CSS
                       XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlReader, cssStream);
                    }

                    document.Close();
                }
                Console.WriteLine(ms);
                return ms.ToArray();
            }
        }
        catch (DocumentException docEx)
        {
            Console.WriteLine("Document exception occurred: " + docEx.Message);
            throw;
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
        //After all of the PDF "stuff" above is done and closed but **before** we
        //close the MemoryStream, grab all of the active bytes from the stream

    }

        
}
