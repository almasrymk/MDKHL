using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Garages;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class AddGarageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    public CreateUpdateGarageDto createUpdateGarage { get; set; }
    private TimeSpan? _time = new TimeSpan();
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();

    TimeEditMode editMode = new TimeEditMode();

    protected override async Task OnInitializedAsync()
    {
        createUpdateGarage = new CreateUpdateGarageDto();
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();
    }
    private async void SubmitCreate()
    {
        try
        {
            createUpdateGarage.Code = "";

            var garage = await GarageAppService.CreateAsync(createUpdateGarage);
            if (garage != null)
            {
                Snackbar.Add("Garage Has been created Successfully", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Can't create garage", Severity.Error);


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
        createUpdateGarage = new CreateUpdateGarageDto();
        _time = new TimeSpan();

        MudDialogAdd.Cancel();

    }



}
