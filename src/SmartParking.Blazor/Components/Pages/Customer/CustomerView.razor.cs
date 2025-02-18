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

public partial class CustomerView
{
    [Parameter]
    public string customerId { get; set; }
    public CreateUpdateCustomerDto customerDataDto { get; set; } = new CreateUpdateCustomerDto();
    public string Picture { get; set; }
    public Guid IdentityUserId { get; set; }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(CustomerPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await Fetch();

        
            Picture =await UsersAppService.GetImage(IdentityUserId) ?? "images/users/149071.png";

       
    }

    private async Task Fetch()
    {
        var customer = await CustomerAppService.GetAsync(Guid.Parse(customerId));
        IdentityUserId= customer.IdentityId;
        customerDataDto = new CreateUpdateCustomerDto
        {
            ArabicName = customer.ArabicName,
            Email = customer.Email,
            FullName = customer.FullName,
            IdentificationType = customer.identificationTypes,
            PhoneNumber = customer.MobileNumber,
            NationalId = customer.NationalId,
            UserName = customer.UserName,

        };
    }
    public void CancelView()
    {
        Navigation.NavigateTo("/customers", false);

    }

    private async Task<IDialogReference> ViewVehicle()
    {
        

        var parameters = new DialogParameters
        {
            ["CustomerId"] = customerId,
        };


        return await DialogService.ShowAsync<ViewCustomerVehicleDialog>("view customer vehicle", parameters );
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
