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
using SmartParking.Blazor.Components.Pages.Customer.Packages;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class UpdateCustomerDialog
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

    public async Task SubmitEdit()
    {
        var successedMessage = L["CustomerUpdateSuccessfullyWith"];
        try
        {
            var customer = await CustomerAppService.UpdateAsync(Guid.Parse(customerId), customerDataDto);
            if (customer != null)
            {

                ShowSnackbar($" {successedMessage} {customerDataDto.FullName}", Severity.Success);
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
            }
            ShowSnackbar(errors, Severity.Error);

        }
        catch (BusinessException businessException)
        {
            string errors = string.Empty;
            foreach (var error in businessException.Message)
            {
                errors += error;
            }
            ShowSnackbar(errors, Severity.Error);

        }
        catch (Exception ex)
        {

            ShowSnackbar(ex.Message, Severity.Error);

        }

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
