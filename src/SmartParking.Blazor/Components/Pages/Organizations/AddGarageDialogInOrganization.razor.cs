using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Garages;
using SmartParking.OrganizationsGarages;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class AddGarageDialogInOrganization
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    [Parameter]
    public int OrganizationId { get; set; }
    public CreateUpdateGarageDto createUpdateGarage { get; set; }
    private TimeSpan? _time = new TimeSpan();

    TimeEditMode editMode = new TimeEditMode();

    protected override async Task OnInitializedAsync()
    {
        createUpdateGarage = new CreateUpdateGarageDto();

    }
    private async void SubmitCreate()
    {
        try
        {
            createUpdateGarage.Code = "";
            createUpdateGarage.OrganizationId = OrganizationId;
            var garage = await GarageAppService.CreateAsync(createUpdateGarage);
            if (garage != null)
            {
                ShowSnackbar("Garage Has been created Successfully", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
            }
            else
            {
                ShowSnackbar("Can't create garage", Severity.Error);


            }
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"Validation failed: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to add organizagaragetion: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to add garage: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to add garage: {ex.Message} + ", Severity.Error);
        }
    }

    private async Task CancelCreate()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        _time = new TimeSpan();

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
