using System;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SmartParking.ZATCATaxInvoice;
using SmartParking.Entities.Payments;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Customers;
using Castle.Core.Resource;
using System.Text.RegularExpressions;

namespace SmartParking.Utilities
{
    public class ZatcaTaxInvoiceAppService : SmartParkingAppService, IZatcaTaxInvoiceAppService
    {
        private readonly IRepository<Payment, Guid> _repository;
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ILogger<InvoiceUtilityAppService> _logger;
        private readonly IRepository<Organization, int> _organizationsRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceRepository"></param>
        /// <param name="logger"></param>
        /// <param name="organizationsRepository"></param>
        public ZatcaTaxInvoiceAppService(
            IRepository<Payment, Guid> repository,
            ILogger<InvoiceUtilityAppService> logger,
            IRepository<Customer, Guid> customerRepository,
            IRepository<Organization, int> organizationsRepository)
        {
            _repository = repository;
            _logger = logger;
            _organizationsRepository = organizationsRepository;
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Get Token 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetTokenAsync(string userName , string password)
        {
            JObject json = await SendPostRequest("https://zatca.mazeedit.com/api/Auth/Login", new { userName = userName /*"maden.tyba"*/, password = password/* "maden.tyba@P@ssw0rd123"*/ });
            var token = json["data"]?["token"]?.ToString();
            return "" + token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="oragnization"></param>
        /// <returns></returns>
        private InvoiceRequestDto CreateInvoiceRequestDto(Payment payment , Customer customer, Organization oragnization)
        {
            var AmountTax = decimal.Parse(string.Format("{0:0.00}", payment.Amount * 15 / 100));
            InvoiceRequestDto request = new InvoiceRequestDto();
            request.CompanyProfile = //ObjectMapper.Map<Organization, CompanyProfileDto>(oragnization!);
                new CompanyProfileDto
                {
                    Country = "SA",//oragnization.Country.ArabicName,
                    Region = oragnization.Region.ArabicName,
                    Governorate = oragnization.Country.ArabicCapital,
                    Address = oragnization.Address,
                    BuildingNumber = "" + oragnization.BuildingNumber,
                    PostalCode = oragnization.PostalCode,
                    CRN = oragnization.CommercialRegistrationNumber,
                    TaxVat = oragnization.TaxIdentificationNumber,
                    EnglishName = oragnization.ArabicName,                   
                };

            request.Invoice = // ObjectMapper.Map<Invoice, InvoiceDto>(invoice!);
                new InvoiceDto
                {
                    InvoiceTypeId = 1,
                    Code = Regex.Replace(payment.PaymentNumber, "[^0-9]", "") ,
                    Date = payment.CreationTime,
                    Currency = "SAR",
                    DealerName = customer.ArabicName,
                    ExchangeRate = 1,
                    VatPercentage = 0,
                    DiscountAmount = 0,
                    Total = payment.Amount ,
                    DealerTaxNumber = "",                   
                    InvoiceDetails = new List<InvoiceDetailDto>()
                };

            request.Invoice.InvoiceDetails.Add(new InvoiceDetailDto
            {
                Id = 1,
                ItemName = "حجز موقف",
                ItemPrice = payment.Amount,
                Discount = 0,
                Net =  payment.Amount ,
                Quantity = 1,
                UnitName = "سيارة",
                Vat = decimal.Parse(string.Format("{0:0.00}"  , payment.Amount * 15 / 100)),
                VatPercentage = 15
            });
             
            request.Address = new AddressDto
            {
                CountryName = "SA",
                RegionName = oragnization.Region?.ArabicName,
                BuildingNumber = (ushort)oragnization.BuildingNumber,
                PostalCode = oragnization.PostalCode,
                NameArabic = oragnization.Address
            };

            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> SendRequestForZACTA(Guid paymentId)
        {
            var payment = await _repository.FirstOrDefaultAsync(e => e.Id == paymentId);
            if (payment == null)
                throw new Exception("The Payment not found.");

            var oragnization = await _organizationsRepository.FirstOrDefaultAsync(e=>e.Id == payment.OrganizationId);
            if (oragnization == null)
                throw new Exception("The Oragnization not found.");

            var customer = await _customerRepository.FirstOrDefaultAsync(e => e.Id == payment.CustomerId);
            if (customer == null)
                customer = new Customer { ArabicName = "غير معرف" };

         var token = await GetTokenAsync(oragnization.UserNameZatca , oragnization.PasswordZatca);
            if (token == "400")
                throw new Exception("Username or password for Zatca not found.");

            InvoiceRequestDto request = CreateInvoiceRequestDto(payment! , customer, oragnization);

            var json = await SendPostRequest("https://zatca.mazeedit.com/Api/Invoices/Reporting", request, token);
            var invoiceQRCode = json["invoiceQRCode"]?.ToString();
            return "" + invoiceQRCode;
        }

        /// <summary>
        /// Get Request
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<JObject> SendPostRequest(string url, object content, string token = "")
        {
            try
            {           
            Uri baseUri = new Uri(url);
            HttpClient _httpClient = new HttpClient();   
                _httpClient.BaseAddress = baseUri;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.ConnectionClose = true;
            if ("" + token != null)
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var c = JsonConvert.SerializeObject(content);
            var _content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            HttpResponseMessage responseQR = await _httpClient.PostAsync(baseUri, _content);

            responseQR.EnsureSuccessStatusCode();
            var resultResponseQR = await responseQR.Content.ReadAsStringAsync();
            var jsonQR = JObject.Parse(resultResponseQR);
            return jsonQR;
            }
            catch (Exception ex)
            {
                var xx = ex.Message;
                throw;
            }
        }
    }
}