using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Vehicles;

namespace SmartParking.Blazor.Components.Pages.Employees;

public partial class EmployeeVehicles
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }

    [Parameter]
    public Guid CustomerId { get; set; }

    public List<VehicleDto> vehicles { get; set; } = new List<VehicleDto>();
    public CustomerDto Customer = new CustomerDto();
    protected override async Task OnInitializedAsync()
    {
        Customer = await EmployeeAppService.GetAsync(CustomerId);
        vehicles = await EmployeeAppService.GetVehiclesRelatedToSpecificEmployee(CustomerId);
    }
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }
}
