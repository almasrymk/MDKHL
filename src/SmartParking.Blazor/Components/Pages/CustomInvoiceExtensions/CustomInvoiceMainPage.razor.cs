using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Customer.Vehicles;
using SmartParking.Blazor.Dtos;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Utilities;
using SmartParking.Validations;
using SmartParking.GarageTransactions;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceMainPage : IDisposable
{
    private DotNetObjectReference<CustomInvoiceMainPage>? _objRef;
    // ── Injected config ───────────────────────────────────────────────────────

    /// <summary>
    /// Provides access to the active gateway's CallbackUrl from paymentconfig.json.
    /// The active provider (Geidea / Oppwa) is determined by Payment:ActiveProvider.
    /// </summary>
    [Inject] private IOptionsMonitor<GeideaOptions>     GeideaOptions  { get; set; } = default!;
    [Inject] private IOptionsMonitor<OppwaRootOptions>  OppwaOptions   { get; set; } = default!;
    [Inject] private IPaymentWebAppService              PaymentWebAppService { get; set; } = default!;
    [Inject] private IOrganizationsAppService           OrganizationsAppService { get; set; } = default!;
    [Inject] private ILogger<CustomInvoiceMainPage>     Logger { get; set; } = default!;

    // ── Page parameters / state ───────────────────────────────────────────────

    [Parameter] public string WatcherCode { get; set; }
    [Parameter] public string? RouteTicketId { get; set; }

    [SupplyParameterFromQuery]
    public Guid? PaymentId { get; set; }

    private string SearchText { get; set; }
    private string PlateSearchText { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal InvoicePrice { get; set; }
    public decimal Vat { get; set; }
    public DateTime Checkout { get; set; }
    public DateTime CheckIn { get; set; }
    public string Duration { get; set; }
    public string EntryPicture { get; set; }
    public string ExitPicture { get; set; }
    public string PlateNumber { get; set; }
    public SmartParking.IntegrationUnit.Dtos.VehiclePlateDto VehiclePlate { get; set; } = new();
    public string TicketId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public int VatRate { get; set; }
    private InvoiceDto? LatestInvoice { get; set; }
    private CreateUpdateInvoiceUiDto? invoiceDto { get; set; } = new();
    private bool ShowInvoiceDetails = false;

    /// <summary>
    /// True while the gateway session is being created.
    /// Disables the payment button and shows a spinner to prevent double-clicks.
    /// </summary>
    private bool _isProcessingPayment = false;

    private InvoiceSearchDto InvoiceSearchDto = new InvoiceSearchDto();

    private int _currentInvoiceId;
    private bool _checkoutClicked = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation(
            "[TRACE] OnInitializedAsync — Uri={Uri}, PaymentId={PaymentId}, RouteTicketId={RouteTicketId}, Thread={Thread}",
            navigationManager.Uri, PaymentId, RouteTicketId,
            System.Threading.Thread.CurrentThread.ManagedThreadId);

        _objRef = DotNetObjectReference.Create(this);
        try
        {
            InvoicePrice = 0;
            VatRate = 15;
            Vat = 0;
            InvoiceAmount = 0;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[TRACE] OnInitializedAsync — init values threw, navigating to /");
            navigationManager.NavigateTo("/", true);
        }

        if (!string.IsNullOrEmpty(RouteTicketId))
        {
            SearchText = RouteTicketId;
            await OnSearch();
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private bool _hasShownPaymentSuccessDialog = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Logger.LogInformation(
            "[TRACE] OnAfterRenderAsync — firstRender={FirstRender}, PaymentId={PaymentId}, Thread={Thread}",
            firstRender, PaymentId, System.Threading.Thread.CurrentThread.ManagedThreadId);

        if (firstRender && PaymentId.HasValue)
        {
            Logger.LogInformation("[TRACE] OnAfterRenderAsync — PaymentId present, entering payment-callback path");

            try
            {
                Logger.LogInformation("[TRACE] OnAfterRenderAsync — calling GetMdkhlAsync({PaymentId})", PaymentId.Value);

                PaymentGetResponseDto payment;
                try
                {
                    payment = await PaymentWebAppService.GetMdkhlAsync(PaymentId.Value);
                }
                catch (Exception fetchEx)
                {
                    Logger.LogError(fetchEx, "[TRACE] OnAfterRenderAsync — GetMdkhlAsync THREW an exception");
                    snackbar.Add("Payment lookup error: " + fetchEx.Message, Severity.Error);
                    return;
                }

                if (payment == null)
                {
                    Logger.LogWarning("[TRACE] OnAfterRenderAsync — GetMdkhlAsync returned NULL");
                    snackbar.Add("Payment record not found.", Severity.Warning);
                    return;
                }

                Logger.LogInformation(
                    "[TRACE] OnAfterRenderAsync — payment found. PaymentStatus={Status}, Amount={Amount}, InvoiceId={InvoiceId}",
                    payment.PaymentStatus, payment.Amount, payment.Invoice?.Id);

                var expectedStatus = PaymentStatus.Success.ToString();
                Logger.LogInformation(
                    "[TRACE] OnAfterRenderAsync — comparing PaymentStatus '{Actual}' == '{Expected}' → {Match}",
                    payment.PaymentStatus, expectedStatus,
                    payment.PaymentStatus == expectedStatus);

                if (payment.PaymentStatus == expectedStatus)
                {
                    InvoicePrice = payment.Invoice?.InvoicePrice ?? payment.Amount;
                    InvoiceSearchDto.TicketId = payment.Invoice?.TicketId ?? "";
                    VatRate = 15;
                    Vat = payment.Invoice?.Vat ?? 0;
                    InvoiceAmount = InvoicePrice + Vat;
                    CheckIn = payment.Invoice?.EntranceTime ?? DateTime.Now;
                    Checkout = payment.Invoice?.InvoiceTime ?? DateTime.Now;
                    _currentInvoiceId = payment.Invoice?.Id ?? 0;

                    string qrData = payment.QrCodeInvoice;
                    int gracePeriod = payment.Organization?.Graceperiod ?? 0;

                    Logger.LogInformation(
                        "[TRACE] OnAfterRenderAsync — about to call ShowPaymentSuccessDialogAsync. InvoicePrice={Price}, TicketId={TicketId}, InvoiceId={InvoiceId}",
                        InvoicePrice, InvoiceSearchDto.TicketId, _currentInvoiceId);

                    await ShowPaymentSuccessDialogAsync(
                        (double)InvoicePrice,
                        InvoiceSearchDto.TicketId,
                        (double)VatRate,
                        (double)Vat,
                        (double)InvoiceAmount,
                        qrData,
                        CheckIn,
                        Checkout,
                        _currentInvoiceId,
                        gracePeriod);

                    Logger.LogInformation("[TRACE] OnAfterRenderAsync — returned from ShowPaymentSuccessDialogAsync");
                }
                else
                {
                    Logger.LogWarning(
                        "[TRACE] OnAfterRenderAsync — payment status not Success, skipping dialog. Status={Status}",
                        payment.PaymentStatus);
                    snackbar.Add($"Payment status: {payment.PaymentStatus}", Severity.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[TRACE] OnAfterRenderAsync — unhandled exception in payment-callback path");
                snackbar.Add("Payment callback error: " + ex.Message, Severity.Error);
            }
        }
        else if (firstRender)
        {
            Logger.LogInformation("[TRACE] OnAfterRenderAsync — firstRender=true but NO PaymentId in query string. Normal page load.");
        }
    }


    // ── Payment flow ──────────────────────────────────────────────────────────

    private async Task ProceedToPayment()
    {
        if (_isProcessingPayment || _checkoutClicked) return;

        try
        {
            _isProcessingPayment = true;
            StateHasChanged();

            var invoice = await invoiceUiAppService.GetInvoiceAfterCheckout(InvoiceSearchDto);
            _currentInvoiceId = invoice.InvoiceId;

            // ── Build callback URL from paymentconfig.json ────────────────────
            // We construct the absolute URL using the app's base URI + the
            // relative path stored in config (e.g. /api/PaymentResult/geidea/callback).
            // This means: change the path in paymentconfig.json and no C# code changes
            // are needed — Geidea will redirect back to the correct endpoint automatically.
            var callbackUrl = ResolveCallbackUrl();

            var dto = new PaymentCheckoutRequestDto
            {
                InvoiceId = invoice.InvoiceId,
                OrganizationId = invoice.OrganizationId,
                Amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, invoice.Discount),
                PaymentCategory = PaymentCategory.PaymentParking,
                PaymentMethod   = PaymentMethod.MADA, // Defaulted for Geidea, dynamically handled on hosted page
                TenantId        = invoice.TenantId!.Value,
                EnPlateNumber   = invoice.PlateNumber,
                CallbackUrl     = callbackUrl
            };

            var result = await gateway.InitiateAsync(dto);

            // ── Dispatch the correct JS widget (Geidea or Oppwa) ─────────────
            // gateway.InitiateAsync() returns JsMethodName + JsArguments that
            // map directly to the JS functions defined in the razor file:
            //   • "geidea.loadCheckout"   for Geidea Hosted Page
            //   • "hyperPay.loadWidget"   for HyperPay/Oppwa widget
            await JS.InvokeVoidAsync("setDotNetHelper", _objRef);
            await JS.InvokeVoidAsync(result.JsMethodName, result.JsArguments);

            _checkoutClicked = true;
        }
        catch (Exception ex)
        {
            snackbar.Add(L["PaymentFailed"] + " " + ex.Message, Severity.Error);
            _checkoutClicked = false;
        }
        finally
        {
            _isProcessingPayment = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Builds the absolute callback URL by combining the app's own BaseUri with
    /// the relative path from paymentconfig.json.
    ///
    /// For Geidea:  BaseUri + Providers:Geidea:{Mode}:CallbackUrl
    ///              → e.g. https://parking.example.com/api/PaymentResult/geidea/callback
    ///
    /// For Oppwa:   Uses NavigationManager because Oppwa's CallbackUrl is typically
    ///              configured as a full path at the gateway level.
    ///
    /// Change the path in paymentconfig.json — no code changes required.
    /// </summary>
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    private string ResolveCallbackUrl()
    {
        return PaymentCallbackHelper.ResolveCallbackUrl(ServiceProvider, navigationManager.BaseUri, gateway.Gateway);
    }

    // ── Invoice search ────────────────────────────────────────────────────────

    private async Task OnSearch()
    {
        try
        {
            if (!string.IsNullOrEmpty(SearchText) && SearchText.Any(char.IsLetter))
            {
                InvoiceSearchDto.TicketId = string.Empty;
                InvoiceSearchDto.PlateNumber = SearchText;
            }
            else
            {
                InvoiceSearchDto.TicketId = SearchText ?? string.Empty;
                InvoiceSearchDto.PlateNumber = string.Empty;
            }

            invoiceDto = await invoiceUiAppService.GetInvoiceAfterPreCheckout(InvoiceSearchDto);
            if (invoiceDto is not null)
            {
                ShowInvoiceDetails = true;
                _checkoutClicked = false;
                InvoiceNumber = invoiceDto.InvoiceNumber;
                InvoiceDate   = invoiceDto.InvoiceTime!.Value;
                InvoicePrice  = Math.Round(invoiceDto.InvoicePrice, 2);
                Vat           = Math.Round(invoiceDto.Vat, 2);
                var duration  = invoiceDto.InvoiceTime.Value - invoiceDto.EntranceTime!.Value;
                Duration      = $"{(int)duration.TotalHours} {L["Hours"]} {duration.Minutes} {L["Minutes"]}";
                CheckIn       = invoiceDto.EntranceTime.Value;
                Checkout      = invoiceDto.InvoiceTime.Value;
                EntryPicture  = invoiceDto.EntryTransactionImage;
                ExitPicture   = invoiceDto.ExitTransactionImage;
                PlateNumber   = invoiceDto.PlateNumber;
                
                ParsePlateNumber(PlateNumber);

                InvoiceAmount = Math.Round(InvoicePrice + Vat, 2);
                VatRate       = 15;
                TicketId      = invoiceDto.TicketId;
            }
            else
            {
                ShowInvoiceDetails = false;
                snackbar.Add(L["CarNotFound"], Severity.Error);
            }
        }
        catch (Volo.Abp.UserFriendlyException ex)
        {
            ShowInvoiceDetails = false;
            snackbar.Add(ex.Message, Severity.Error);
        }
        catch (Exception)
        {
            ShowInvoiceDetails = false;
            snackbar.Add(L["CarNotFound"], Severity.Error);
        }
        StateHasChanged();
    }

    private void ParsePlateNumber(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate)) return;

        try
        {
            VehiclePlate = VehicleValidation.Parse(plate);
        }
        catch
        {
            VehiclePlate = new SmartParking.IntegrationUnit.Dtos.VehiclePlateDto
            {
                EnPlateNumber = plate,
                ArPlateNumber = plate,
                EnPlateLetters = string.Empty,
                ArPlateLetters = string.Empty
            };
        }
    }

    private void OnSearchValueChanged(string searchValue)
    {
        SearchText = searchValue;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task HandlePaymentSuccess(System.Text.Json.JsonElement? geideaOrder = null)
    {
        snackbar.Add("Payment successful!", Severity.Success);

        string? qrData = null;
        qrData = FetchQrData(geideaOrder, qrData);

        int gracePeriod = 0;
        if (invoiceDto != null && invoiceDto.OrganizationId > 0)
        {
            var org = await OrganizationsAppService.GetAsync(invoiceDto.OrganizationId);
            gracePeriod = org?.Graceperiod ?? 0;
        }

        await ShowPaymentSuccessDialogAsync(
            (double)InvoicePrice,
            InvoiceSearchDto.TicketId,
            (double)VatRate,
            (double)Vat,
            (double)InvoiceAmount,
            qrData,
            CheckIn,
            Checkout,
            _currentInvoiceId,
            gracePeriod);
    }

    private async Task ShowPaymentSuccessDialogAsync(
        double price,
        string ticketId,
        double taxPercentage,
        double taxAmount,
        double totalAmount,
        string? qrData,
        DateTime? entranceTime,
        DateTime? exitTime,
        int invoiceId,
        int gracePeriod)
    {
        Logger.LogInformation(
            "[TRACE] ShowPaymentSuccessDialogAsync — entered. _hasShown={HasShown}, InvoiceId={InvoiceId}, Thread={Thread}",
            _hasShownPaymentSuccessDialog, invoiceId,
            System.Threading.Thread.CurrentThread.ManagedThreadId);

        if (_hasShownPaymentSuccessDialog)
        {
            Logger.LogWarning("[TRACE] ShowPaymentSuccessDialogAsync — already shown, returning early (duplicate guard)");
            return;
        }
        _hasShownPaymentSuccessDialog = true;

        IDialogReference? dialog = null;

        try
        {
            var parameters = new DialogParameters
            {
                { "Price", price },
                { "TicketId", ticketId },
                { "TaxPercentage", taxPercentage },
                { "TaxAmount", taxAmount },
                { "TotalAmount", totalAmount },
                { "QrData", qrData },
                { "EntranceTime", entranceTime },
                { "ExitTime", exitTime },
                { "InvoiceId", invoiceId },
                { "GracePeriod", gracePeriod }
            };

            var options = new DialogOptions
            {
                CloseButton = false,
                CloseOnEscapeKey = false,
                BackdropClick = false,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };

            Logger.LogInformation("[TRACE] ShowPaymentSuccessDialogAsync — calling InvokeAsync → DialogService.ShowAsync");

            try
            {
                await InvokeAsync(async () =>
                {
                    Logger.LogInformation(
                        "[TRACE] ShowPaymentSuccessDialogAsync — inside InvokeAsync, Thread={Thread}",
                        System.Threading.Thread.CurrentThread.ManagedThreadId);

                    dialog = await DialogService.ShowAsync<PaymentSuccessDialog>(" ", parameters, options);

                    Logger.LogInformation(
                        "[TRACE] ShowPaymentSuccessDialogAsync — ShowAsync returned. dialog={DialogNull}",
                        dialog == null ? "NULL" : "OK");
                });
            }
            catch (Exception showEx)
            {
                Logger.LogError(showEx, "[TRACE] ShowPaymentSuccessDialogAsync — DialogService.ShowAsync THREW inside InvokeAsync");
                _hasShownPaymentSuccessDialog = false;
                snackbar.Add("Dialog open error: " + showEx.Message, Severity.Error);
                return;
            }

            if (dialog == null)
            {
                Logger.LogError("[TRACE] ShowPaymentSuccessDialogAsync — dialog reference is NULL after ShowAsync. MudDialogProvider may be missing.");
                _hasShownPaymentSuccessDialog = false;
                snackbar.Add("Dialog could not be opened (null reference). Check MudDialogProvider in layout.", Severity.Error);
                return;
            }

            Logger.LogInformation("[TRACE] ShowPaymentSuccessDialogAsync — awaiting dialog.Result (waiting for user to close)");

            // Wait until the user explicitly closes the dialog.
            var result = await dialog.Result;

            Logger.LogInformation(
                "[TRACE] ShowPaymentSuccessDialogAsync — dialog closed by user. Canceled={Canceled}",
                result?.Canceled);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[TRACE] ShowPaymentSuccessDialogAsync — unhandled exception");
            _hasShownPaymentSuccessDialog = false;
            snackbar.Add("Payment success dialog error: " + ex.Message, Severity.Error);
            return;
        }

        // Dialog closed by user.
        // 1. Clean the URL address bar.
        try
        {
            var cleanPath = new Uri(navigationManager.Uri).GetLeftPart(UriPartial.Path);
            Logger.LogInformation("[TRACE] ShowPaymentSuccessDialogAsync — calling history.replaceState to {CleanPath}", cleanPath);
            await JS.InvokeVoidAsync("history.replaceState", null, "", cleanPath);
        }
        catch (Exception jsEx)
        {
            Logger.LogWarning(jsEx, "[TRACE] ShowPaymentSuccessDialogAsync — history.replaceState failed (ignored)");
        }

        // 2. Reload once with clean path.
        var refreshUrl = new Uri(navigationManager.Uri).GetLeftPart(UriPartial.Path);
        Logger.LogInformation("[TRACE] ShowPaymentSuccessDialogAsync — navigating to {RefreshUrl} with forceLoad=true", refreshUrl);
        navigationManager.NavigateTo(refreshUrl, forceLoad: true);
    }

    private static string FetchQrData(System.Text.Json.JsonElement? geideaOrder, string qrData)
    {
        PaymentMethod finalPaymentMethod = PaymentMethod.MADA; // Default

        if (geideaOrder.HasValue)
        {
            try
            {
                // Attempt to map Geidea's payment type back to local PaymentMethod Enum
                if (geideaOrder.Value.TryGetProperty("paymentMethod", out var pmProp) && pmProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (pmProp.TryGetProperty("brand", out var brandProp) && brandProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var brand = brandProp.GetString()?.ToLowerInvariant();
                        if (brand == "applepay" || brand == "apple pay" || brand == "apple-pay")
                            finalPaymentMethod = PaymentMethod.ApplePay;
                        else if (brand == "mada")
                            finalPaymentMethod = PaymentMethod.MADA;
                        else if (brand == "visa")
                            finalPaymentMethod = PaymentMethod.VISA;
                        else if (brand == "mastercard")
                            finalPaymentMethod = PaymentMethod.MASTERCARD;
                    }
                }
                else if (geideaOrder.Value.TryGetProperty("order", out var orderProp) && orderProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (orderProp.TryGetProperty("paymentMethod", out var nestedPmProp) && nestedPmProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (nestedPmProp.TryGetProperty("brand", out var nestedBrandProp) && nestedBrandProp.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var brand = nestedBrandProp.GetString()?.ToLowerInvariant();
                            if (brand == "applepay" || brand == "apple pay" || brand == "apple-pay")
                                finalPaymentMethod = PaymentMethod.ApplePay;
                            else if (brand == "mada")
                                finalPaymentMethod = PaymentMethod.MADA;
                            else if (brand == "visa")
                                finalPaymentMethod = PaymentMethod.VISA;
                            else if (brand == "mastercard")
                                finalPaymentMethod = PaymentMethod.MASTERCARD;
                        }
                    }
                }

                if (geideaOrder.Value.TryGetProperty("qrCode", out var qrCodeProp) && qrCodeProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    qrData = qrCodeProp.GetString()!;
                }
                else if (geideaOrder.Value.TryGetProperty("qrMessage", out var qrMsgProp) && qrMsgProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    qrData = qrMsgProp.GetString()!;
                }
                else if (geideaOrder.Value.TryGetProperty("order", out var orderObjProp) && orderObjProp.ValueKind == System.Text.Json.JsonValueKind.Object && orderObjProp.TryGetProperty("qrCode", out var nestedQrProp))
                {
                    qrData = nestedQrProp.GetString()!;
                }
                else if (geideaOrder.Value.TryGetProperty("orderId", out var orderIdProp) && orderIdProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    qrData = orderIdProp.GetString()!;
                }
            }
            catch { }
        }

        return qrData;
    }

    private async Task Cancel()
    {
        try
        {
            await InvoiceUiAppService.CancelInvoiceAsync(
                _currentInvoiceId
                );

            snackbar.Add(
                $"{L["Done"]} {L["Delete"]} {L["Invoice"]}",
                Severity.Success);

            await Task.Delay(1000);

            ResetPage();

            navigationManager.NavigateTo(
                navigationManager.Uri,
                forceLoad: true);
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
        }
    }
    private void ResetPage()
    {
        _checkoutClicked = false;
        ShowInvoiceDetails = false;
        invoiceDto = null;
        SearchText = string.Empty;
        PlateSearchText = string.Empty;
        InvoiceSearchDto = new InvoiceSearchDto();
        _currentInvoiceId = 0;

        StateHasChanged();
    }

    public void Dispose()
    {
        _objRef?.Dispose();
    }

}
