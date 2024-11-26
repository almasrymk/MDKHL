using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Organizations;
using System.Globalization;
using System.Threading.Tasks;
using System;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using System.Collections.Generic;

namespace SmartParking.Blazor.Components.Pages.MyAccount;

public partial class ViewMyAccount
{
    private bool CountryClicked = false;
    private string CityNameEdit { get; set; }
    private string CountryNameEdit { get; set; }
    private string RegionNameEdit { get; set; }
    private string StateNameEdit { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string CommercialRegistrationNumber { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private string CountrySelectValue { get; set; }
    private string CitySelectValue { get; set; }
    public string RegionSelectValue { get; set; }
    public string StateSelectValue { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    private DateTime? LicenseExpiryDate { get; set; }
    public decimal FirstHourPrice { get; set; }
    public decimal SecondHourPrice { get; set; }
    public decimal ThirdHourPrice { get; set; }
    public decimal AdditionalHourPrice { get; set; }
    public decimal FullDayPrice { get; set; }
    public bool IsNumberOfHoursExceeded { get; set; }
    public int NumberOfHours { get; set; }
    public bool IsClose { get; set; }
    public TimeSpan? ClosingHoursStart { get; set; }
    public TimeSpan? ClosingHoursEnd { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public string Address { get; set; }
    public int BuildingNumber { get; set; }
    public string StreetName { get; set; }
    public int SecondaryNumber { get; set; }
    public int PostalCode { get; set; }
    public string District { get; set; }
    public string OrganizationPicture { get; set; }
    public string Photo { get; set; }
    public string Code { get; set; }
    public int FreeMinutes { get; set; }
    private string CityName { get; set; }
    private string CountryName { get; set; }
    private string RegionName { get; set; }
    private string StateName { get; set; }
    public int Status { get; set; } = 1;

    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    private async Task FetchData()
    {
        var tenant = currenttenant.Id;
        var id = await OrganizationAppService.GetOrganizationIdByTenant(tenant);
        var organization = await OrganizationAppService.GetAsync(id);
        EnglishName = organization.EnglishName;
        ArabicName = organization.ArabicName;
        NumOfCameras = organization.NumOfCameras;
        CommercialRegistrationNumber = organization.CommercialRegistrationNumber;
        if (organization.Status == Enums.Status.Active)
        {
            Status = 1;
        }
        else if (organization.Status == Enums.Status.Inactive)
        {
            Status = 2;
        }
        else
        {
            Status = 3;
        }
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            CountryName = organization.CountryName;
            RegionName = organization.RegionName;
            StateName = organization.StateName;
            CityName = organization.CityName;
        }
        else
        {

            CountryName = organization.CountryNameAr;
            RegionName = organization.RegionNameAr;
            StateName = organization.StateNameAr;
            CityName = organization.CityNameAr;
        }
        MaxCapacity = organization.MaxCapacity;
        CountryNameEdit = organization.CountryName;
        CityNameEdit = organization.CityName;
        RegionNameEdit = organization.RegionName;
        StateSelectValue = organization.StateId.ToString();
        ContactMail = organization.ContactMail;
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        PhoneNum = organization.PhoneNum;
        WhatsAppNum = organization.WhatsAppNum;
        CitySelectValue = organization.CityId.ToString();
        CountrySelectValue = organization.CountryId.ToString();
        RegionSelectValue = organization.RegionId.ToString();
        FirstHourPrice = organization.FirstHourPrice;
        SecondHourPrice = organization.SecondHourPrice;
        ThirdHourPrice = organization.ThirdHourPrice;
        FullDayPrice = organization.FullDayPrice;
        NumberOfHours = organization.NumberOfHours;
        IsNumberOfHoursExceeded = organization.IsNumberOfHoursExceeded;
        FullDayPrice = organization.FullDayPrice;
        ClosingHoursStart = organization.ClosingHoursStart.ToTimeSpan();
        ClosingHoursEnd = organization.ClosingHoursEnd.ToTimeSpan();
        AdditionalHourPrice = organization.AdditionalHourPrice;
        Address = organization.Address;
        BuildingNumber = organization.BuildingNumber;
        StreetName = organization.StreetName;
        SecondaryNumber = organization.SecondaryNumber;
        PostalCode = organization.PostalCode;
        District = organization.District;
        IsClose = organization.IsClose;
        OrganizationPicture = organization.OrganizationPicture;
        Code = organization.Code;
        Address = organization.Address;
        FreeMinutes = organization.FreeMinutes;


    }


    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
        regions = await StateAppService.GetRegionLookupAsync();
        countries = await CountriesAppService.GetCountryLookupAsync();
        cities = await CitiesAppService.GetCityLookupAsync();
        await FetchData();
        await base.OnInitializedAsync();


    }
    private async Task ViewGarages()
    {
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<GarageDialog>("view garage", parameters);
    }
    private async Task ViewCategories()
    {
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CategoriesDialog>("view garage", parameters);
    }
    private async Task ViewContact()
    {
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<ContactDataDialog>("view contact", parameters);
    }
    private async Task ViewCustomers()
    {
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CustomerDialog>("view package", parameters);
    }
    private async Task ViewUsers()
    {
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<UsersDialog>("view users", parameters);
    }
    private async Task ViewSubscription()
    {
        var Org = await OrganizationAppService.GetOrganizationForCurrentTenat();
        int? subsId = Org.SubscriptionId;

        var subscription = await SubscriptionAppService.GetAsync(Org.SubscriptionId ?? 0);
        //change it to garage.subscription

        var parameters = new DialogParameters
        {
            ["subscription"] = subscription
        };
        await Dialog.ShowAsync<OrganizationSubscriptionDialog>("view DetailsSubscription", parameters);
    }

    private void Navigate()
    {
        Navigation.NavigateTo("/editMyAccount");
    }
}
