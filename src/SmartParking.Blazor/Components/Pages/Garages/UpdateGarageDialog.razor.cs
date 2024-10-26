using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class UpdateGarageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
   [Parameter] public CreateUpdateGarageDto Garage { get; set; }
    [Parameter]
    public  int UpdatedId { get; set; }
    private TimeSpan? _time = new TimeSpan();
    protected override Task OnInitializedAsync()
    {
        _time = Garage.ClosingHours.ToTimeSpan();
        Console.WriteLine(_time);

        return base.OnInitializedAsync();
    }

    private async Task SubmitEdit()
    {
		var successedMessage = L["Garage Updated Successfully"];
		var errorMessage = L["Failed to update garage"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
        {
            Garage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);

            await GarageAppService.UpdateAsync(UpdatedId, Garage);
            ShowSnackbar($"{successedMessage}", Severity.Success);
            MudDialogAdd.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
            Navigation.NavigateTo("/garages", true);


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationErrorMessage}: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"{errorMessage}: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage} : {ex.Message} + ", Severity.Error);
        }
    }

    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();


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
}
