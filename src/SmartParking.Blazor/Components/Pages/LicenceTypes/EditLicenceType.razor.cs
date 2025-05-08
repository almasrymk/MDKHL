using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using System.Security.Authentication;
using Volo.Abp;
using SmartParking.Utilities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.LicenceTypes
{
    partial class EditLicenceType
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }

        [Parameter] public CreateUpdateLicenceTypeDto LicenceType { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        bool _clearable;
        bool IsPictureChanged;
        private MudForm form;
        IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
        private string Picture { get; set; }

        private async Task OnImageUpload(IBrowserFile file)
        {
            var imageFile = file;
            if (imageFile != null)
            {
                // Save the image and get the file path
                Picture = await ImageHelper.GetImageBase64Async(imageFile);
                IsPictureChanged = true;
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var result = await PlateColorsAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            plateColors = result.Items.ToList();
            Picture = LicenceType.Image;

            await base.OnInitializedAsync();
        }

        private async Task SubmitEdit()
        {

            try
            {
                if (IsPictureChanged)
                    LicenceType.Image = await FileHelper.SaveImageFromBase64Async(Picture, PathHelper.LicenceTypeFolder);
                await LicenceTypesAppService.UpdateAsync(UpdatedId, LicenceType);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {LicenceType.ArabicName}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/licenceTypes", forceLoad: true);
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
