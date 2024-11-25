using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Packages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerVehicles
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }

    [Parameter]
    public Guid CustomerId { get; set; }

    public List<VehicleDto> vehicles { get; set; } = new List<VehicleDto>();
    public List<PackageDto> Packages { get; set; } = new List<PackageDto>();
    public CustomerDto Customer = new CustomerDto();
    protected override async Task OnInitializedAsync()
    {
        Customer = await CustomerAppService.GetAsync(CustomerId);
        vehicles = await CustomerAppService.GetVehiclesRelatedToSpecificCustomer(CustomerId);
        Packages = await CustomerPackagesAppService.GetListPackagesByCustomer(CustomerId);
    }
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }

}
