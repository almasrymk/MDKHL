using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Organizations;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationView
{
    
    private string CityNameEdit { get; set; }
    private string CountryNameEdit { get; set; }
    private string RegionNameEdit { get; set; }
    private string StateNameEdit { get; set; }
    [Parameter]
    public string Id { get; set; }  // Capture string ID
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string UnifiedLicenseNum { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private int CountrySelectValue { get; set; }
    private int CitySelectValue { get; set; }

    public int RegionSelectValue { get; set; }
    public int StateSelectValue { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    private DateTime? LicenseExpiryDate { get; set; }

    public bool Status { get; set; } = true;
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    private void SelectState(string state)
    {
        if (state != "0")
            StateSelectValue = int.Parse(state);
    }
    private void SelectRegion(string region)
    {
        if (region != "0")
            RegionSelectValue = int.Parse(region);
    }
    private void SelectCountry(string country)
    {
        if (country != "0")
            CountrySelectValue = int.Parse(country);
    }
    private void SelectCity(string city)
    {
        if (city != "0")
            CitySelectValue = int.Parse(city);
    }
   private void Back()
    {
        Navigation.NavigateTo("/organizations");
    }
    private async Task FetchData()
    {

        var id = int.Parse(Id);
        var organization = await OrganizationAppService.GetAsync(id);
        EnglishName = organization.EnglishName;
        ArabicName = organization.ArabicName;
        NumOfCameras = organization.NumOfCameras;
        UnifiedLicenseNum = organization.UnifiedLicenseNum;
        if (organization.Status == Enums.Status.Active)
        {
            Status = true;
        }
        else
        {
            Status = false;
        }
        MaxCapacity = organization.MaxCapacity;
        CountryNameEdit = organization.CountryName;
        CityNameEdit = organization.CityName;
        RegionNameEdit = organization.RegionName;
        RegionNameEdit = organization.StateName;
        ContactMail = ContactMail;
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        LicenseExpiryDate = organization.LicenseExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        PhoneNum = PhoneNum;
        WhatsAppNum = WhatsAppNum;
        CitySelectValue = organization.CityId;
        CountrySelectValue = organization.CountryId;
        RegionSelectValue = organization.RegionId;
        StateSelectValue = organization.StateId;
    }
    protected override async Task OnInitializedAsync()
    {

        await FetchData();
        await base.OnInitializedAsync();


    }
}