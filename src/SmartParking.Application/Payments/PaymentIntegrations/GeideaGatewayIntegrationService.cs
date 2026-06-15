using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// IPaymentGateway implementation for Geidea Direct REST API with Geidea.js card tokenisation.
///
/// Flow overview:
///   1. <see cref="InitiateAsync"/>         → creates a Geidea session, stores sessionId as CheckoutId.
///   2. Browser                             → Geidea.js renders the card form using the sessionId.
///                                            Geidea.js internally handles 3DS2 — no server involvement needed.
///   3. Geidea redirects to callbackUrl     → browser sent to api/PaymentResult/geidea/callback
///                                            with orderId + merchantReferenceId + status.
///   4. <see cref="HandleCallbackAsync"/>   → resolves Payment by PaymentNumber (merchantReferenceId),
///                                            updates CheckoutId to orderId, marks Invoice Paid, calls ZATCA.
/// </summary>
public class GeideaGatewayIntegrationService : IPaymentGatewayIntegrationService, ITransientDependency
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<GeideaOptions> _options;
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IDataFilter _dataFilter;
    private readonly IZatcaTaxInvoiceAppService _zatcaTaxInvoiceAppService;
    private readonly ILogger<GeideaGatewayIntegrationService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeideaGatewayIntegrationService(
        HttpClient http,
        IOptionsMonitor<GeideaOptions> options,
        IRepository<Payment, Guid> repository,
        IPaymentAppService paymentAppService,
        IDataFilter dataFilter,
        IRepository<Invoice, int> invoiceRepo,
        IZatcaTaxInvoiceAppService zatcaTaxInvoiceAppService,
        ILogger<GeideaGatewayIntegrationService> logger)
    {
        _http = http;
        _options = options;
        _repository = repository;
        _paymentAppService = paymentAppService;
        _dataFilter = dataFilter;
        _invoiceRepo = invoiceRepo;
        _zatcaTaxInvoiceAppService = zatcaTaxInvoiceAppService;
        _logger = logger;
    }

    private GeideaEnvironmentOptions Current =>
        _options.CurrentValue.Mode == "Live"
            ? _options.CurrentValue.Live
            : _options.CurrentValue.Test;

    // ── IPaymentGateway ───────────────────────────────────────────────────────

    public GatewayKey Gateway => GatewayKey.Geidea;

    public bool SupportsMethod(PaymentMethod method) =>
        method is PaymentMethod.MADA or PaymentMethod.VISA or PaymentMethod.MASTERCARD;

    /// <summary>
    /// 1. Saves a pending Payment record (generates PaymentNumber).
    /// 2. Creates a Geidea session via POST /payment-intent/api/v2/direct/session.
    /// 3. Stores the sessionId as CheckoutId on the Payment record.
    /// Returns the sessionId so the Blazor page can initialise Geidea.js.
    /// </summary>
    public async Task<GatewayInitiateResult> InitiateAsync(PaymentCheckoutRequestDto request)
    {
        var paymentRecord = await _paymentAppService.SavePaymentRequest(
            request,
            PaymentStatus.Pending, 
            request.CustomerId,
            entityId: Current.MerchantPublicKey);

        request.PaymentNumber = paymentRecord.PaymentNumber;

        var sessionId = await CreateSessionAsync(request);

        await _paymentAppService.UpdatePaymentStausByPaymentNumberAsync(
            paymentRecord.PaymentNumber, sessionId);

        var callbackUrl = !string.IsNullOrEmpty(request.CallbackUrl)
            ? request.CallbackUrl
            : Current.CallbackUrl;

        var jsUrl = Current.BaseUrl.Replace("https://api.", "https://www.", StringComparison.OrdinalIgnoreCase).TrimEnd('/') + "/hpp/geideaCheckout.min.js";

        return GatewayInitiateResult.ForGeidea(
            sessionId,
            paymentRecord.PaymentNumber,
            Current.MerchantPublicKey,
            callbackUrl,
            jsUrl);
    }

    /// <summary>
    /// Verifies by orderId: looks up the Payment whose CheckoutId was updated to orderId
    /// after the callback ran, then finalises the record.
    /// </summary>
    public async Task<PaymentVerificationResult> VerifyAsync(string orderId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == orderId);
            if (payment is null)
                return PaymentVerificationResult.Failed($"No payment found for Geidea orderId: {orderId}");

            var paymentId = await FinalisePaymentAsync(payment);
            return PaymentVerificationResult.Success(paymentId);
        }
    }

    /// <summary>
    /// Called by PaymentResultController for GET/POST api/PaymentResult/geidea/callback.
    /// The controller parses Request.Query + Request.Form into the dictionary before calling here,
    /// keeping this class free of ASP.NET Core types.
    ///
    /// Expected keys: orderId, status, merchantReferenceId, responseCode, responseMessage
    /// </summary>
    public async Task<PaymentVerificationResult> HandleCallbackAsync(IReadOnlyDictionary<string, string> callbackParams)
    {
        callbackParams.TryGetValue("orderId",             out var orderId);
        callbackParams.TryGetValue("status",              out var status);
        callbackParams.TryGetValue("merchantReferenceId", out var merchantReferenceId);
        callbackParams.TryGetValue("responseCode",        out var responseCode);
        callbackParams.TryGetValue("responseMessage",     out var responseMessage);

        if (string.IsNullOrEmpty(orderId))
            return PaymentVerificationResult.Failed("Missing orderId in Geidea callback");

        if (!string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase))
            return PaymentVerificationResult.Failed(
                $"Geidea payment did not succeed. Status={status}, " +
                $"Code={responseCode}, Message={responseMessage}");

        if (string.IsNullOrEmpty(merchantReferenceId))
            return PaymentVerificationResult.Failed("Missing merchantReferenceId in Geidea callback");

        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Look up by PaymentNumber (= merchantReferenceId we set during session creation)
            var payment = await _repository.FirstOrDefaultAsync(
                p => p.PaymentNumber == merchantReferenceId);

            if (payment is null)
                return PaymentVerificationResult.Failed(
                    $"No payment record for merchantReferenceId: {merchantReferenceId}");

            // Overwrite the temporary sessionId with the permanent orderId for audit trail
            payment.CheckoutId = orderId;
            var paymentId = await FinalisePaymentAsync(payment);
            return PaymentVerificationResult.Success(paymentId);
        }
    }

    // ── Session creation ──────────────────────────────────────────────────────

    /// <summary>
    /// POST /payment-intent/api/v2/direct/session
    /// Returns the Geidea sessionId on success.
    /// </summary>
    public async Task<string> CreateSessionAsync(PaymentCheckoutRequestDto request)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var amountStr = request.Amount.ToString("F2", CultureInfo.InvariantCulture);

        var callbackUrl = !string.IsNullOrEmpty(request.CallbackUrl)
            ? request.CallbackUrl
            : Current.CallbackUrl;

        var body = new GeideaCreateSessionRequest
        {
            Amount           = request.Amount.ToString(),
            Currency         = Current.Currency,
            MerchantReferenceId = request.PaymentNumber,
            CallbackUrl      = callbackUrl,
            Language         = "en",
            Timestamp        = timestamp,
            Signature        = GenerateSignature(timestamp, amountStr, request.PaymentNumber),
            MerchantPublicKey = Current.MerchantPublicKey
        };

        var json = JsonSerializer.Serialize(body, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Geidea request is {@json}", json);

        SetBasicAuthHeader();

        //var response = await _http.PostAsync(
        //    Current.BaseUrl.TrimEnd('/') + Current.Endpoints.CreateSession, content);

        var url = $"{Current.BaseUrl.TrimEnd('/')}{Current.Endpoints.CreateSession}";

        // Create the request message explicitly
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        // SendAsync now receives the correct object type
        var response = await _http.SendAsync(req);

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Geidea response is {@responseBody}", responseBody);
        //if (!response.IsSuccessStatusCode)
        //    throw new InvalidOperationException(
        //        $"Geidea session API returned {(int)response.StatusCode}: {responseBody}");

        var sessionResponse = JsonSerializer.Deserialize<GeideaCreateSessionResponse>(
            responseBody, _jsonOptions);

        //if (sessionResponse?.ResponseCode != "000")
        //    throw new InvalidOperationException(
        //        $"Geidea session error [{sessionResponse?.ResponseCode}]: " +
        //        $"{sessionResponse?.ResponseMessage} — {sessionResponse?.DetailedResponseMessage}");

        return sessionResponse.Session?.Id
            ?? throw new InvalidOperationException(
                "Geidea returned success code but session.id was null");
    }

    // ── Signature ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Geidea HMAC-SHA256 signature:
    ///   message = MerchantPublicKey + timestamp + amount("F2") + currency
    ///   key     = ApiPassword
    ///   result  = Base64(HMAC-SHA256(message, key))
    /// </summary>
    private string GenerateSignature(string timestamp, string amount, string paymentNumber)
    {
        //var message = Current.MerchantPublicKey + timestamp + amount + Current.Currency;
        var message = $"{Current.MerchantPublicKey}{amount}{Current.Currency}{paymentNumber}{timestamp}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Current.ApiPassword));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    // ── Auth header ───────────────────────────────────────────────────────────

    private void SetBasicAuthHeader()
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{Current.MerchantPublicKey}:{Current.ApiPassword}"));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    // ── DB finalisation ───────────────────────────────────────────────────────

    private async Task<Guid> FinalisePaymentAsync(Payment payment)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
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
