using Microsoft.AspNetCore.Components;
using MudBlazor;
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

    private MudForm form;
    private bool isSlotsReadOnly = true;


    TimeEditMode editMode = new TimeEditMode();

    protected override async Task OnInitializedAsync()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        if (currrentUser.IsInRole(Enums.RolesType.Mdkhl.ToString())||currrentUser.IsInRole("admin"))
        {

            var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            Organizations = result.Items.ToList();
        }
    }
    private async void SubmitCreate()
    {
		var successedMessage = L["GarageCreatedSuccessfully"];
		var errorMessage = L["Can'tCreateGarage"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
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
                ShowSnackbar($"{successedMessage}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                NavigationManager.NavigateTo("/garages",true);
            }
            else
            {
                ShowSnackbar($"{errorMessage}", Severity.Error);


            }
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
            ShowSnackbar($"{errorMessage} : {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage} : {ex.Message} + ", Severity.Error);
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
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }

    private string validateNumber(int value)
    {
        if (value < 0)
            return "The value not valid.";
        if (value > 9999)
            return "The value not valid.";

        return null; // No error
    }
    private void HandleMaxCapacityChange(int maxCapacity)
    {
        createUpdateGarage.MaxCapacity = maxCapacity;

        // Reset all slots to zero to start fresh
        createUpdateGarage.MaxSlotsForEmployess = 0;
        createUpdateGarage.MaxSlotsForCustomers = 0;
        createUpdateGarage.MaxSlotsForFree = 0;
    }

    private void HandleEmployeeSlotsChange(int maxSlotsForEmployees)
    {
        createUpdateGarage.MaxSlotsForEmployess = maxSlotsForEmployees;
        DistributeRemainingCapacity();
    }

    private void HandleCustomerSlotsChange(int maxSlotsForCustomers)
    {
        createUpdateGarage.MaxSlotsForCustomers = maxSlotsForCustomers;
        DistributeRemainingCapacity();
    }

    private void HandleFreeSlotsChange(int maxSlotsForFree)
    {
        createUpdateGarage.MaxSlotsForFree = maxSlotsForFree;
        DistributeRemainingCapacity();
    }

    private void DistributeRemainingCapacity()
    {
        int totalAllocated = createUpdateGarage.MaxSlotsForEmployess
                           + createUpdateGarage.MaxSlotsForCustomers
                           + createUpdateGarage.MaxSlotsForFree;

        int remainingCapacity = createUpdateGarage.MaxCapacity - totalAllocated;

        if (remainingCapacity < 0)
        {
            // If total slots exceed MaxCapacity, reset to prevent overflow
            createUpdateGarage.MaxSlotsForEmployess = 0;
            createUpdateGarage.MaxSlotsForCustomers = 0;
            createUpdateGarage.MaxSlotsForFree = 0;
        }
        else
        {
            // Distribute remaining slots dynamically
            if (createUpdateGarage.MaxSlotsForEmployess > 0)
            {
                if(createUpdateGarage.MaxSlotsForCustomers > 0)
                {
                    if (createUpdateGarage.MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            // If total slots exceed MaxCapacity, reset to prevent overflow
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }
                    }
                    else
                    {
                        createUpdateGarage.MaxSlotsForFree = createUpdateGarage.MaxCapacity- createUpdateGarage.MaxSlotsForEmployess- createUpdateGarage.MaxSlotsForCustomers;

                    }

                }
                else if (createUpdateGarage.MaxSlotsForFree>0)
                {
                    if (createUpdateGarage.MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                        createUpdateGarage.MaxSlotsForCustomers=createUpdateGarage.MaxCapacity - createUpdateGarage.MaxSlotsForEmployess - createUpdateGarage.MaxSlotsForFree;
                    }

                }
            }
            else if (createUpdateGarage.MaxSlotsForCustomers > 0)
            {
                if (createUpdateGarage.MaxSlotsForFree > 0)
                {
                    if (createUpdateGarage.MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                      createUpdateGarage.MaxSlotsForEmployess = createUpdateGarage.MaxCapacity - createUpdateGarage.MaxSlotsForCustomers - createUpdateGarage.MaxSlotsForFree;
                    }
                }
                else if (createUpdateGarage.MaxSlotsForEmployess > 0)
                {
                    if (createUpdateGarage.MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {
                       createUpdateGarage.MaxSlotsForFree = createUpdateGarage.MaxCapacity - createUpdateGarage.MaxSlotsForEmployess - createUpdateGarage.MaxSlotsForCustomers;

                    }

                }
            }
            else if(createUpdateGarage.MaxSlotsForFree > 0)
            {
                if (createUpdateGarage.MaxSlotsForEmployess > 0)
                {
                    if (createUpdateGarage.MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {
                    createUpdateGarage.MaxSlotsForCustomers = createUpdateGarage.MaxCapacity - createUpdateGarage.MaxSlotsForEmployess - createUpdateGarage.MaxSlotsForFree;

                    }
                }
                else if (createUpdateGarage.MaxSlotsForCustomers > 0)
                {
                    if (createUpdateGarage.MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            createUpdateGarage.MaxSlotsForEmployess = 0;
                            createUpdateGarage.MaxSlotsForCustomers = 0;
                            createUpdateGarage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                    createUpdateGarage.MaxSlotsForEmployess = createUpdateGarage.MaxCapacity - createUpdateGarage.MaxSlotsForFree - createUpdateGarage.MaxSlotsForCustomers;
                    }

                }
            }
        }
    }
}
