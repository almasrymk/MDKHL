using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class UpdateGarageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
   [Parameter] public CreateUpdateGarageDto Garage { get; set; }
    [Parameter]
    public  int UpdatedId { get; set; }
    private TimeSpan? _time = new TimeSpan();
    protected override Task OnInitializedAsync()
    {
        _time = Garage.ClosingHours.ToTimeSpan();

        return base.OnInitializedAsync();
    }

    private async Task SubmitEdit()
    {
        try
        {
            Garage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);

            await GarageAppService.UpdateAsync(UpdatedId, Garage);
            Snackbar.Add("Garage Updated Successfully", Severity.Success);
            MudDialogAdd.Close(DialogResult.Ok(true));

        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();


    }
}
