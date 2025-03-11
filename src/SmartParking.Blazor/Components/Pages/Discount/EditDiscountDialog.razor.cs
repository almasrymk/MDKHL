using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Discounts;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Discount;

public partial class EditDiscountDialog
{

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public DiscountDto discount { get; set; } = new DiscountDto();
    private MudForm editForm;
    [Parameter] public int Id { get; set; }

    private async Task Save()
    {

        try
        {
            // Create the input object for update
            var input = new CreateUpdateDiscountDto
            {
                ArabicName = discount.ArabicName,
                EnglishName = discount.EnglishName,
                DiscountPercent = discount.DiscountPercent,
                Notes = discount.Notes,
                Status = discount.Status,
                TenantId = currrentTenant.Id
            };

            // Call the update method

            var discountt = await DiscountAppService.UpdateAsync(discount.Id, input);

            if (discountt != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/discounts", forceLoad: true);
            }

            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName}", Severity.Success);

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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }

    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
