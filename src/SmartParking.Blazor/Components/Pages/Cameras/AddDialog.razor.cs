using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras
{
    partial class AddDialog
    {
        private MudForm form;
        private CreateUpdateCameraDto cameraDto = new CreateUpdateCameraDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;
        public List<GarageDto> Garages { get; set; } = new List<GarageDto>();

        IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();
        IList<CameraModelDto> CameraModels = new List<CameraModelDto>();
        IList<CameraMainTypeDto> CameraMainTypes = new List<CameraMainTypeDto>();


        protected override async Task OnInitializedAsync()
        {
            CameraBrands = await CameraBrandAppService.GetAllCameraBrandsAsync();
            CameraModels = await CameraModelAppService.GetAllCameraModelsAsync();
            CameraMainTypes = await CameraMainTypeAppService.GetAllCameraMainTypesAsync();
            var result = await GaragesAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            Garages = result.Items.ToList();
            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {
            try
            {
                var garage = Garages.Find(x => x.Id == cameraDto.GarageId);
                if (garage == null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {L["ChooseGarage"]}", Severity.Error);
                }
                else
                {
                    cameraDto.TenantId = garage.TenantId;
                    cameraDto.OrganizationId = garage.OrganizationId;
                    var createdCameraDto = await CamerasAppService.CreateAsync(cameraDto);
                    MudDialog.Close(DialogResult.Ok(true));
                    SnackBar.ShowSnackbar(Snackbar, $"{createdCameraDto.Name} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo("/cameras", forceLoad: true);
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

                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{L[AlertMessages.AlertMessages.ADD]} : {ex.Code}", Severity.Error);
            }
            catch (AuthenticationException ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{L[AlertMessages.AlertMessages.ADD]}: {ex.Message} ", Severity.Error);
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
}
