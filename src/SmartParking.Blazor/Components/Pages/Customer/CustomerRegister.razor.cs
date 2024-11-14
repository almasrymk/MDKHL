using MudBlazor;
using SmartParking.Cutomers;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerRegister
{
    private MudForm form;
    public CreateUpdateCustomerDto createUpdateCustomerDto = new CreateUpdateCustomerDto();
    public string Picture { get; set; }


    protected async override Task OnInitializedAsync()
        {
            if ((currentUser.IsInRole(Enums.RolesType.Mdkhl.ToString())) || (currentUser.IsInRole("admin")))
            {
                Picture = "images/MDHKL_04.png";

            }
            else
            {
                Picture = await OrganizationAppService.GetOrganizationPictureByOrganizationTenant(CurrentTenant.Id);

            }
        }

    public async  Task HandleSubmit()
    {
		var successedMessage = L["CustomerCreatedSuccessfullyWith"];
		try
        {
            CustomerDto customer;

			createUpdateCustomerDto.UserName = createUpdateCustomerDto.MobileNumber;
            if(CurrentTenant.Id == null)
            {
				customer = await CustomerAppService.CreateMdkhlAsync(createUpdateCustomerDto);
			}
            else
            {
				customer = await CustomerAppService.CreateAsync(createUpdateCustomerDto);
			}
            if (customer != null)
            {

                ShowSnackbar($" {successedMessage} {createUpdateCustomerDto.FullName}" ,Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/customers");



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

    public void HandleCancel()
    {
        Navigation.NavigateTo("/customers", true);

    }

}
