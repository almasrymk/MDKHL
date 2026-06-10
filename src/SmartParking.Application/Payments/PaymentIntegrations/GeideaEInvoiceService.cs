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
/// Implements the Geidea Pay-by-Invoice (eInvoice) REST API.
///
/// Flow overview:
///   1. <see cref="CreateInvoiceAsync"/>       → POST eInvoice — stores paymentIntentId.
///   2. <see cref="SendInvoiceByEmailAsync"/>   → POST .../sendByEmail — delivers link.
///      — or —
///      <see cref="SendInvoiceBySmsAsync"/>     → POST .../sendBySms — delivers link via SMS.
///   3. Customer pays via the Geidea Hosted Page linked in the email/SMS.
///   4. <see cref="UpdateInvoiceAsync"/>        → PUT eInvoice — update status/items if needed.
///   5. <see cref="GetInvoiceAsync"/>           → GET eInvoice — poll or verify final state.
///   6. <see cref="DeleteInvoiceAsync"/>        → DELETE eInvoice — cancel "Created" invoices.
///
/// Endpoint paths are read from paymentconfig.json → Providers:Geidea:{Mode}:Endpoints.
/// All endpoints use Basic Auth: Base64(MerchantPublicKey:ApiPassword).
/// </summary>
public class GeideaEInvoiceService : IGeideaEInvoiceService, ITransientDependency
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<GeideaOptions> _options;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeideaEInvoiceService(
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

    // ── IGeideaEInvoiceService ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<GeideaCreateInvoiceResponse> CreateInvoiceAsync(GeideaCreateInvoiceRequest request)
    {
        SetBasicAuthHeader();

        var json    = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(
            BuildUrl(Current.Endpoints.CreateInvoice), content);

        return await DeserializeAsync<GeideaCreateInvoiceResponse>(response);
    }

    /// <inheritdoc/>
    public async Task<GeideaUpdateInvoiceResponse> UpdateInvoiceAsync(GeideaUpdateInvoiceRequest request)
    {
        SetBasicAuthHeader();

        var json    = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync(
            BuildUrl(Current.Endpoints.UpdateInvoice), content);

        return await DeserializeAsync<GeideaUpdateInvoiceResponse>(response);
    }

    /// <inheritdoc/>
    public async Task<GeideaSendInvoiceByEmailResponse> SendInvoiceByEmailAsync(
        string paymentIntentId,
        GeideaSendInvoiceByEmailRequest request)
    {
        SetBasicAuthHeader();

        var json    = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = BuildUrl(Current.Endpoints.SendInvoiceByEmail,
            ("{paymentIntentId}", paymentIntentId));

        var response = await _http.PostAsync(url, content);

        return await DeserializeAsync<GeideaSendInvoiceByEmailResponse>(response);
    }

    /// <inheritdoc/>
    public async Task<GeideaSendInvoiceBySmsResponse> SendInvoiceBySmsAsync(
        string paymentIntentId,
        GeideaSendInvoiceBySmsRequest request)
    {
        SetBasicAuthHeader();

        var json    = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = BuildUrl(Current.Endpoints.SendInvoiceBySms,
            ("{paymentIntentId}", paymentIntentId));

        var response = await _http.PostAsync(url, content);

        return await DeserializeAsync<GeideaSendInvoiceBySmsResponse>(response);
    }

    /// <inheritdoc/>
    public async Task<GeideaGetInvoiceResponse> GetInvoiceAsync(string paymentIntentId)
    {
        SetBasicAuthHeader();

        var url = BuildUrl(Current.Endpoints.GetInvoice,
            ("{paymentIntentId}", paymentIntentId));

        var response = await _http.GetAsync(url);

        return await DeserializeAsync<GeideaGetInvoiceResponse>(response);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Geidea only allows deletion of invoices in "Created" status.
    /// Attempting to delete a "Sent" or "Paid" invoice will return a non-success response code.
    /// </remarks>
    public async Task<GeideaDeleteInvoiceResponse> DeleteInvoiceAsync(string paymentIntentId)
    {
        SetBasicAuthHeader();

        var url = BuildUrl(Current.Endpoints.DeleteInvoice,
            ("{paymentIntentId}", paymentIntentId));

        var response = await _http.DeleteAsync(url);

        return await DeserializeAsync<GeideaDeleteInvoiceResponse>(response);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Combines BaseUrl with an endpoint path read from config, then URL-encodes and
    /// substitutes any named placeholders (e.g. {paymentIntentId}).
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
    /// as required by all Geidea eInvoice endpoints.
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
                $"Geidea eInvoice API returned {(int)response.StatusCode}: {body}");

        return JsonSerializer.Deserialize<T>(body, _jsonOptions)
            ?? throw new InvalidOperationException(
                $"Geidea eInvoice API returned an empty or unparseable body: {body}");
    }
}
