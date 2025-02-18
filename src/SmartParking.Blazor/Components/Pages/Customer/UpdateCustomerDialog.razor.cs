using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Customers;
using SmartParking.Cutomers;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using System.Linq;
using Volo.Abp.Validation;
using SmartParking.Enums;
using SmartParking.Blazor.Components.Pages.Customer.Vehicles;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Authentication;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Permissions.CustomerPermissions;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class UpdateCustomerDialog
{
    [Parameter]
    public string customerId { get; set; }
    public CreateUpdateCustomerDto customerDataDto { get; set; } = new CreateUpdateCustomerDto();
    public string Picture { get; set; }
    public Guid IdentityUserId { get; set; }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(CustomerPermissions.Edit))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await Fetch();

        Picture = await UsersAppService.GetImage(IdentityUserId) ?? "images/users/149071.png";

    }

    private async Task Fetch()
    {
        var customer = await CustomerAppService.GetAsync(Guid.Parse(customerId));
        IdentityUserId = customer.IdentityId;


        customerDataDto = new CreateUpdateCustomerDto
        {
            ArabicName = customer.ArabicName,
            Email = customer.Email,
            FullName = customer.FullName,
            IdentificationType = customer.identificationTypes,
            PhoneNumber = customer.MobileNumber,
            NationalId = customer.NationalId,
            UserName = customer.UserName,
            CustomerCategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customer.CustomerCategory)
         };
    }

    public async Task SubmitEdit()
    {
        
        try
        {
            var customer = await CustomerAppService.UpdateAsync(Guid.Parse(customerId), customerDataDto);
            if (customer != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]}" +" "+
                    $"{L[AlertMessages.AlertMessages.UPDATE]} " +" "+
                     $"{customerDataDto.FullName} ", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/customers",true);
            }


        }
        catch (AbpValidationException abpValidationException)
        {
            string errors = string.Empty;
            foreach (var error in abpValidationException.ValidationErrors)
            {
                errors += error;
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[error.MemberNames.First()]} - {error.ErrorMessage}", Severity.Error);
            }

        }
        catch (BusinessException businessException)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]}: {businessException.Message}", Severity.Error);

        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]}: {ex.Message} ", Severity.Error);

        }

    }

    public void CancelEdit()
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
