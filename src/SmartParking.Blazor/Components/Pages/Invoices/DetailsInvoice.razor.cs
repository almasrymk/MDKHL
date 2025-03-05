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
  
    public string code;
    [Parameter] 
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string InvoiceType { get; set; }

    public string GarageEnglishName { get; set; }
    public string GarageArabicName { get; set; }
    public string PlateNumber { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public string  OrgAddress { get; set; }
    public string  CustomerName { get; set; }
    public DateTime ?InvoiceTime { get; set; }

    public DateTime ?EntranceTime { get; set; }
    public DateTime ?ExitTime { get; set; }

    public int OrganizationId { get; set; }
    public Guid? CustomerId { get; set; }

    public bool IsPaid { get; set; }
    public bool IsPackage { get; set; }
    private string LocalizedInvoiceType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }





    public string CommercialRegisteration { get; set; }

    public List<OrganizationDto> organizations = new List<OrganizationDto>();

    public List<GarageDto> garages = new List<GarageDto>();

    public List<VehicleDto> vehicles = new List<VehicleDto>();

    private DiscountDto selectedItem;
    public List<DiscountDto> _discounts = new List<DiscountDto>();

    private void HandleLookupSelection(DiscountDto selected)
    {
        selectedItem = selected;
    }

    protected override async Task OnInitializedAsync()
    {
        await FetchData();
        var organizationDto = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        var garageDto = await GarageAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        var vehicleDto = await VehiclesAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        organizations = organizationDto.Items.ToList();

        garages = garageDto.Items.ToList();

        vehicles = vehicleDto.Items.ToList();
        CommercialRegisteration = organizations.Find(x => x.ArabicName == OrganizationArabicName).CommercialRegistrationNumber;
        var org = organizations.Find(x => x.ArabicName == OrganizationArabicName);
        OrgAddress=org.Address;
        OrganizationId = org.Id;

        var result = await DiscountAppService.GetListAsync(new PagedAndSortedAndSearchResultRequestDto());
        _discounts=result.Items.ToList();
        await base.OnInitializedAsync();

    }
    private async Task FetchData()
    {
        var invoice = await InvoiceAppService.GetAsync(Id);
        InvoiceNumber = invoice.InvoiceNumber;
        TotalHours = invoice.TotalHours;
        InvoicePrice = invoice.InvoicePrice;
        InvoiceType = invoice.InvoiceType.ToString();
        OrganizationArabicName = invoice.OrganizationArabicName;
        OrganizationEnglishName = invoice.OrganizationEnglishName;
        GarageEnglishName = invoice.GarageEnglishName;
        GarageArabicName = invoice.GarageArabicName;
        PlateNumber = invoice.PlateNumber;
        InvoiceTime = invoice.InvoiceTime;
        EntranceTime = invoice.EntranceTime;
        CustomerName=invoice.CustomerEnName;
        ExitTime = invoice.InvoiceTime;
        CustomerId = invoice.CustomerId;
        IsPaid= invoice.InvoiceType == Enums.InvoiceType.Paid.ToString()?true:false;
        LocalizedInvoiceType = L[invoice.InvoiceType];
        IsPackage=invoice.IsPackage;


    }
    private void Back()
    {
        Navigation.NavigateTo("/invoices");
    }
    private async Task PayInvoice()
    {
        var paymentChekout = new PaymentCheckoutRequestDto
        {
            OrganizationId = OrganizationId,
            PaymentCategory = IsPackage ? PaymentCategory.Package : PaymentCategory.PaymentParking,
            InvoiceId = Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(InvoicePrice, selectedItem.DiscountPercent),
            CustomerId = CustomerId
        };
        var payment = await PaymentAppSerivce.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
        if (payment != null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]}", MudBlazor.Severity.Success);

            await Task.Delay(1000);
            Navigation.NavigateTo($"/payments/view/{payment.PaymentId}");
        }
    }


}
