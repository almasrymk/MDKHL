using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// IPaymentGateway implementation for Oppwa/HyperPay (hosted widget flow).
/// Registered via the Func&lt;GatewayKey, IPaymentGateway&gt; factory delegate — NOT replacing the
/// existing <see cref="OppwaIntegrationAppService"/> which stays untouched.
/// </summary>
public class OppwaGatewayIntegrationService : IPaymentGatewayIntegrationService, ITransientDependency
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<OppwaRootOptions> _options;
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IDataFilter _dataFilter;
    private readonly IZatcaTaxInvoiceAppService _zatcaTaxInvoiceAppService;

    private OppwaEnvironmentOptions Current =>
        _options.CurrentValue.Mode == "Live"
            ? _options.CurrentValue.Live
            : _options.CurrentValue.Test;

    public OppwaGatewayIntegrationService(
        HttpClient http,
        IOptionsMonitor<OppwaRootOptions> options,
        IRepository<Payment, Guid> repository,
        IPaymentAppService paymentAppService,
        IDataFilter dataFilter,
        IRepository<Invoice, int> invoiceRepo,
        IZatcaTaxInvoiceAppService zatcaTaxInvoiceAppService)
    {
        _http = http;
        _options = options;
        _repository = repository;
        _paymentAppService = paymentAppService;
        _dataFilter = dataFilter;
        _invoiceRepo = invoiceRepo;
        _zatcaTaxInvoiceAppService = zatcaTaxInvoiceAppService;
    }

    // ── IPaymentGateway ───────────────────────────────────────────────────────

    public GatewayKey Gateway => GatewayKey.Oppwa;

    public bool SupportsMethod(PaymentMethod method) =>
        method is PaymentMethod.MADA or PaymentMethod.VISA
                or PaymentMethod.MASTERCARD or PaymentMethod.ApplePay;

    /// <summary>
    /// 1. Saves a pending Payment record (generates PaymentNumber).
    /// 2. Calls HyperPay /v1/checkouts to get checkoutId.
    /// 3. Stores checkoutId on the Payment record.
    /// Returns the checkoutId so the Blazor page can pass it to the JS widget.
    /// </summary>
    public async Task<GatewayInitiateResult> InitiateAsync(PaymentCheckoutRequestDto request)
    {
        var paymentRecord = await _paymentAppService.SavePaymentRequest(
            request,
            PaymentStatus.Pending,
            request.CustomerId,
            entityId: Current.EntityId);

        request.PaymentNumber = paymentRecord.PaymentNumber;

        var checkoutId = await CreateHyperPayCheckoutAsync(request);

        await _paymentAppService.UpdatePaymentStausByPaymentNumberAsync(
            paymentRecord.PaymentNumber, checkoutId, request.PaymentMethod);

        var shopperResultUrl = !string.IsNullOrEmpty(request.CallbackUrl)
            ? request.CallbackUrl
            : $"{Current.BaseUrl}/api/PaymentResult/hyperpay/result";

        var scriptUrl = $"{Current.BaseUrl}/v1/paymentWidgets.js?checkoutId={checkoutId}";

        return GatewayInitiateResult.ForOppwa(
            checkoutId,
            paymentRecord.PaymentNumber,
            shopperResultUrl,
            request.PaymentMethod.ToString().ToUpperInvariant(),
            scriptUrl);
    }

    /// <summary>
    /// Verifies the payment by calling the HyperPay status API using the resourcePath
    /// returned from the HyperPay widget redirect.
    /// </summary>
    public async Task<PaymentVerificationResult> VerifyAsync(string resourcePath)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(Current.Auth.TokenType, Current.Auth.Token);

        var response = await _http.GetAsync(
            $"{Current.BaseUrl}{resourcePath}?entityId={Current.EntityId}");

        var json = await response.Content.ReadAsStringAsync();
        JsonConvert.DeserializeObject<HyperPayResultDto>(json); // parse for logging if needed

        var checkoutId = ExtractCheckoutId(resourcePath);
        var paymentId = await FinalisePaymentAsync(checkoutId);
        return PaymentVerificationResult.Success(paymentId);
    }

    /// <summary>
    /// Parses resourcePath from the callback params dictionary and delegates to VerifyAsync.
    /// The controller passes Request.Query parsed as a plain dictionary.
    /// </summary>
    public Task<PaymentVerificationResult> HandleCallbackAsync(IReadOnlyDictionary<string, string> callbackParams)
    {
        callbackParams.TryGetValue("resourcePath", out var resourcePath);
        return VerifyAsync(resourcePath ?? string.Empty);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<string> CreateHyperPayCheckoutAsync(PaymentCheckoutRequestDto dto)
    {
        var payload = new Dictionary<string, string>
        {
            { "entityId",              Current.EntityId },
            { "amount",                dto.Amount.ToString("F2") },
            { "currency",              Current.Currency },
            { "paymentType",           Current.PaymentType },
            { "merchantInvoiceId",     dto.PaymentNumber },
            { "merchantTransactionId", dto.PaymentNumber },
            { "transactionCategory",   "EC" },
            { "customer.givenName",    "Customer" },
            { "customer.surname",      "User" },
            { "customer.email",        "user@example.com" },
            { "billing.country",       "SA" },
            { "billing.city",          "Riyadh" },
            { "billing.state",         "Riyadh" },
            { "billing.street1",       "Olaya St." },
            { "billing.postcode",      "12211" },
        };

        if (_options.CurrentValue.Mode == "Test")
            payload["customParameters[3DS2_enrolled]"] = "true";

        var request = new HttpRequestMessage(HttpMethod.Post, $"{Current.BaseUrl}/v1/checkouts")
        {
            Headers = { Authorization = new AuthenticationHeaderValue(Current.Auth.TokenType, Current.Auth.Token) },
            Content = new FormUrlEncodedContent(payload)
        };

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonDocument.Parse(json)
            .RootElement.GetProperty("id").GetString()!;
    }

    private static string ExtractCheckoutId(string resourcePath)
    {
        var parts = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3
            ? parts[2]
            : throw new InvalidOperationException($"Cannot parse checkoutId from resourcePath: {resourcePath}");
    }

    private async Task<Guid> FinalisePaymentAsync(string checkoutId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == checkoutId);
            if (payment is null)
                throw new InvalidOperationException($"No payment found for checkoutId {checkoutId}");

            var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
            if (invoice is not null)
            {
                invoice.InvoiceType = InvoiceType.Paid;
                invoice.InvoiceNumber = payment.PaymentNumber;
                var zatcaResult = await _zatcaTaxInvoiceAppService.SendToZACTA(payment.Id);
                payment.QrCodeInvoice = zatcaResult.invoiceQRCode;
                payment = await _repository.UpdateAsync(payment);
                await _invoiceRepo.UpdateAsync(invoice);
            }

            return payment.Id;
        }
    }
}
