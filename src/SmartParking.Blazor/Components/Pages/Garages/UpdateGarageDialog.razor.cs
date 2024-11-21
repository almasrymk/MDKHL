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
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    protected async override Task OnInitializedAsync()
    {
        _time = Garage.ClosingHours.ToTimeSpan();
        Console.WriteLine(_time);
        if (currrentUser.IsInRole(Enums.RolesType.Mdkhl.ToString()) || currrentUser.IsInRole("admin"))
        {

            var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            Organizations = result.Items.ToList();
        }
        
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
        Garage.MaxCapacity = maxCapacity;

        // Reset all slots to zero to start fresh
        Garage.MaxSlotsForEmployess = 0;
        Garage.MaxSlotsForCustomers = 0;
        Garage.MaxSlotsForFree = 0;
    }

    private void HandleEmployeeSlotsChange(int maxSlotsForEmployees)
    {
        Garage.MaxSlotsForEmployess = maxSlotsForEmployees;
        DistributeRemainingCapacity();
    }

    private void HandleCustomerSlotsChange(int maxSlotsForCustomers)
    {
        Garage.MaxSlotsForCustomers = maxSlotsForCustomers;
        DistributeRemainingCapacity();
    }

    private void HandleFreeSlotsChange(int maxSlotsForFree)
    {
        Garage.MaxSlotsForFree = maxSlotsForFree;
        DistributeRemainingCapacity();
    }

    private void DistributeRemainingCapacity()
    {
        int totalAllocated = Garage.MaxSlotsForEmployess
                           + Garage.MaxSlotsForCustomers
                           + Garage.MaxSlotsForFree;

        int remainingCapacity = Garage.MaxCapacity - totalAllocated;

        if (remainingCapacity < 0)
        {
            // If total slots exceed MaxCapacity, reset to prevent overflow
            Garage.MaxSlotsForEmployess = 0;
            Garage.MaxSlotsForCustomers = 0;
            Garage.MaxSlotsForFree = 0;
        }
        else
        {
            // Distribute remaining slots dynamically
            if (Garage.MaxSlotsForEmployess > 0)
            {
                if (Garage.MaxSlotsForCustomers > 0)
                {
                    if (Garage.MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            // If total slots exceed MaxCapacity, reset to prevent overflow
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }
                    }
                    else
                    {
                        Garage.MaxSlotsForFree = Garage.MaxCapacity - Garage.MaxSlotsForEmployess - Garage.MaxSlotsForCustomers;

                    }

                }
                else if (Garage.MaxSlotsForFree > 0)
                {
                    if (Garage.MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                        Garage.MaxSlotsForCustomers = Garage.MaxCapacity - Garage.MaxSlotsForEmployess - Garage.MaxSlotsForFree;
                    }

                }
            }
            else if (Garage.MaxSlotsForCustomers > 0)
            {
                if (Garage.MaxSlotsForFree > 0)
                {
                    if (Garage.MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                        Garage.MaxSlotsForEmployess = Garage.MaxCapacity - Garage.MaxSlotsForCustomers - Garage.MaxSlotsForFree;
                    }
                }
                else if (Garage.MaxSlotsForEmployess > 0)
                {
                    if (Garage.MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {
                        Garage.MaxSlotsForFree = Garage.MaxCapacity - Garage.MaxSlotsForEmployess - Garage.MaxSlotsForCustomers;

                    }

                }
            }
            else if (Garage.MaxSlotsForFree > 0)
            {
                if (Garage.MaxSlotsForEmployess > 0)
                {
                    if (Garage.MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {
                        Garage.MaxSlotsForCustomers = Garage.MaxCapacity - Garage.MaxSlotsForEmployess - Garage.MaxSlotsForFree;

                    }
                }
                else if (Garage.MaxSlotsForCustomers > 0)
                {
                    if (Garage.MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            Garage.MaxSlotsForEmployess = 0;
                            Garage.MaxSlotsForCustomers = 0;
                            Garage.MaxSlotsForFree = 0;
                        }

                    }
                    else
                    {

                        Garage.MaxSlotsForEmployess = Garage.MaxCapacity - Garage.MaxSlotsForFree - Garage.MaxSlotsForCustomers;
                    }

                }
            }
        }
    }

}
