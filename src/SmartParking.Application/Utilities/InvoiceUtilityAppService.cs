using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Tls;
using SmartParking.Cameras;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;

namespace SmartParking.Utilities;

public class InvoiceUtilityAppService : SmartParkingAppService, IInvoiceUtilityAppService
{
    private readonly IRepository<Organization, int> _organizationsRepository;
    private readonly HttpClient _httpClient;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private string zatcaInvoices = "https://zatca.mazeedit.com/api/Invoices/V1/QRCode";
    private string zatcaLogin = "https://zatca.mazeedit.com/api/Auth/Login";
    private string openGate = "http://mdkhl.mazeedit.com/Barrierer/Open";
    private readonly ILogger<InvoiceUtilityAppService> _logger;


    public InvoiceUtilityAppService(IRepository<Organization, int> organizationsRepository, HttpClient httpClient, IRepository<Invoice, int> invoiceRepository, ILogger<InvoiceUtilityAppService> logger)
    {
        _organizationsRepository = organizationsRepository;
        _httpClient = httpClient;
        _invoiceRepository = invoiceRepository;
        _logger = logger;

    }

    public async Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId)
    {
        var invoice = await _invoiceRepository.GetAsync(i => i.Id == invoiceId);
        var oragnization = await _organizationsRepository.GetAsync(invoice.OrganizationId);
        return await GetQrCodeAsync(invoice.InvoicePrice, invoice.OrganizationId);
    }

    public async Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId)
    {
        using (DataFilter.Disable<IMultiTenant>()) {

            var oragnization = await _organizationsRepository.FirstOrDefaultAsync(_ => _.Id==organizationId);
            //"12-25-2024"
            return await GetQrCodeAsync(amount, oragnization);
        }
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
        _logger.LogInformation($"{DateTime.Now} ====> path | {path}");
        _logger.LogInformation($"{DateTime.Now} ====> body | {contentQR.ToJsonString()}");

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
            userName = "maden.tyba",
            password = "maden.tyba@P@ssw0rd123"
        };

        JObject json = await CreateRequest(zatcaLogin, user);
        var token = json["data"]?["token"]?.ToString();
        return token;
    }

}
