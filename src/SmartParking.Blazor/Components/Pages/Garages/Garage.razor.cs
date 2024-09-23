using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Blazor.Customs;
using SmartParking.Garages;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class Garage
{
 

    private List<GarageDto> Garages = new List<GarageDto>();
    private GarageDto View;
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };



    private TimeSpan? _time=new TimeSpan();
    CreateUpdateGarageDto createUpdateGarage=new CreateUpdateGarageDto();
    private async Task GetData()
    {
        var response = await GarageAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Garages = response.Items.ToList();

        StateHasChanged();

    }
   
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await GetData();

    }
  
    private  Task CreateGarage()
    {
    

        return DialogService.ShowAsync<AddGarageDialog>("Simple Dialog", options);


    }
    private Task OnClickRowEdit(GarageDto row)
    {
        createUpdateGarage.Name = row.Name; 
        createUpdateGarage.MaxCapacity = row.MaxCapacity;
        createUpdateGarage.ClosingHours =row.ClosingHours;
        createUpdateGarage.Code = row.Code;
        createUpdateGarage.Status = row.Status;
        createUpdateGarage.OrganizationId = row.OrganizationId;

        var parameters = new DialogParameters { 
            ["Garage"] = createUpdateGarage,
            ["UpdatedId"]=row.Id
        };


        return DialogService.ShowAsync<UpdateGarageDialog>("Edit Garage", parameters);
        
    }
    private  Task OnClickRowView(GarageDto row)
    {

        var parameters = new DialogParameters
        {
            ["Garage"] = row,
        };


        return DialogService.ShowAsync<ViewGarageDialog>("View Garage", parameters);
    }


}
