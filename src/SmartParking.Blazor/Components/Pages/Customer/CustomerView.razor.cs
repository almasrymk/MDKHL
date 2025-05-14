using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Packages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;
using Volo.Abp;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using SmartParking.Enums;
using Blazorise;
using SmartParking.Blazor.Components.Pages.Customer.Vehicles;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Permissions.CustomerPermissions;
using SmartParking.Entities.Organizations;

namespace SmartParking.Blazor.Components.Pages.Customer;

/// <summary>
/// CustomerView component, responsible for displaying customer details and providing actions to view vehicle, invoice, and package information.
/// </summary>
public partial class CustomerView
{
    /// <summary>
    /// Customer ID parameter, used to fetch customer data.
    /// </summary>
    [Parameter]
    public string customerId { get; set; }

    /// <summary>
    /// Customer data DTO, used to store customer information.
    /// </summary>
    public CreateUpdateCustomerDto customerDataDto { get; set; } = new CreateUpdateCustomerDto();

    /// <summary>
    /// Customer picture URL.
    /// </summary>
    public string Picture { get; set; }

    /// <summary>
    /// Customer's identity user ID.
    /// </summary>
    public Guid IdentityUserId { get; set; }

    /// <summary>
    /// Initializes the component, checks authorization, and fetches customer data.
    /// </summary>
    protected async override Task OnInitializedAsync()
    {
        // Check if user has default customer permissions
        if (!await AuthorizationService.IsGrantedAsync(CustomerPermissions.Default))
        {
            // Redirect to access denied page if not authorized
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }

        // Fetch customer data
        await Fetch();

        // Get customer picture
        Picture = await UsersAppService.GetImage(IdentityUserId) ?? "images/users/149071.png";
    }

    /// <summary>
    /// Fetches customer data from the API.
    /// </summary>
    private async Task Fetch()
    {
        var customer = await CustomerAppService.GetAsync(Guid.Parse(customerId));

        IdentityUserId = customer.IdentityId;

        customerDataDto = Mapper.Map<CreateUpdateCustomerDto>(customer);
    }

    /// <summary>
    /// Cancels the current view and navigates back to the customers list.
    /// </summary>
    public void CancelView()
    {
        Navigation.NavigateTo("/customers", false);
    }

    /// <summary>
    /// Displays a dialog to view customer vehicle information.
    /// </summary>
    private async Task<IDialogReference> ViewVehicle()
    {
        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };
        return await DialogService.ShowAsync<ViewCustomerVehicleDialog>("view customer vehicle", parameters);
    }

    /// <summary>
    /// Displays a dialog to view customer invoice information.
    /// </summary>
    private async Task<IDialogReference> ViewInvoice()
    {
        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };
        return await DialogService.ShowAsync<ViewCustomerInvoiceDialog>("view customer invoice", parameters);
    }

    /// <summary>
    /// Displays a dialog to view customer package information.
    /// </summary>
    private async Task<IDialogReference> ViewPackage()
    {
        // Create dialog parameters
        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };
        return await DialogService.ShowAsync<ViewCustomerPackageDialog>("view customer package", parameters);
    }
}
