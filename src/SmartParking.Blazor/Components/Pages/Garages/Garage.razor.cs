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
    public int updateId { get; set; }
    private bool updateModal=false;
    private bool createModal=false;
    private bool viewModal=false;

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
    private async Task SubmitEdit()
    {
        try
        {
            createUpdateGarage.ClosingHours = TimeOnly.FromTimeSpan(_time ?? TimeSpan.Zero);

            await GarageAppService.UpdateAsync(updateId,createUpdateGarage);
            Snackbar.Add("Garage Updated Successfully", Severity.Success);
        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message,Severity.Error);
        }
        updateModal = false;
    } 
    private async void SubmitCreate()
    {
        try
        {
            createUpdateGarage.Code = "";
            
            var garage= await GarageAppService.CreateAsync(createUpdateGarage);
            if (garage != null)
            {
                Snackbar.Add("Garage Has been created Successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add("Can't create garage", Severity.Error);


            }
        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message,Severity.Error);
            Console.WriteLine(e);
        }
        createModal = false;
    }   

    private async Task CancelCreate()
    {
        createUpdateGarage= new CreateUpdateGarageDto();
        createModal = false;
        _time = new TimeSpan();


    }
    private async Task CancelEdit()
    {
        updateId = 0;
        createUpdateGarage= new CreateUpdateGarageDto();
        updateModal = false;
        _time = new TimeSpan();

    }
    private async Task CreateGarage()
    {
        createModal=true;
        createUpdateGarage= new CreateUpdateGarageDto();
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();


    }
    private async Task OnClickRowEdit(GarageDto row)
    {
        var cellId = row.Id;
        var garage= await GarageAppService.GetAsync(cellId);
        createUpdateGarage.Name = garage.Name; 
        createUpdateGarage.MaxCapacity = garage.MaxCapacity;
        _time=garage.ClosingHours.ToTimeSpan();
        createUpdateGarage.Code = garage.Code;
        createUpdateGarage.Status = garage.Status;
        createUpdateGarage.OrganizationId = garage.OrganizationId;
        updateId = cellId;
        updateModal = true;
    }
    private async Task OnClickRowView(GarageDto row)
    {
        View = row;
        viewModal= true;
    }
    private  void OnCloseViewOnly()
    {
        View = null;
        viewModal= false;

    }
}
