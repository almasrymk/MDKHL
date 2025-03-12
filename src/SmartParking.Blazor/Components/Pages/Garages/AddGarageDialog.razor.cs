using Blazorise;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.AlertMessages;
using SmartParking.Blazor.Components.Shared;
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

public partial class AddGarageDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    public CreateUpdateGarageDto createUpdateGarage { get; set; }
    private TimeSpan? _time = new TimeSpan();
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();

    private MudForm form;

    protected override async Task OnInitializedAsync()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        createUpdateGarage.Status = Enums.Status.Active;
        if (!currrentTenant.Id.HasValue)
        {

            var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            Organizations = result.Items.ToList();
        }
    }
    private async void SubmitCreate()
    {
		try
        {
            if (currrentUser.TenantId != null)
            {
                createUpdateGarage.OrganizationId = await OrganizationAppService.GetOrganizationIdByTenant(currrentUser.TenantId);
            }
            
            createUpdateGarage.Code = "";
            createUpdateGarage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);

            var garage = await GarageAppService.CreateAsync(createUpdateGarage);
            if (garage != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{garage.NameEn} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                NavigationManager.NavigateTo("/garages",true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar,$"{garage.NameEn} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}", Severity.Error);
            }
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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
    }

    private async Task CancelCreate()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        _time = new TimeSpan();

        MudDialogAdd.Cancel();

    }
    private void HandleMaxCapacityChange(int maxCapacity)
    {
        createUpdateGarage.MaxCapacity = maxCapacity;
        createUpdateGarage.MaxSlotsForEmployess = 0;
        createUpdateGarage.MaxSlotsForCustomers = 0;
        createUpdateGarage.MaxSlotsForFree = 0;
    }

    private void HandleEmployeeSlotsChange(int maxSlotsForEmployees)
    {
        createUpdateGarage.MaxSlotsForEmployess = maxSlotsForEmployees;
        Disrtibute();
    }

    private void HandleCustomerSlotsChange(int maxSlotsForCustomers)
    {
        createUpdateGarage.MaxSlotsForCustomers = maxSlotsForCustomers;
        Disrtibute();
    }

    private void HandleFreeSlotsChange(int maxSlotsForFree)
    {
        createUpdateGarage.MaxSlotsForFree = maxSlotsForFree;
        Disrtibute();
    }

    private void Disrtibute()
    {
        int MaxSlotsForEmployess =  createUpdateGarage.MaxSlotsForEmployess;
        int MaxSlotsForCustomers = createUpdateGarage.MaxSlotsForCustomers;
        int MaxSlotsForFree = createUpdateGarage.MaxSlotsForFree;
        NumbersValidations.DistributeRemainingCapacity(ref MaxSlotsForEmployess,ref MaxSlotsForCustomers,ref MaxSlotsForFree,createUpdateGarage.MaxCapacity);
        createUpdateGarage.MaxSlotsForEmployess= MaxSlotsForEmployess;
        createUpdateGarage.MaxSlotsForFree= MaxSlotsForFree;
        createUpdateGarage.MaxSlotsForCustomers= MaxSlotsForCustomers;
    }



}
