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

    protected override async Task OnInitializedAsync()
    {
        await FetchData();  
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
        PaymentCheckoutDto payment = await RequestToPaymentCheckout();
        if (payment != null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);

            await Task.Delay(1000);
            Navigation.NavigateTo($"/payments/view/{payment.PaymentId}");
        }
    }

    private async Task<PaymentCheckoutDto> RequestToPaymentCheckout()
    {
        var paymentChekout = new PaymentCheckoutRequestDto
        {
            OrganizationId = Invoice.OrganizationId,
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId
        };
        var payment = await PaymentAppSerivce.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
        return payment;
    }
}
