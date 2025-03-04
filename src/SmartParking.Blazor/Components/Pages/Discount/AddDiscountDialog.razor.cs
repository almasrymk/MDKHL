using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Discounts;
using SmartParking.Lookups;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Discount;

public partial class AddDiscountDialog
{
    private MudForm form;
    public CreateUpdateDiscountDto input = new CreateUpdateDiscountDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {

        try
        {
            CreateUpdateDiscountDto createUpdateDiscountDto = new CreateUpdateDiscountDto()
            {
                ArabicName = input.ArabicName,
                EnglishName = input.EnglishName,
                DiscountPercent = input.DiscountPercent,
                Notes = input.Notes,
                Status= Enums.Status.Inactive,
                TenantId=currrentTenant.Id
            };

            var discount = await DiscountAppService.CreateAsync(createUpdateDiscountDto);
            if (discount != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateDiscountDto.EnglishName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/discounts", forceLoad: true);

            }

            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateDiscountDto.ArabicName}", Severity.Error);

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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
