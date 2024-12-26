using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Entities.Organizations;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class UpdateGarageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    [Parameter] public CreateUpdateGarageDto Garage { get; set; }
    [Parameter]
    public int UpdatedId { get; set; }
    private TimeSpan? _time = new TimeSpan();
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    protected async override Task OnInitializedAsync()
    {
        _time = Garage.ClosingHours.ToTimeSpan();
        if (currrentUser.IsInRole(Enums.RolesType.Mdkhl.ToString()) || currrentUser.IsInRole("admin"))
        {

            var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            Organizations = result.Items.ToList();
        }

    }

    private async Task SubmitEdit()
    {

        try
        {
            Garage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);

            await GarageAppService.UpdateAsync(UpdatedId, Garage);
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {Garage.NameEn}", Severity.Success);
            MudDialogAdd.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
            Navigation.NavigateTo("/garages", true);


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
    }

    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();


    }


    private void HandleMaxCapacityChange(int maxCapacity)
    {
        Garage.MaxCapacity = maxCapacity;
        Garage.MaxSlotsForEmployess = 0;
        Garage.MaxSlotsForCustomers = 0;
        Garage.MaxSlotsForFree = 0;
    }

    private void HandleEmployeeSlotsChange(int maxSlotsForEmployees)
    {
        Garage.MaxSlotsForEmployess = maxSlotsForEmployees;
        Disrtibute();
    }

    private void HandleCustomerSlotsChange(int maxSlotsForCustomers)
    {
        Garage.MaxSlotsForCustomers = maxSlotsForCustomers;
        Disrtibute();
    }

    private void HandleFreeSlotsChange(int maxSlotsForFree)
    {
        Garage.MaxSlotsForFree = maxSlotsForFree;
        Disrtibute();
    }

    private void Disrtibute()
    {
        int MaxSlotsForEmployess = Garage.MaxSlotsForEmployess;
        int MaxSlotsForCustomers = Garage.MaxSlotsForCustomers;
        int MaxSlotsForFree = Garage.MaxSlotsForFree;
        NumbersValidations.DistributeRemainingCapacity(ref MaxSlotsForEmployess, ref MaxSlotsForCustomers, ref MaxSlotsForFree, Garage.MaxCapacity);
        Garage.MaxSlotsForEmployess = MaxSlotsForEmployess;
        Garage.MaxSlotsForFree = MaxSlotsForFree;
        Garage.MaxSlotsForCustomers = MaxSlotsForCustomers;
    }

}
