using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using Abp.IO;
using SmartParking.Utilities;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.VehicleModels;

public partial class AddVehicleModel
{
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IVehicleModelsAppService VehicleModelsAppService { get; set; }
    [Inject] IVehicleBrandsAppService VehicleBrandsAppService { get; set; }
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();

    private MudForm form;
    private CreateUpdateVehicleModelDto vehicleModelDto = new CreateUpdateVehicleModelDto();
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    private string ModelPicture { get; set; }
    bool _clearable;

    protected override async Task OnInitializedAsync()
    {
        var result = await VehicleBrandsAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = (1 - 1) * 100
        });
        VehicleBrands = result.Items.ToList();
        await base.OnInitializedAsync();
    }
    private async Task OnImageUpload(IBrowserFile file)
    {
        var imageFile = file;
        if (imageFile != null)
        {
            // Save the image and get the file path
            ModelPicture = await ImageHelper.GetImageBase64Async(imageFile);
            StateHasChanged();
        }
    }
    private async Task Submit()
    {


        try
        {
            vehicleModelDto.Image = await Utilities.FileHelper.SaveImageFromBase64Async(ModelPicture, PathHelper.ModelFolder);
            await VehicleModelsAppService.CreateAsync(vehicleModelDto);
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {vehicleModelDto.ArabicName} ", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
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
