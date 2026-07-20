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
        await FetchData();  
        StateHasChanged();
        await base.OnInitializedAsync();
    }
    private async Task FetchData()
    {
        Invoice = await InvoiceAppService.GetAsync(Id);

        LocalizedInvoiceType = L[Invoice.InvoiceType];
        IsPaid= Invoice.InvoiceType == Enums.InvoiceType.Paid.ToString()?true:false;
    }
    private void Back()
    {
        Navigation.NavigateTo("/invoices");
    }
    private async Task PayInvoice()
    {
        if (Discount != default)
        {
            await InvoiceAppService.UpdateDiscountForInvoice(Id, Discount);
        }

        try
        {
            var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

            if (PaymentMethod == PaymentMethod.POS &&
                organization.OrganizationSettings is { PosTerminalEnabled: true })
            {
                var checkoutRequest = new PaymentCheckoutRequestDto
                {
                    OrganizationId = Invoice.OrganizationId,
                    PaymentCategory = PaymentCategory.PaymentParking,
                    InvoiceId = Id,
                    PaymentMethod = PaymentMethod,
                    Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
                    CustomerId = Invoice.CustomerId,
                    PaymentName = PaymentMethod.POS.ToString(),
                    PaymentNumber = string.Empty
                };

                Guid createdPaymentId = Guid.Empty;

                var parameters = new DialogParameters<PosPaymentDialog>
                {
                    { x => x.Request, checkoutRequest },
                    { x => x.OrgSettings, organization.OrganizationSettings },
                    { x => x.OnPaymentCreated, EventCallback.Factory.Create<Guid>(this, id => createdPaymentId = id) }
                };

                var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
                var dialog = await DialogService.ShowAsync<PosPaymentDialog>("POS Transaction", parameters, options);
                var result = await dialog.Result;

                if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
                {
                    if (snapshot.Outcome == SamaResponseOutcome.Approved)
                    {
                        SnackBar.ShowSnackbar(Snackbar, $"{L["CheckoutConfirm"]}", MudBlazor.Severity.Success);
                        await Task.Delay(1000);
                        Navigation.NavigateTo($"/payments/view/{createdPaymentId}");
                    }
                    else
                    {
                        Snackbar.Add(snapshot.SamaDescription ?? "Transaction Declined", MudBlazor.Severity.Warning);
                    }
                }
            }
            else
            {
                PaymentCheckoutDto payment = await RequestToPaymentCheckout();
                if (payment != null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L["CheckoutConfirm"]}", MudBlazor.Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo($"/payments/view/{payment.PaymentId}");
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L[ex.Message], MudBlazor.Severity.Error);
        }
    }

    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout()
    {
        var paymentCheckout = new PaymentCheckoutRequestDto
        {
            OrganizationId = Invoice.OrganizationId,
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId
        };
        PaymentCheckoutDto? payment = await PaymentAppSerivce.CreateCheckoutInvoicePaymentWebAsync(paymentCheckout);
        return payment;
    }
}
