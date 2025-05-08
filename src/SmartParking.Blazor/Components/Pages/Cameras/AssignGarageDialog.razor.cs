using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cameras;
using SmartParking.Garages;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Cameras
{
    partial class AssignGarageDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateCameraDto Camera { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        public List<GarageDto> Garages { get; set; } = new List<GarageDto>();
        bool _clearable;
        private MudForm form;

        protected override async Task OnInitializedAsync()
        {
            var result = await GaragesAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            Garages = result.Items.ToList();
            await base.OnInitializedAsync();
        }

        private async Task SubmitEdit()
        {

            try
            {
                Camera.TenantId = Garages.Find(x => x.Id == Camera.GarageId).TenantId;
                await CamerasAppService.UpdateAsync(UpdatedId, Camera);

                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ASSIGN]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));

            }

            catch (Exception ex)
            {

                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ASSIGN]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
            }
        }

        private async Task CancelEdit()
        {
            MudDialogAdd.Cancel();


        }

    }
}
