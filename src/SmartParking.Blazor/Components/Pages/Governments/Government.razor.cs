using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Governments;

public partial class Government
{

    public IReadOnlyList<GovernorateDto> items = Array.Empty<GovernorateDto>();
    public IReadOnlyList<CountryLookupDto> countries = Array.Empty<CountryLookupDto>();
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };


    protected override async Task OnInitializedAsync()
    {
        var input = new PagedAndSortedResultRequestDto();
        await base.OnInitializedAsync();
        items = (await AppService.GetListAsync(input)).Items;
        countries = (await AppService.GetCountryLookupAsync()).Items;
    }

    
    private async Task OpenCreateGovernmentModal(DialogOptions dialogOptions)
    {
         await DialogService.ShowAsync<AddGovernmentDialog>("Add",dialogOptions);
    }

    private async Task OpenEditGovernmentModal(DialogOptions dialogOptions)
    {
        await DialogService.ShowAsync<EditGovernmentDialog>("Edit", dialogOptions);

    }

    //private MudDialogInstance MudDialog { get; set; }
    //private void HandleSubmit()
    //{
    //    MudDialog.Close(DialogResult.Ok(model));
    //}
    //private void Submit() => MudDialog.Close(DialogResult.Ok(model));

    //private void Cancel() => MudDialog.Cancel();
}

