using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using SmartParking.Utilities;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.VehicleModels;

public partial class EditVehicleModel
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }

    [Parameter] public CreateUpdateVehicleModelDto VehicleModel { get; set; }
    [Parameter]
    public int UpdatedId { get; set; }
    bool _clearable;
    bool IsPictureChanged;
    private MudForm form;
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    private string ModelPicture { get; set; }

    private async Task OnImageUpload(IBrowserFile file)
    {
        var imageFile = file;
        if (imageFile != null)
        {
            // Save the image and get the file path
            ModelPicture = await ImageHelper.GetImageBase64Async(imageFile);
            IsPictureChanged = true;
            StateHasChanged();
        }
    }
    protected override async Task OnInitializedAsync()
    {
        var result = await VehiclesBrandsAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = (1 - 1) * 100
        });
        VehicleBrands = result.Items.ToList();
        ModelPicture = VehicleModel.Image;
        await base.OnInitializedAsync();
    }

    private async Task SubmitEdit()
    {


        try
        {
            if (IsPictureChanged)
                VehicleModel.Image = await FileHelper.SaveImageFromBase64Async(ModelPicture, PathHelper.ModelFolder);
            await VehicleModelsAppService.UpdateAsync(UpdatedId, VehicleModel);
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {VehicleModel.ArabicName} ", Severity.Success);
            MudDialogAdd.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
            Navigation.NavigateTo("/vehicleModels", true);
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

    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();


    }

}
