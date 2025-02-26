using Abp.Json;
using Microsoft.AspNetCore.Mvc;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
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
    private readonly IGarageTransactionsAppService GarageTransactionAppService;
    private readonly ICamerasAppService CameraAppService;
    private readonly IGarageAppService GarageAppService;

    public InvoiceUtilityAppService(IRepository<Organization, int> organizationsRepository, HttpClient httpClient, IRepository<Invoice, int> invoiceRepository, ILogger<InvoiceUtilityAppService> logger, IGarageTransactionsAppService garageTransactionAppService, ICamerasAppService cameraAppService, IGarageAppService garageAppService)
    {
        _organizationsRepository = organizationsRepository;
        _httpClient = httpClient;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        GarageTransactionAppService = garageTransactionAppService;
        CameraAppService = cameraAppService;
        GarageAppService = garageAppService;
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
            userName = "ibrahimia-1",
            password = "ibrahimiaP@ssw0rd"
        };

        JObject json = await CreateRequest(zatcaLogin, user);
        var token = json["data"]?["token"]?.ToString();
        return token;
    }

    public async Task<bool> OpenGate(OpenGateRequestDto camera, string GarageUrl) {
  
        var content = new StringContent(camera.ToJsonString(), Encoding.UTF8, "application/json");
        _logger.LogInformation($"Open Gate Request body => {camera.ToJsonString()}");
        _logger.LogInformation($"Open Gate Request Header => {content.Headers.ToJsonString()}");
        var response = await _httpClient.PostAsync(GarageUrl, content);
        _logger.LogInformation($"Open Gate Response body => {response.Headers.ToJsonString()}");
        return response.IsSuccessStatusCode;

    }

    public async Task<bool> SendRequestToOpenGate(int ExitGarageTransactionId)
    {

        var cameraId = await GarageTransactionAppService.GetCameraIdByGarageTransactionId(ExitGarageTransactionId);
        var camera = await CameraAppService.GetCameraIPByCameraId(cameraId);
        var openGateBody = new OpenGateRequestDto
        {
            ip = camera.CameraIP,
            port = 80,
            username = camera.UserName ?? "",
            password = camera.Password ?? ""
        };
        return await OpenGate(openGateBody,camera.Garage.OpenGateURL);
    }
}
