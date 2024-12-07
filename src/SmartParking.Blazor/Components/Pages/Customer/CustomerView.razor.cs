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

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerView
{
    [Parameter]
    public string customerId { get; set; }
    public CreateUpdateCustomerDto customerDataDto { get; set; } = new CreateUpdateCustomerDto();
    public string Picture { get; set; }
    protected async override Task OnInitializedAsync()
    {
        await Fetch();

        if ((currentUser.IsInRole(Enums.RolesType.Mdkhl.ToString())) || (currentUser.IsInRole("admin")))
        {
            Picture = "images/MDHKL_04.png";

        }
        else
        {
            Picture = await OrganizationAppService.GetOrganizationPictureByOrganizationTenant(CurrentTenant.Id);

        }
    }

    private async Task Fetch()
    {
        var customer = await CustomerAppService.GetAsync(Guid.Parse(customerId));

        customerDataDto = new CreateUpdateCustomerDto
        {
            ArabicName = customer.ArabicName,
            Email = customer.Email,
            FullName = customer.FullName,
            IdentificationType = customer.identificationTypes,
            MobileNumber = customer.MobileNumber,
            NationalId = customer.NationalId,
            UserName = customer.UserName,
            CustomerCategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customer.CustomerCategory)
        };
    }

    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>

    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
    public void CancelView()
    {
        Navigation.NavigateTo("/customers", true);

    }

    private async Task<IDialogReference> ViewVehicle()
    {
        

        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };


        return await DialogService.ShowAsync<ViewCustomerVehicleDialog>("view customer vehicle", parameters);
    }  
    private async Task<IDialogReference> ViewInvoice()
    {
        

        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };


        return await DialogService.ShowAsync<ViewCustomerInvoiceDialog>("view customer invoice", parameters);
    }    
    private async Task<IDialogReference> ViewPackage()
    {
        

        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };


        return await DialogService.ShowAsync<ViewCustomerPackageDialog>("view customer package", parameters);
    }

}
