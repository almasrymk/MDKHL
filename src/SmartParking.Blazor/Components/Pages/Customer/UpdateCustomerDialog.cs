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

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class UpdateCustomerDialog
{
    [Parameter]
    public string customerId { get; set; }
    [Parameter]
    public CreateUpdateCustomerDto customerDataDto { get; set; }
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    public string Picture { get; set; }
    protected async override Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();
        if ((currentUser.IsInRole(Enums.RolesType.Mdkhl.ToString())) || (currentUser.IsInRole("admin")))
        {
            Picture = "images/MDHKL_04.png";

        }
        else
        {
            Picture = await OrganizationAppService.GetOrganizationPictureByOrganizationTenant(CurrentTenant.Id);

        }
    }

    public async Task SubmitEdit()
    {
        var successedMessage = L["CustomerUpdateSuccessfullyWith"];
        try
        {
            CustomerDto customer;
            customerDataDto.Password = "P@ssw0rd";
            customerDataDto.RepeatPassword = "P@ssw0rd";
            customer = await CustomerAppService.UpdateAsync(Guid.Parse(customerId), customerDataDto);
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
}
