using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Garages;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class AddGarageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    public CreateUpdateGarageDto createUpdateGarage { get; set; }
    private TimeSpan? _time = new TimeSpan();
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();

    TimeEditMode editMode = new TimeEditMode();

    protected override async Task OnInitializedAsync()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();
    }
    private async void SubmitCreate()
    {
        try
        {
            createUpdateGarage.Code = "";
            createUpdateGarage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);


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
