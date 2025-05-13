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
    /// <summary>
    /// Component for adding a new camera using a dialog form.
    /// </summary>
    public partial class AddDialog
    {
        private CreateUpdateCameraDto cameraDto = new CreateUpdateCameraDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private MudForm form;
        private bool _clearable;
        public List<GarageDto> Garages { get; set; } = new List<GarageDto>();

        private IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();

        
        private IList<CameraModelDto> CameraModels = new List<CameraModelDto>();

     
        private IList<CameraMainTypeDto> CameraMainTypes = new List<CameraMainTypeDto>();

        /// <summary>
        /// Called when the component is initialized. Loads dropdown data for camera brands, models, types, and garages.
        /// </summary>
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

        /// <summary>
        /// Submits the form data to create a new camera. Validates input and shows error or success messages accordingly.
        /// </summary>
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

        /// <summary>
        /// Cancels the operation and closes the dialog without saving.
        /// </summary>
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }

}
