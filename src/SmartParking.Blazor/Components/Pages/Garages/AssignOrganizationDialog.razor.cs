using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Garages
{
    partial class AssignOrganizationDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateGarageDto Garage { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        public List<OrganizationDto> Organizations { get; set; } = new List<OrganizationDto>();
        bool _clearable;
        private MudForm form;

        protected override async Task OnInitializedAsync()
        {
            var result = await OrganizationsAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            Organizations = result.Items.ToList();

            //await base.OnInitializedAsync();
        }

        private async Task SubmitEdit()
        {

            try
            {

                var tenantId = Organizations.Find(x => x.Id == Garage.OrganizationId).TenantId;
                Garage.TenantId = tenantId;
                var garage = await GaragesAppService.UpdateAsync(UpdatedId, Garage);
                var output = await GaragesAppService.ChangeTenantForCamerasRelatedToGarage(garage.Id, tenantId);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ASSIGN]} ", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                Navigation.NavigateTo("/garages");

            }
            catch (Exception e)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ASSIGN]}", Severity.Error);
            }
        }

        private async Task CancelEdit()
        {
            MudDialogAdd.Cancel();


        }


    }
}
