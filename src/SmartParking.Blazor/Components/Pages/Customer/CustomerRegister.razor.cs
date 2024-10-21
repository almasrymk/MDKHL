using MudBlazor;
using SmartParking.Cutomers;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerRegister
{
    public CreateUpdateCustomerDto createUpdateCustomerDto = new CreateUpdateCustomerDto();
    public async  Task HandleSubmit()
    {
        try
        {
            createUpdateCustomerDto.UserName = createUpdateCustomerDto.MobileNumber;
            var customer =  await CustomerAppService.CreateAsync(createUpdateCustomerDto);
            Console.WriteLine(customer);
            if (customer != null)
            {

                ShowSnackbar($"Customer {createUpdateCustomerDto.FullName} was created successfully",Severity.Success);
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
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }

    public void HandleCancel()
    {
        Navigation.NavigateTo("/customers", true);

    }

}
