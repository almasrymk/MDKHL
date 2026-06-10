using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Implements Geidea post-payment operational endpoints for exception handling
/// and customer-service use-cases (refunds, server-side order verification).
///
/// Endpoints covered:
///   • <see cref="RefundPaymentAsync"/>  → POST .../refund
///   • <see cref="GetOrderStatusAsync"/> → GET  .../orders/{orderId}
///
/// Endpoint paths are read from paymentconfig.json → Providers:Geidea:{Mode}:Endpoints.
/// All endpoints use Basic Auth: Base64(MerchantPublicKey:ApiPassword).
/// </summary>
public class GeideaPaymentManagementService : IGeideaPaymentManagementService, ITransientDependency
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<GeideaOptions> _options;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeideaPaymentManagementService(
        HttpClient http,
        IOptionsMonitor<GeideaOptions> options)
    {
        _http = http;
        _options = options;
    }

    // ── Configuration shortcut ────────────────────────────────────────────────

    private GeideaEnvironmentOptions Current =>
        _options.CurrentValue.Mode == "Live"
            ? _options.CurrentValue.Live
            : _options.CurrentValue.Test;

    // ── IGeideaPaymentManagementService ───────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// For a full refund leave <see cref="GeideaRefundRequest.RefundAmount"/> null.
    /// For a partial refund set it to the desired amount — it must not exceed the
    /// original captured amount.
    /// Only orders in a "Success" / captured status are eligible for refund.
    /// </remarks>
    public async Task<GeideaRefundResponse> RefundPaymentAsync(GeideaRefundRequest request)
    {
        SetBasicAuthHeader();

        var json    = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(
            BuildUrl(Current.Endpoints.Refund), content);

        return await DeserializeAsync<GeideaRefundResponse>(response);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This is the recommended server-to-server verification method.
    /// It is independent of the frontend callback and should be used:
    /// <list type="bullet">
    ///   <item>When verifying a payment for reconciliation purposes.</item>
    ///   <item>When a callback delivery cannot be guaranteed (e.g. network drop).</item>
    ///   <item>When the frontend callback status must be confirmed before settlement.</item>
    /// </list>
    /// </remarks>
    public async Task<GeideaOrderStatusResponse> GetOrderStatusAsync(string orderId)
    {
        SetBasicAuthHeader();

        var url = BuildUrl(Current.Endpoints.GetOrder,
            ("{orderId}", orderId));

        var response = await _http.GetAsync(url);

        return await DeserializeAsync<GeideaOrderStatusResponse>(response);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Combines BaseUrl with an endpoint path read from config, then URL-encodes and
    /// substitutes any named placeholders (e.g. {orderId}).
    /// </summary>
    private string BuildUrl(string endpointPath, params (string placeholder, string value)[] substitutions)
    {
        foreach (var (placeholder, value) in substitutions)
            endpointPath = endpointPath.Replace(placeholder, Uri.EscapeDataString(value),
                StringComparison.Ordinal);

        return Current.BaseUrl.TrimEnd('/') + endpointPath;
    }

    /// <summary>
    /// Sets the Authorization header using Basic Auth (Base64 of PublicKey:ApiPassword)
    /// as required by all Geidea payment management endpoints.
    /// </summary>
    private void SetBasicAuthHeader()
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{Current.MerchantPublicKey}:{Current.ApiPassword}"));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    /// <summary>
    /// Reads the HTTP response body and deserialises it to <typeparamref name="T"/>.
    /// Throws <see cref="InvalidOperationException"/> if the HTTP status is not a success code.
    /// </summary>
    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Geidea Payment Management API returned {(int)response.StatusCode}: {body}");

        return JsonSerializer.Deserialize<T>(body, _jsonOptions)
            ?? throw new InvalidOperationException(
                $"Geidea Payment Management API returned an empty or unparseable body: {body}");
    }
}
