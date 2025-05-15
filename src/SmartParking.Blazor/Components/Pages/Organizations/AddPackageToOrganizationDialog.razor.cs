using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Organizations;
using SmartParking.Packages;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Organizations
{
    partial class AddPackageToOrganizationDialog
    {
        [Parameter] public int organizationId { get; set; }

        private MudForm form;
        private CreateUpdatePackageDto createUpdatePackage = new CreateUpdatePackageDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;
        private List<OrganizationDto> organizations = new List<OrganizationDto>();
        private List<PackageTypeDto> packageTypes = new List<PackageTypeDto>();
        private TimeSpan? timeSpanFrom;
        private TimeSpan? timeSpanTo;
        //public List<PackageDto> Garages { get; set; } = new List<GarageDto>();

        protected override async Task OnInitializedAsync()
        {
            var organizationsList = await organizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            organizations = organizationsList.Items.ToList();
            var packageTypesList = await packageTypesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            packageTypes = packageTypesList.Items.ToList();
            timeSpanFrom = createUpdatePackage.PublishFrom.ToTimeSpan();
            timeSpanTo = createUpdatePackage.PublishTo.ToTimeSpan();
            await base.OnInitializedAsync();
        }
        private void OnTimeFromChanged(TimeSpan? value)
        {
            // Handle null case and update CreateUpdatePackage.PublishFrom
            if (value.HasValue)
            {
                timeSpanFrom = value;
                createUpdatePackage.PublishFrom = TimeOnly.FromTimeSpan(value.Value);
            }
        }
        private void OnTimeToChanged(TimeSpan? value)
        {
            // Handle null case and update CreateUpdatePackage.PublishTo
            if (value.HasValue)
            {
                timeSpanTo = value;
                createUpdatePackage.PublishTo = TimeOnly.FromTimeSpan(value.Value);
            }
        }
        private async Task Submit()
        {
            var successedMessage = L["PackageCreatedSuccessfully"];
            var errorMessage = L["FillRequriedItems"];
            var validationErrorMessage = L["PleaseFixTheValidationErrors"];

            if (form.IsValid)
            {
                // Call your service to create the camera
                try
                {
                    createUpdatePackage.OrganizationId = organizationId;
                    await PackagesAppService.CreateAsync(createUpdatePackage);
                }
                catch (AbpValidationException ex)
                {
                    foreach (var validationError in ex.ValidationErrors)
                    {
                        Snackbar.Add($"{validationError.MemberNames.First()}: {validationError.ErrorMessage}", Severity.Error);
                    }
                }
                catch (Exception e)
                {
                    Snackbar.Add(errorMessage, Severity.Error);
                    return;
                }
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));

            }
            else
            {
                Snackbar.Add(validationErrorMessage, Severity.Error);
            }
        }
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}
