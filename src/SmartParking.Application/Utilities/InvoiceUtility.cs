using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        decimal percentage = invoice.InvoicePrice * 0.14m;
        var oragnization = await _organizationsRepository.GetAsync(invoice.OrganizationId);
        return await GetQrCodeAsync(invoice.InvoicePrice, invoice.OrganizationId);
    }



    public async Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId)
    {

        var oragnization = await _organizationsRepository.GetAsync(organizationId);
        //"12-25-2024"
        var qR = new
        {
            sellerName = oragnization.EnglishName,
            vatRegistration = oragnization.CommercialRegistrationNumber,
            timeStamp = DateTime.Now.ToString("MM-dd-yyyy"),
            invoiceAmount = amount.ToString(),
            vatAmount = amount * 0.15M
        };
        return await GetZatkaCode(qR);
    }

    private async Task<QRCodeDto> GetZatkaCode(object qR)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var contentQR = new StringContent(JsonConvert.SerializeObject(qR), Encoding.UTF8, "application/json");
        HttpResponseMessage responseQR = await _httpClient.PostAsync(zatcaInvoices, contentQR);

        responseQR.EnsureSuccessStatusCode();

        var resultResponseQR = await responseQR.Content.ReadAsStringAsync();

        var jsonQR = JObject.Parse(resultResponseQR);
        var qrCodedto = new QRCodeDto
        {
            QRCode = jsonQR["qrCode"]?.ToString()
        };
        return qrCodedto;
    }

    public async Task<string?> GetTokenAsync()
    {
        var user = new
        {
            userName = "ibrahimia-1",
            password = "ibrahimiaP@ssw0rd"
        };
        var contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(zatcaLogin, contentUser);

        response.EnsureSuccessStatusCode();

        var resultResponse = await response.Content.ReadAsStringAsync();

        var json = JObject.Parse(resultResponse);
        var token = json["data"]?["token"]?.ToString();
        return token;
    }
}
