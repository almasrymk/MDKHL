using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using System.Linq;
using Microsoft.AspNetCore.Components;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Blazor.Components.Pages.Invoices;
using System.Xml.Linq;
using SmartParking.Entities.Invoices;
using SmartParking.Payments;
using FluentValidation;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Discounts;
using SmartParking.Utilities;


using Microsoft.AspNetCore.Components;
using SmartParking.Blazor.Components.PosTerminal;
using SmartParking.Enums.PosTerminal;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class DetailsInvoice 
{
    [Inject] private IInvoicePaymentProcessorAppService InvoicePaymentProcessorAppService { get; set; } = default!;

    [Parameter] 
    public int Id { get; set; }

    private InvoiceDto Invoice = new InvoiceDto();
    private string LocalizedInvoiceType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Discount { get; set; }=default;
    public bool IsPaid { get; set; }
    private void HandleAddDiscount(decimal selected)
    {
        Discount = selected;
    }

    private void OnSelectPaymentMethod(PaymentMethod selected)
    {
        PaymentMethod = selected;
    }
    protected override async Task OnInitializedAsync()
    {
        await GetInvoice();
        LocalizedInvoiceType = Invoice.InvoiceType switch
        {
            "Paid" => L["Paid"],
            "UnPaid" => L["UnPaid"],
            _ => Invoice.InvoiceType
        };
        IsPaid = Invoice.InvoiceType == InvoiceType.Paid.ToString();
    }
    private async Task GetInvoice()
    {
        Invoice = await InvoiceAppService.GetAsync(Id);
        IsPaid = Invoice.InvoiceType == InvoiceType.Paid.ToString();
        StateHasChanged();
    }
    private void Back()
    {
        Navigation.NavigateTo("/invoices");
    }
    private async Task PayInvoice()
    {
        try
        {
            var initResult = await InvoicePaymentProcessorAppService.InitiatePaymentAsync(new ProcessInvoicePaymentRequestDto
            {
                InvoiceId = Id,
                PaymentMethod = PaymentMethod,
                Discount = Discount
            });

            if (initResult.Result == InvoicePaymentResult.AlreadyPaid)
            {
                Snackbar.Add(L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Info);
                await GetInvoice();
                return;
            }

            if (initResult.Result == InvoicePaymentResult.PaymentInProgress)
            {
                Snackbar.Add(L["PaymentInProgress"], MudBlazor.Severity.Warning);
                return;
            }

            if (initResult.Result == InvoicePaymentResult.Failed)
            {
                Snackbar.Add(initResult.Message ?? L["PaymentFailed"], MudBlazor.Severity.Error);
                return;
            }

            if (PaymentMethod == PaymentMethod.POS)
            {
                var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = initResult.PaymentNumber ?? string.Empty
                };

                Guid createdPaymentId = initResult.PaymentId ?? Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, organization.OrganizationSettings },
                    { x => x.ExistingPaymentId, createdPaymentId },
                    { x => x.ExistingPaymentNumber, initResult.PaymentNumber },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) }
                };

                var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
                var dialog = await DialogService.ShowAsync<PosPaymentDialog>("POS Transaction", parameters, options);
                var result = await dialog.Result;

                if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
                {
                    var completeResult = await InvoicePaymentProcessorAppService.CompletePosPaymentAsync(new CompletePosPaymentRequestDto
                    {
                        InvoiceId = Id,
                        PaymentId = createdPaymentId,
                        SamaCode = snapshot.SamaCode,
                        RawJsonResult = snapshot.Outcome.ToString(),
                        TerminalId = checkoutRequest.PaymentNumber,
                        TransactionStan = null,
                        Rrn = snapshot.Rrn,
                        AuthCode = snapshot.AuthCode,
                        MaskedPan = snapshot.MaskedPan,
                        CardScheme = snapshot.CardScheme
                    });

                    if (completeResult.Result == InvoicePaymentResult.Completed)
                    {
                        SnackBar.ShowSnackbar(Snackbar, $"{L["CheckoutConfirm"]}", MudBlazor.Severity.Success);
                        await Task.Delay(1000);
                        Navigation.NavigateTo($"/payments/view/{createdPaymentId}");
                    }
                    else if (completeResult.Result == InvoicePaymentResult.AlreadyPaid)
                    {
                        Snackbar.Add(completeResult.Message ?? L["InvoiceIsAlreadyPaid"], MudBlazor.Severity.Warning);
                        await GetInvoice();
                    }
                    else
                    {
                        Snackbar.Add(snapshot.SamaDescription ?? completeResult.Message ?? "Transaction Declined", MudBlazor.Severity.Warning);
                    }
                }
            }
            else
            {
                if (initResult.Result == InvoicePaymentResult.Completed && initResult.PaymentId.HasValue)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L["CheckoutConfirm"]}", MudBlazor.Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo($"/payments/view/{initResult.PaymentId.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L[ex.Message], MudBlazor.Severity.Error);
        }
    }
}
