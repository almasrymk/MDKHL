using Microsoft.AspNetCore.Mvc;
using SmartParking.Payments.PaymentIntegrations;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

/// <summary>
/// Handles payment gateway callbacks for all registered gateways.
///
/// Oppwa/HyperPay:  GET  api/PaymentResult/hyperpay/result?resourcePath=...
/// Geidea:          GET  api/PaymentResult/geidea/callback?orderId=...&amp;status=...&amp;merchantReferenceId=...
///                  POST api/PaymentResult/geidea/callback  (form-encoded fallback)
///
/// The existing IOppwaIntegrationAppService dependency is preserved on the Oppwa endpoint
/// for zero-breakage compatibility.  The Geidea endpoint resolves the gateway via the
/// Func&lt;GatewayKey, IPaymentGateway&gt; factory — no concrete Application types referenced here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentResultController : ControllerBase
{
    private readonly IOppwaIntegrationAppService _oppwaLegacyService;
    private readonly Func<GatewayKey, IPaymentGatewayIntegrationService> _gatewayFactory;

    public PaymentResultController(
        IOppwaIntegrationAppService oppwaLegacyService,
        Func<GatewayKey, IPaymentGatewayIntegrationService> gatewayFactory)
    {
        _oppwaLegacyService = oppwaLegacyService;
        _gatewayFactory = gatewayFactory;
    }

    // ── Oppwa / HyperPay ──────────────────────────────────────────────────────

    /// <summary>
    /// Called by the HyperPay widget after payment completes.
    /// Preserves the original behaviour 100%.
    /// </summary>
    [HttpGet("hyperpay/result")]
    public async Task<IActionResult> HyperPayResult([FromQuery] string resourcePath)
    {
        var result = await _oppwaLegacyService.VerifyHyperPayPaymentAsync(resourcePath);

        if (!result.IsSuccess)
            return Redirect("/payment-failed");

        var payment = await _oppwaLegacyService.HandlePayment(result.PaymentId);

        if (!payment.IsSuccess || payment.Value == default)
            return Redirect("/payment-failed");

        if (payment.Value == 0)
            return Ok();

        return Redirect($"/custom-invoice/details/{result.PaymentId}");
    }

    // ── Geidea ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Geidea redirects the browser here after the card form / 3DS2 flow completes.
    /// Accepts both GET (query params) and POST (form body).
    ///
    /// We parse all params into a plain dictionary here so the gateway class
    /// stays free of Microsoft.AspNetCore.Http dependencies.
    /// </summary>
    //[HttpGet("geidea/callback")]
    [HttpPost("geidea/callback")]
    public async Task<IActionResult> GeideaCallback([FromBody] GeideaCallbackPayload payload)
    {


        // 1. Guard check for bad parsing
        if (payload?.Order == null)
        {
            Console.WriteLine("[Geidea Callback] Received empty body or incorrect format.");
            return BadRequest("Invalid payload format.");
        }

        // Temporary debug log to confirm you got the data
        Console.WriteLine($"[Geidea Callback] Received Order ID: {payload.Order.OrderId}");
        Console.WriteLine($"[Geidea Callback] Status: {payload.Order.Status} | Detailed: {payload.Order.DetailedStatus}");

        // Temporary debug log
        var queryKeys = string.Join(", ", Request.Query.Keys);
        var formKeys = Request.HasFormContentType
                        ? string.Join(", ", Request.Form.Keys)
                        : "no form";

        Console.WriteLine($"[Geidea Callback] Query keys: {queryKeys}");
        Console.WriteLine($"[Geidea Callback] Form keys: {formKeys}");

        ///////////////////////////////////////////////////////////////////////////////////////
        IReadOnlyDictionary<string, string> callbackParams;

        try
        {
            // Merge query-string + form fields (query-string wins on collision)
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in Request.Query)
                dict[key] = value.ToString();

            if (Request.HasFormContentType)
                foreach (var (key, value) in Request.Form)
                    dict.TryAdd(key, value.ToString());

            // Merge payload data into the dictionary
            if (payload?.Order != null)
            {
                dict.TryAdd("orderId", payload.Order.OrderId);
                dict.TryAdd("merchantReferenceId", payload.Order.MerchantReferenceId);
                dict.TryAdd("amount", payload.Order.Amount.ToString());
                dict.TryAdd("currency", payload.Order.Currency);
                dict.TryAdd("status", payload.Order.Status);
                dict.TryAdd("detailedStatus", payload.Order.DetailedStatus);
                dict.TryAdd("updatedDate", payload.Order.UpdatedDate);
            }

            if (!string.IsNullOrEmpty(payload?.Signature))
                dict.TryAdd("signature", payload.Signature);

            callbackParams = dict;
        }
        catch
        {
            return Redirect("/payment-failed?reason=parse_error");
        }

        PaymentVerificationResult result;
        try
        {
            var gateway = _gatewayFactory(GatewayKey.Geidea);
            result = await gateway.HandleCallbackAsync(callbackParams);
        }
        catch (Exception)
        {
            return Redirect("/payment-failed?reason=geidea_exception");
        }

        if (!result.IsSuccess)
            return Redirect($"/payment-failed?reason={Uri.EscapeDataString(result.ErrorMessage ?? "unknown")}");

        return Redirect($"/custom-invoice/details/{result.PaymentId}");
    }

    // ── Paymob ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Paymob callback skeleton endpoint.
    /// </summary>
    [HttpGet("paymob/callback")]
    [HttpPost("paymob/callback")]
    public async Task<IActionResult> PaymobCallback()
    {
        IReadOnlyDictionary<string, string> callbackParams;

        try
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in Request.Query)
                dict[key] = value.ToString();
            if (Request.HasFormContentType)
                foreach (var (key, value) in Request.Form)
                    dict.TryAdd(key, value.ToString());
            callbackParams = dict;
        }
        catch
        {
            return Redirect("/payment-failed?reason=parse_error");
        }

        PaymentVerificationResult result;
        try
        {
            var gateway = _gatewayFactory(GatewayKey.Paymob);
            result = await gateway.HandleCallbackAsync(callbackParams);
        }
        catch (Exception)
        {
            return Redirect("/payment-failed?reason=paymob_exception");
        }

        if (!result.IsSuccess)
            return Redirect($"/payment-failed?reason={Uri.EscapeDataString(result.ErrorMessage ?? "unknown")}");

        return Redirect($"/custom-invoice/details/{result.PaymentId}");
    }
}

/// ///////////////////////////////////////////////////////////////////////////////////////////////

