using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments.PaymentIntegrations;

public class OppwaIntegrationAppService : IOppwaIntegrationAppService , ITransientDependency
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<OppwaRootOptions> _options;
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IDataFilter _dataFilter;


    private readonly Dictionary<string, string> PaymentEntityID = new Dictionary<string, string>
    {
        { "VISA", "8ac7a4ca9244f53c01924741733e0327" },
        { "MADA", "8ac7a4ca9244f53c019247402c950320" }
    };
    public OppwaIntegrationAppService(
        HttpClient http,
        IOptionsMonitor<OppwaRootOptions> options,
        IRepository<Payment, Guid> repository,
        IPaymentAppService paymentAppService,
        IDataFilter dataFilter,
        IRepository<Invoice, int> invoiceRepo)
    {
        _http = http;
        _options = options;
        _repository = repository;
        _paymentAppService = paymentAppService;
        _dataFilter = dataFilter;
        _invoiceRepo = invoiceRepo;
    }

    private OppwaEnvironmentOptions Current =>
        _options.CurrentValue.Mode == "Live"
            ? _options.CurrentValue.Live
            : _options.CurrentValue.Test;

    public async Task<string> PrepareCheckoutAsync(
        decimal amount,
        PaymentMethod brand)
    {
     //   var entityId = GetEntityIdByPaymentMethod(brand);


        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "entityId", Current.EntityId },
            { "amount", amount.ToString("F2", CultureInfo.InvariantCulture) },
            { "currency", Current.Currency },
            { "paymentType", Current.PaymentType }
        });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{Current.BaseUrl}/v1/checkouts");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                Current.Auth.TokenType,
                Current.Auth.Token);

        request.Content = content;

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json)
            .RootElement.GetProperty("id").GetString()!;
    }

    public async Task SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
       string checkoutid )
    {

        paymentCheckoutRequest.PaymentNumber = await _paymentAppService.GeneratePaymentNumberAsync(paymentCheckoutRequest.OrganizationId);
        Payment payment = new Payment
        {
            Amount = paymentCheckoutRequest.Amount,
            Currency = Current.Currency,
            PaymentMethod = paymentCheckoutRequest.PaymentMethod,
            InvoiceId = paymentCheckoutRequest.InvoiceId,
            CustomerPackageId = paymentCheckoutRequest.CustomerPackageId,
            PackageId = paymentCheckoutRequest.PackageId,
            OrganizationId = paymentCheckoutRequest.OrganizationId,
            PaymentCategory = paymentCheckoutRequest.PaymentCategory,
            CheckoutId = checkoutid,
            PaymentStatus = PaymentStatus.Pending,
            PaymentNumber = paymentCheckoutRequest.PaymentNumber,
            ArPlateNumber = paymentCheckoutRequest.ArPlateNumber,
            EnPlateNumber = paymentCheckoutRequest.EnPlateNumber,
            TenantId =  paymentCheckoutRequest.TenantId,

        };
        payment = await _repository.InsertAsync(payment);

    }
    public async Task<PaymentVerificationResult> VerifyHyperPayPaymentAsync(string resourcePath)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(Current.Auth.TokenType, Current.Auth.Token);

        var response = await client.GetAsync(
            $"{Current.BaseUrl}{resourcePath}?entityId={Current.EntityId}"
        );

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<HyperPayResultDto>(json);

        if (!IsSuccess(result.Result.Code))
            return PaymentVerificationResult.Failed();

        var checkoutId = ExtractCheckoutId(resourcePath);
        var paymentId =await UpdatePayment(checkoutId);

        return PaymentVerificationResult.Success(paymentId);
    }

    private string GetEntityIdByPaymentMethod(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.VISA => PaymentEntityID["VISA"],
            PaymentMethod.MASTERCARD => PaymentEntityID["VISA"],
            PaymentMethod.MADA => PaymentEntityID["MADA"],
            PaymentMethod.ApplePay => PaymentEntityID["MADA"],
            _ => throw new Exception("Payment method is not supported.")
        };
    }
    private bool IsSuccess(string code)
    {
        return code.StartsWith("000.000.")
            || code.StartsWith("000.100.1");
    }
    private static string ExtractCheckoutId(string resourcePath)
    {
        // resourcePath: /v1/checkouts/{checkoutId}/payment
        var parts = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // ["v1", "checkouts", "{checkoutId}", "payment"]
        return parts.Length >= 3 ? parts[2] : throw new Exception("Invalid resourcePath");
    }
    private async Task<Guid> UpdatePayment(string checkout)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == checkout);
            if (payment != null)
            {
                var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
                if (invoice != null)
                {
                    invoice.InvoiceType = InvoiceType.Paid;
                    invoice.InvoiceNumber=payment.PaymentNumber;
                    await _invoiceRepo.UpdateAsync(invoice);
                }
            }
            return payment.Id;
        }
    }

}
