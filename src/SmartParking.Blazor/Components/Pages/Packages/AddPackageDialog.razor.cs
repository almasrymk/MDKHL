using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Lookups;
using SmartParking.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Packages;

public partial class AddPackageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    public CreateUpdatePackageDto createUpdatePackage { get; set; }
    private List<OrganizationDto> organizations = new List<OrganizationDto>();
    private List<PackageTypeDto> packageTypes = new List<PackageTypeDto>();
    private TimeSpan? timeSpanFrom;
    private TimeSpan? timeSpanTo;
    protected override async Task OnInitializedAsync()
    {
        createUpdatePackage = new CreateUpdatePackageDto();
        var organizationsList = await organizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        organizations = organizationsList.Items.ToList();
        var packageTypesList = await packageTypesAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        packageTypes = packageTypesList.Items.ToList();
        timeSpanFrom = createUpdatePackage.PublishFrom.ToTimeSpan();
        timeSpanTo = createUpdatePackage.PublishTo.ToTimeSpan();
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
    private async void SubmitCreate()
    {
        try
        {
            var package = await packagesAppService.CreateAsync(createUpdatePackage);
            if (package != null)
            {
                Snackbar.Add("Package Has been created Successfully", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Can't create Package", Severity.Error);
            }
        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message, Severity.Error);
            Console.WriteLine(e);
        }
    }
    private async Task CancelCreate()
    {
        createUpdatePackage = new CreateUpdatePackageDto();
        MudDialogAdd.Cancel();
    }
  
}
