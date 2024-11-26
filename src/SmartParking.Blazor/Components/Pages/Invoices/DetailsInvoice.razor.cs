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


namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class DetailsInvoice 
{
    public string code;
    [Parameter] 
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string OrganizationArabicName { get; set; }
    public string InvoiceType { get; set; }

    public string GarageName { get; set; }
    public string PlateNumber { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public string  OrgAddress { get; set; }
    public string  CustomerName { get; set; }
    public DateTime ?InvoiceTime { get; set; }

    public DateTime ?EntranceTime { get; set; }



    public string CommercialRegisteration { get; set; }

    public List<OrganizationDto> organizations = new List<OrganizationDto>();

    public List<GarageDto> garages = new List<GarageDto>();

    public List<VehicleDto> vehicles = new List<VehicleDto>();
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };


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
        OrgAddress = organizations.Find(x => x.ArabicName == OrganizationArabicName).Address;

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
        GarageName = invoice.GarageName;
        PlateNumber = invoice.PlateNumber;
        InvoiceTime = invoice.InvoiceTime;
        EntranceTime = invoice.EntranceTime;
        CustomerName=invoice.CustomerEnName;



    }
    private void Back()
    {
        Navigation.NavigateTo("/invoices");
    }
    private Task ViewInvoiceDetails()
    {
        var invoicedto = new InvoiceDto
        {
            InvoiceNumber = InvoiceNumber,
            TotalHours = TotalHours,
            InvoicePrice = InvoicePrice,
            InvoiceTime = InvoiceTime,
            GarageName = GarageName,
            OrganizationArabicName = OrganizationArabicName,
            PlateNumber = PlateNumber,
            EntranceTime = EntranceTime,
            CustomerArName=CustomerName,
            
        };
        
        var parameters = new DialogParameters
        {
            ["Invoice"] = invoicedto,
            ["CommercialRegisteration"]= CommercialRegisteration,
            ["ID"]=Id,
            ["OrganizationAddress"]=OrgAddress,
        };
        return DialogService.ShowAsync<InvoiceDetailsViewDialog>("",parameters);
        

    }
}
