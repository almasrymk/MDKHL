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
using SmartParking.Results;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Organizations;
using SmartParking.Cutomers;
using SmartParking.Localization;
using SmartParking.Entities.Invoices;

namespace SmartParking.Utilities
{
    public class ZatcaTaxInvoiceAppService : SmartParkingAppService, IZatcaTaxInvoiceAppService
    {
        private readonly IRepository<Payment, Guid> _repository;
        private readonly ICustomerAppService _customerRepository;
        private readonly ILogger<InvoiceUtilityAppService> _logger;
        private readonly IOrganizationsAppService _organizationsRepository;
        private readonly IRepository<Invoice, int> _invoiceRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceRepository"></param>
        /// <param name="logger"></param>
        /// <param name="organizationsRepository"></param>
        public ZatcaTaxInvoiceAppService(
            IRepository<Payment, Guid> repository,
            ILogger<InvoiceUtilityAppService> logger,
            ICustomerAppService customerRepository,
            IOrganizationsAppService organizationsRepository,
            IRepository<Invoice, int> invoiceRepository)
        {
            _repository = repository;
            _logger = logger;
            _organizationsRepository = organizationsRepository;
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Get Token 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetTokenAsync(string userName, string password)
        {
            JObject json = await SendPostRequest("https://zatca.mazeedit.com/api/Auth/Login", new { userName = userName, password = password });
            var token = json["data"]?["token"]?.ToString();
            return "" + token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="oragnization"></param>
        /// <returns></returns>
        private InvoiceRequestDto CreateInvoiceRequestDto(decimal Amount, decimal Discount, Payment payment, CustomerDto customer, OrganizationDto oragnization)
        {
            var vat = Math.Round(Amount * (decimal)0.15, 2);
            var NetAmount = Amount + vat - Discount;

            InvoiceRequestDto request = new InvoiceRequestDto();
            request.CompanyProfile = //ObjectMapper.Map<Organization, CompanyProfileDto>(oragnization!);
                new CompanyProfileDto
                {
                    Country = "SA",//oragnization.Country.ArabicName,
                    Region = oragnization.RegionNameAr,
                    Governorate = oragnization.CityNameAr,
                    Address = oragnization.Address,
                    BuildingNumber = "" + oragnization.BuildingNumber,
                    PostalCode = oragnization.PostalCode,
                    CRN = oragnization.CommercialRegistrationNumber,
                    TaxVat = oragnization.TaxNumber,
                    EnglishName = oragnization.ArabicName,
                };

            request.Invoice = // ObjectMapper.Map<Invoice, InvoiceDto>(invoice!);
                new InvoiceDto
                {
                    InvoiceTypeId = 2,
                    Code = Regex.Replace(payment.PaymentNumber, "[^0-9]", ""),
                    Date = payment.CreationTime,
                    Currency = "SAR",
                    DealerName = customer.ArabicName,
                    ExchangeRate = 1,
                    VatPercentage = 15,
                    DiscountAmount = Discount,
                    Total = Amount,                    
                    DealerTaxNumber = "",
                    InvoiceDetails = new List<InvoiceDetailDto>()
                };

            request.Invoice.InvoiceDetails.Add(new InvoiceDetailDto
            {
                Id = 1,
                ItemName = "حجز موقف",
                ItemPrice = Amount,
                Discount = 0,
                Net = Amount,
                Quantity = 1,
                UnitName = "سيارة",
                Vat = vat,
                VatPercentage = 15
            });

            request.Address = new AddressDto
            {
                CountryName = "SA",
                RegionName = oragnization.RegionNameAr,
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
        public async Task<ResponseInvoice> SendToZACTA(Guid paymentId)
        {
            decimal Amount = 0, Discount = 0;
            ResponseInvoice response = new ResponseInvoice();
            var payment = await _repository.FirstOrDefaultAsync(e => e.Id == paymentId);
            if (payment == null)
            {
                response.generalError += LocalizationHelper.GetLocalizedString("ThePaymentNotFound");
                return response;
            }

            var oragnization = await _organizationsRepository.GetAsync(payment.OrganizationId);
            if (oragnization == null)
            {
                response.generalError += LocalizationHelper.GetLocalizedString("TheOragnizationNotFound");
                return response;
            }

            if ("" + oragnization.UserNameZatca == "" || "" + oragnization.PasswordZatca == "")
            {
                response.generalError += LocalizationHelper.GetLocalizedString("UsernameOrPasswordForZatcaNotFound");
                return response;
            }

            var customer = new CustomerDto { ArabicName = "غير معرف" };
            if (payment.CustomerId != null)
                customer = await _customerRepository.GetAsync(payment.CustomerId.Value);
            if (customer == null)
                customer = new CustomerDto { ArabicName = "غير معرف" };

            var token = await GetTokenAsync(oragnization.UserNameZatca, oragnization.PasswordZatca);
            if (token == "400" || "" + token == "")
            {
                response.generalError += LocalizationHelper.GetLocalizedString("UsernameOrPasswordForZatcaNotFound");
                return response;
            }

            Amount = payment.Amount;

            if (payment.InvoiceId > 0)
            {
                var invoice = await _invoiceRepository.GetAsync(payment.InvoiceId.Value);
                if (invoice != null)
                {
                    Amount = invoice.InvoicePrice;
                    Discount = invoice.Discount;
                }
            }

            InvoiceRequestDto request = CreateInvoiceRequestDto(Amount, Discount, payment!, customer, oragnization);
            response = await SendPostRequest("https://zatca.mazeedit.com/Api/Invoices/Reporting", request, token);
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ResponseInvoice> SendPostRequest(string url, object content, string token = "")
        {
            try
            {
                ResponseInvoice response = new ResponseInvoice();
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
                var resultResponseQR = await responseQR.Content.ReadAsStringAsync();

                responseQR.EnsureSuccessStatusCode();
                response = JsonConvert.DeserializeObject<ResponseInvoice>(resultResponseQR)!;



                //if (responseQR.IsSuccessStatusCode)
                //    response = JsonConvert.DeserializeObject<ResponseInvoice>(resultResponseQR)!;
                //else
                //{
                //    response = JsonConvert.DeserializeObject<ResponseInvoice>(resultResponseQR)!;
                //    response.status = false;
                //    var errorResult = JsonConvert.DeserializeObject<ValidationErrorResponseDto>(resultResponseQR);

                //    response.validationResults = new ValidationErrorResponseDto { Errors = new Dictionary<string, List<string>>() };
                //    foreach (var error in errorResult?.Errors!)
                //        response.validationResults.Errors.Add(error.Key, error.Value);
                //}
                return response!;
            }
            catch (Exception ex)
            {
                var xx = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<JObject> SendPostRequest(string url, object content)
        {
            try
            {
                Uri baseUri = new Uri(url);
                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = baseUri;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.ConnectionClose = true;
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