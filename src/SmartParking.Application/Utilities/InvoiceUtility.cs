using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Tls;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Utilities;

public class InvoiceUtility : IInvoiceUtility
{
    private readonly IRepository<Organization, int> _organizationsRepository;
    private readonly HttpClient _httpClient;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private string zatcaInvoices = "https://zatca.mazeedit.com/api/Invoices/V1/QRCode";
    private string zatcaLogin = "https://zatca.mazeedit.com/api/Auth/Login";
    public InvoiceUtility(IRepository<Organization, int> organizationsRepository, HttpClient httpClient, IRepository<Invoice, int> invoiceRepository)
    {
        _organizationsRepository = organizationsRepository;
        _httpClient = httpClient;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId)
    {
        var invoice = await _invoiceRepository.GetAsync(i => i.Id == invoiceId);
        var oragnization = await _organizationsRepository.GetAsync(invoice.OrganizationId);
        return await GetQrCodeAsync(invoice.InvoicePrice, invoice.OrganizationId);
    }



    public async Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId)
    {

        var oragnization = await _organizationsRepository.GetAsync(organizationId);
        //"12-25-2024"
        return await GetQrCodeAsync(amount, oragnization);
    }

    public async Task<QRCodeDto> GetQrCodeAsync(decimal amount, Organization organization)
    {
        
        var qR = new
        {
            sellerName = organization.EnglishName,
            vatRegistration = organization.TaxIdentificationNumber,
            timeStamp = DateTime.Now.ToString("MM-dd-yyyy"),
            invoiceAmount = amount.ToString(),
            vatAmount = (amount * 0.15M).ToString()
        };
        return await GetZatkaCode(qR);
    }

    private async Task<QRCodeDto> GetZatkaCode(object qR)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        JObject jsonQR = await CreateRequest(zatcaInvoices, qR);
        var qrCodedto = new QRCodeDto
        {
            QRCode = jsonQR["qrCode"]?.ToString()
        };
        return qrCodedto;
    }

    private async Task<JObject> CreateRequest(string path, object qR)
    {
        var contentQR = new StringContent(JsonConvert.SerializeObject(qR), Encoding.UTF8, "application/json");
        HttpResponseMessage responseQR = await _httpClient.PostAsync(path, contentQR);

        responseQR.EnsureSuccessStatusCode();

        var resultResponseQR = await responseQR.Content.ReadAsStringAsync();

        var jsonQR = JObject.Parse(resultResponseQR);
        return jsonQR;
    }

    public async Task<string?> GetTokenAsync()
    {
        var user = new
        {
            userName = "ibrahimia-1",
            password = "ibrahimiaP@ssw0rd"
        };

        JObject json = await CreateRequest(zatcaLogin, user);
        var token = json["data"]?["token"]?.ToString();
        return token;
    }
}
