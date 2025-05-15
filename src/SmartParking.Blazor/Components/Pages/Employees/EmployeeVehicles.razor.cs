using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Vehicles;

namespace SmartParking.Blazor.Components.Pages.Employees;
/// <summary>
/// Represents the Employee Vehicles component.
/// </summary>  
public partial class EmployeeVehicles
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }

    [Parameter]
    public Guid CustomerId { get; set; }

    public List<VehicleDto> vehicles { get; set; } = new List<VehicleDto>();
    public CustomerDto Customer = new CustomerDto();
    /// <summary>
    /// Initializes the component.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Customer = await EmployeeAppService.GetAsync(CustomerId);
        vehicles = await EmployeeAppService.GetVehiclesRelatedToSpecificEmployee(CustomerId);
    }
    /// <summary>
    /// Closes the view  dialog.
    /// </summary>
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }
}
