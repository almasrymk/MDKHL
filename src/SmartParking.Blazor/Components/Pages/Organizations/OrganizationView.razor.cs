using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationView
{
    private string CityName { get; set; }
    private string CountryName{ get; set; }
    private string RegionName { get; set; }
    private string StateName { get; set; }
    private string Code { get; set; }
    [Parameter]
    public string Id { get; set; }  // Capture string ID
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
    public int Status { get; set; } = 1;//1-> active , 2-> inactive 3->deleted 
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public string Address { get; set; }
    public int BuildingNumber { get; set; }
    public string StreetName { get; set; }
    public int SecondaryNumber { get; set; }
    public int PostalCode { get; set; }
    public string District { get; set; }
    public string Photo { get; set; }
    public string OrganizationPicture { get; set; }

    private async Task FetchData()
    {
        var id = int.Parse(Id);
        var organization = await OrganizationsAppService.GetAsync(id);
        EnglishName = organization.EnglishName;
        ArabicName = organization.ArabicName;
        NumOfCameras = organization.NumOfCameras;
        CommercialRegistrationNumber = organization.CommercialRegistrationNumber;
        Address = organization.Address;
        OrganizationPicture = organization.OrganizationPicture;
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
             StateName=organization.StateName;
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
        ContactMail = organization.ContactMail;
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        LicenseExpiryDate = organization.LicenseExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        PhoneNum = organization.PhoneNum;
        WhatsAppNum = organization.WhatsAppNum;
        StateName=organization.StateName;
        Code = organization.Code;
        BuildingNumber = organization.BuildingNumber;
        StreetName = organization.StreetName;
        SecondaryNumber = organization.SecondaryNumber;
        PostalCode = organization.PostalCode;
        District = organization.District;
    }
    protected override async Task OnInitializedAsync()
    {
        await FetchData();




        await base.OnInitializedAsync();


    }
    private void Back()
    {
        Navigation.NavigateTo("/organizations");
    }
    private async Task ViewCategories()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CategoriesDialog>("view garage", parameters);
    }

    private async Task ViewContact()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<ContactDataDialog>("view contact", parameters);
    }
    private async Task ViewGarages()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<GarageDialog>("view garage", parameters);
    }
    private async Task ViewCustomers()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CustomerDialog>("view customer", parameters);
    }
    private async Task ViewUsers()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<UsersDialog>("view users", parameters);
    }
}