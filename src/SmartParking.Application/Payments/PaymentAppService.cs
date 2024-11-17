using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments;
[Authorize]
public class PaymentAppService : CrudAppService<Payment, PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IOrganizationsAppService organizationsAppService;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Package, int> _packageRepo;
    private readonly HttpClient _httpClient;

    private const string EntityId = "8ac7a4ca9244f53c0192473d20e40315";
    private const string Currency = "SAR";
    private const string PaymentType = "ReadyUI";
    private const string PaymentUrl = "https://eu-test.oppwa.com/v1/checkouts";
    private const string token = "OGFjN2E0Y2E5MjQ0ZjUzYzAxOTI0NzNkMjBlNDAzMTV8NXJDQjdzYUpQN2U3U1lrWiU6QjQ=";
    private const string tokenType = "Bearer";

    private readonly Dictionary<string, string> PaymentEntityID = new Dictionary<string, string>
    {
        { "VISA", "8ac7a4ca9244f53c0192473d20e40315" },
        { "MADA", "8ac7a4ca9244f53c019247402c950320" }
    };

    public PaymentAppService(IRepository<Payment, Guid> repository,
        IRepository<Invoice, int> invoiceRepo, IRepository<Package, int> packageRepo,
        IRepository<Customer, Guid> customerRepo, IDataFilter dataFilter,
        IOrganizationsAppService _organizationsAppService,
        HttpClient httpClient)
    : base(repository)
    {

        _repository = repository;
        _customerRepo = customerRepo;
        organizationsAppService = _organizationsAppService;
        _invoiceRepo = invoiceRepo;
        _packageRepo = packageRepo;
        _httpClient = httpClient;
    }



    public async Task<PaymentCheckoutDto> CreateCheckoutPaymentAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {

        var customer = await _customerRepo.GetQueryableAsync();
        var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
        // ENABLE TO VALIDATE var organizationId = organizationsAppService.GetAsync(paymentCheckoutRequest.OrganizationId);

        if (customerId != null)
        {

            // handle cash transaction
            if (paymentCheckoutRequest.PaymentMethod == PaymentMethod.Cash)
            {
                return await SavePaymentRequest(paymentCheckoutRequest, customerId, PaymentStatus.Success);

            }
            else   //handle online transaction
            {

                var entityId = GetEntityIdByPaymentMethod(paymentCheckoutRequest.PaymentMethod);
                var checkoutId = await HyperPayPayment(paymentCheckoutRequest, entityId);

                return await SavePaymentRequest(paymentCheckoutRequest, customerId, PaymentStatus.Pending,
                    checkoutid: checkoutId, entityId: entityId);

            }





        }
        else
            throw new Exception("Customer not found");



    }

    private async Task<PaymentCheckoutDto> SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
        Guid customerId,
        PaymentStatus paymentStatus,
        string checkoutid = null,
        string entityId = null)
    {
        Payment payment = new Payment
        {
            Amount = paymentCheckoutRequest.Amount,
            Currency = Currency,
            PaymentMethod = paymentCheckoutRequest.PaymentMethod,
            InvoiceId = paymentCheckoutRequest.InvoiceId,
            PackageId = paymentCheckoutRequest.PackageId,
            OrganizationId = paymentCheckoutRequest.OrganizationId,
            PaymentCategory = paymentCheckoutRequest.PaymentCategory,
            CheckoutId = checkoutid,
            EntityId = entityId,
            CustomerId = customerId,
            PaymentStatus = paymentStatus
        };

        payment = await _repository.InsertAsync(payment);


        return new PaymentCheckoutDto
        {
            CheckoutId = payment.CheckoutId,
            PaymentId = payment.Id,
            PaymentStatus = payment.PaymentStatus,
            PaymentMethod = payment.PaymentMethod,
            EntityId = payment.EntityId,
            TokenId = token
        };



    }
    private async Task<string> HyperPayPayment(PaymentCheckoutRequestDto paymentCheckoutRequest, string entityId)
    {
        var content = new StringContent(
          $"entityId={entityId}&amount={paymentCheckoutRequest.Amount}&currency={Currency}&paymentType={PaymentType}",
          Encoding.ASCII,
          "application/x-www-form-urlencoded"
      );

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, token);


        var response = await _httpClient.PostAsync(PaymentUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Payment request failed with status: {response.StatusCode}");
        }


        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseContent);
        var checkoutId = responseObject["id"]?.ToString();
        if (string.IsNullOrEmpty(checkoutId))
        {
            throw new Exception("Unable to Get CheckOut ID");
        }
        return checkoutId;


    }



    private string GetEntityIdByPaymentMethod(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.VISA => PaymentEntityID["VISA"],
            PaymentMethod.MASTERCARD => PaymentEntityID["VISA"],
            PaymentMethod.MADA => PaymentEntityID["MADA"],
            _ => throw new Exception("Payment method is not supported.")
        };
    }

    public async Task<PaymentDto> GetPaymentStatus(string input, string entityId)
    {
        var url = $"{PaymentUrl}/{input}/payment?entityId={EntityId}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, token);

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseContent);

            var resultDescription = responseObject["result"]?["description"]?.ToString();
            var status = PaymentStatus.Cancelled;

            if (!string.IsNullOrEmpty(resultDescription))
            {
                if (resultDescription.Contains("pending", StringComparison.OrdinalIgnoreCase))
                {
                    status = PaymentStatus.Pending;
                }
                else if (resultDescription.Contains("successfuly", StringComparison.OrdinalIgnoreCase))
                {
                    status = PaymentStatus.Success;
                }
            }

            var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == input);

            if (payment != null)
            {
                if (payment.PaymentCategory == PaymentCategory.Invoice)
                {

                    var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.PaymentCategoryId);
                    if (invoice != null)
                    {
                        invoice.InvoiceType = status == PaymentStatus.Success ? InvoiceType.Paid : InvoiceType.UnPaid;
                        await _invoiceRepo.UpdateAsync(invoice);
                    }
                }
                else if (payment.PaymentCategory == PaymentCategory.Package)
                {

                    var package = await _packageRepo.FirstOrDefaultAsync(p => p.Id == payment.PaymentCategoryId);
                    if (package != null)
                    {
                        package.Status = status == PaymentStatus.Success ? Status.Active : Status.Inactive;
                        await _packageRepo.UpdateAsync(package);
                    }
                }

                payment.PaymentStatus = status;
                await _repository.UpdateAsync(payment);
            }

            return new PaymentDto
            {
                CheckoutId = payment.CheckoutId,
                CustomerId = payment.CustomerId,
                Id = payment.Id,
                PaymentStatus = status,
            };
        }
        else
        {
            throw new Exception($"Payment status request failed with status: {response.StatusCode}");
        }
    }


}


