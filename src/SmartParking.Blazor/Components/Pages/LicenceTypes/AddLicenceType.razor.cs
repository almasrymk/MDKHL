using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.LicenceTypes
{
    partial class AddLicenceType
    {
        IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();

        private MudForm form;
        private CreateUpdateLicenceTypeDto licenceTypeDto = new CreateUpdateLicenceTypeDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;
        private string Picture { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var result = await PlateColorsAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            plateColors = result.Items.ToList();
            await base.OnInitializedAsync();
        }
        private async Task OnImageUpload(IBrowserFile file)
        {
            var imageFile = file;
            if (imageFile != null)
            {
                // Save the image and get the file path
                Picture = await ImageHelper.GetImageBase64Async(imageFile);
                StateHasChanged();
            }
        }
        private async Task Submit()
        {

            try
            {
                licenceTypeDto.Image = await FileHelper.SaveImageFromBase64Async(Picture, PathHelper.LicenceTypeFolder);
                await LicenceTypesAppService.CreateAsync(licenceTypeDto);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {licenceTypeDto.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/licenceTypes", true);
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
}
