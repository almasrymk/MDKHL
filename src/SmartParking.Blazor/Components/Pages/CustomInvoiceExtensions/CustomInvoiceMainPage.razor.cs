using Microsoft.AspNetCore.Components;
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
        _objRef = DotNetObjectReference.Create(this);
        try
        {
            InvoicePrice = 0;
            VatRate = 15;
            Vat = 0;
            InvoiceAmount = 0;
        }
        catch (Exception)
        {
            navigationManager.NavigateTo("/", true);
        }

        if (!string.IsNullOrEmpty(RouteTicketId))
        {
            SearchText = RouteTicketId;
            await OnSearch();
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && PaymentId.HasValue)
        {
            try
            {
                // Remove the paymentId from the browser's address bar immediately 
                // so that refreshing the page does not re-open the dialog.
                // await JS.InvokeVoidAsync("history.replaceState", null, "", "/checkout-extension");

                var payment = await PaymentWebAppService.GetMdkhlAsync(PaymentId.Value);

                if (payment != null && payment.PaymentStatus == PaymentStatus.Success.ToString())
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

                    var parameters = new DialogParameters
                    {
                        { "Price", (double)InvoicePrice },
                        { "TicketId", InvoiceSearchDto.TicketId },
                        { "TaxPercentage", (double)VatRate },
                        { "TaxAmount", (double)Vat },
                        { "TotalAmount", (double)InvoiceAmount },
                        { "QrData", qrData },
                        { "EntranceTime", CheckIn },
                        { "ExitTime", Checkout },
                        { "InvoiceId", _currentInvoiceId },
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

                    var dialog = await DialogService.ShowAsync<PaymentSuccessDialog>(" ", parameters, options);
                    await dialog.Result;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                snackbar.Add(L["PaymentFailed"] + " " + ex.Message, Severity.Error);
            }
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
                snackbar.Add(L["InvoiceNotFound"], Severity.Error);
                await Task.Delay(1000);
            }
        }
        catch (Exception e)
        {
            snackbar.Add(e.Message, Severity.Error);
            await Task.Delay(1000);
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

        string qrData = null;
        qrData = FetchQrData(geideaOrder, qrData);

        int gracePeriod = 0;
        if (invoiceDto != null && invoiceDto.OrganizationId > 0)
        {
            var org = await OrganizationsAppService.GetAsync(invoiceDto.OrganizationId);
            gracePeriod = org?.Graceperiod ?? 0;
        }

         var dialog = await DialogService.ShowAsync<PaymentSuccessDialog>(" ", new DialogParameters
        {
            { "Price", (double)InvoicePrice },
            { "TicketId", InvoiceSearchDto.TicketId },
            { "TaxPercentage", (double)VatRate },
            { "TaxAmount", (double)Vat },
            { "TotalAmount", (double)InvoiceAmount },
            { "QrData", qrData },
            { "EntranceTime", CheckIn },
            { "ExitTime", Checkout },
            { "InvoiceId", _currentInvoiceId },
            { "GracePeriod", gracePeriod }
        }, new DialogOptions
         {
             CloseButton = false,
             CloseOnEscapeKey = false,
             BackdropClick = false,
             MaxWidth = MaxWidth.Small,
             FullWidth = true
         });
        await dialog.Result;

        StateHasChanged();
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
