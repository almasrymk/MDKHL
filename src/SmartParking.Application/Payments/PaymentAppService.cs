using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Payments;
//[Authorize]
public class PaymentAppService : CrudAppService<Payment, PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Package, int> _packageRepo;
    private readonly HttpClient _httpClient;

    public PaymentAppService(IRepository<Payment, Guid> repository , IRepository<Invoice, int> invoiceRepo,  IRepository<Package, int> packageRepo , IRepository<Customer, Guid> customerRepo, IDataFilter dataFilter, HttpClient httpClient)
    : base(repository)
    {
     
        _repository = repository;
        _customerRepo = customerRepo;
        _invoiceRepo = invoiceRepo;
        _packageRepo = packageRepo;
        _httpClient = httpClient;
    }



    public async Task<PaymentDto> CreateCheckoutPaymentAsync(CreateUpdatePaymentDto input)
    {
        var url = "https://eu-test.oppwa.com/v1/checkouts";

        var customer = await _customerRepo.GetQueryableAsync();
        input.CustomerId =  customer.Where(s=>s.IdentityUserId == CurrentUser.Id).Select(s=>s.Id).FirstOrDefault();

  
        var content = new StringContent(
            $"entityId={input.EntityId}&amount={input.Amount}&currency={input.Currency}&paymentType={input.PaymentType}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );


        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "OGFjN2E0Y2E5MjQ0ZjUzYzAxOTI0NzNkMjBlNDAzMTV8NXJDQjdzYUpQN2U3U1lrWiU6QjQ=");


        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseContent);


            var checkoutId = responseObject["id"]?.ToString();
            if (!string.IsNullOrEmpty(checkoutId))
            {
                input.CheckoutId = checkoutId;


                var payment = ObjectMapper.Map<CreateUpdatePaymentDto, Payment>(input);
                await _repository.InsertAsync(payment);


                return ObjectMapper.Map<Payment, PaymentDto>(payment);
            }
            else
            {
                throw new Exception("Checkout ID is missing in the response.");
            }
        }
        else
        {
            throw new Exception($"Payment request failed with status: {response.StatusCode}");
        }
    }

    public async Task<PaymentDto> GetPaymentStatus(string input, string entityId)
    {
        var url = $"https://eu-test.oppwa.com/v1/checkouts/{input}/payment?entityId={entityId}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "OGFjN2E0Y2E5MjQ0ZjUzYzAxOTI0NzNkMjBlNDAzMTV8NXJDQjdzYUpQN2U3U1lrWiU6QjQ=");

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


