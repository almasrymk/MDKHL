using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cutomers;
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
    public CustomerDto Customer = new CustomerDto();
    protected override async Task OnInitializedAsync()
    {
       vehicles = await CustomerAppService.GetVehiclesRelatedToSpecificCustomer(CustomerId);
        Customer = await CustomerAppService.GetAsync(CustomerId);
    }
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }

}
