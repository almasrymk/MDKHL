using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras
{
    partial class EditCamera
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateCameraDto Camera { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        public List<GarageDto> Garages { get; set; } = new List<GarageDto>();
        bool _clearable;
        private MudForm form;
        private int OrgId { get; set; }
        private string GarageCode { get; set; }
        private int GarageId { get; set; }
        IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();
        IList<CameraModelDto> CameraModels = new List<CameraModelDto>();
        IList<CameraMainTypeDto> CameraMainTypes = new List<CameraMainTypeDto>();
        protected override async void OnInitialized()
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
            var garage = Garages.Find(x => x.Id == Camera.GarageId);
            GarageCode = garage.Code;
            GarageId = garage.Id;
            Camera.OrganizationId = garage.OrganizationId;
        }

        private async Task SubmitEdit()
        {

            try
            {
                if (Camera.GarageId != GarageId)
                {
                    var newGarage = Garages.Find(x => x.Id == Camera.GarageId);
                    Camera.OrganizationId = newGarage.OrganizationId;
                    Camera.TenantId = newGarage.TenantId;
                }
                if (string.IsNullOrEmpty(Camera.DeviceId))
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("VaildtionError")}: {LocalizationHelper.GetLocalizedString("DeviceId")}- {LocalizationHelper.GetLocalizedString("EmptyValidation")}", Severity.Error);
                    return;

                }

                await CamerasAppService.UpdateAsync(UpdatedId, Camera);
                MudDialogAdd.Close(DialogResult.Ok(true));
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {Camera.Name}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/cameras", forceLoad: true);


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


}
