
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using SmartParking.LookupsAppServices;
using SmartParking.Organizations;
using SmartParking.Subscriptions;
using SmartParking.Utilities;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;
namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationAdd
{
    private MudForm form;
    public string UserName { get; set; }
    public string ArabicName { get; set; } 
    public string EnglishName { get; set; }
    public string CommercialRegistrationNumber { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private string CountrySelectValue {  get; set; }
    private string PackageSelectValue {  get; set; }
    private string CitySelectValue {  get; set; }
    private string StateSelectValue {  get; set; }
    public string RegionSelectValue { get; set; }
    private DateTime? CRExpiryDate { get; set; }
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
    public bool Status { get; set; } = true;
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
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfCustomers { get; set; }
    public int SubscriptionSlotNumber { get; set; }
    public int ParkingSlot { get; set; }

    private SubscriptionDto? SelectedPackage;
    public int FreeMinutes { get; set; }
    public int Graceperiod { get; set; }
    private string NewAddress { get; set; }
    public string TaxIdentificationNumber { get; set; }
    public string NationalId { get; set; }


    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    IList<SubscriptionDto> packages = new List<SubscriptionDto>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        countries = await CountriesAppService.GetCountryLookupAsync();
        packages = await SubscriptionAppService.GetAllAsync();
    }
    
    private async Task GetRegions(string value)
    {
         CountrySelectValue = value;
        if (CountrySelectValue != null)
        {
            regions = await RegionAppService.GetRegionByCountry(int.Parse(CountrySelectValue));
        }
         
    }    
    private async Task GetCities(string value)
    {
         StateSelectValue = value;
        if (StateSelectValue != null)
        {
            cities = await CitiesAppService.GetCityByState(int.Parse(StateSelectValue));
        }
        
    }
	private async Task GetStates(string value)
	{
		RegionSelectValue = value;
		if (RegionSelectValue != null)
		{
			states = await StateAppService.GetStateLookupByRegionAsync(int.Parse(RegionSelectValue));

		}

	}


    private async Task OnPackageSelected(string value)
    {
        PackageSelectValue = value;

        SelectedPackage = packages.FirstOrDefault(p => p.Id.ToString() == value);

        if (SelectedPackage != null)
        {
            NumbersOfGarages = SelectedPackage.NumbersOfGarages;
            NumberOfCameras = SelectedPackage.NumberOfCameras;
            NumberOfUsers = SelectedPackage.NumberOfUsers;
            NumberOfCustomers = SelectedPackage.NumberOfCustomers;
            SubscriptionSlotNumber = SelectedPackage.SubscriptionSlotNumber;
            ParkingSlot = SelectedPackage.ParkingSlot; 
        }
        else
        {
            // Reset values if no package is selected
            NumbersOfGarages = 0;
            NumberOfCameras = 0;
            NumberOfUsers = 0;
            NumberOfCustomers = 0;
            SubscriptionSlotNumber = 0;
        }
        StateHasChanged();
    }

    private async Task Save()
    {

		Status StatusValue;
        if (Status)
        {
            StatusValue = Enums.Status.Active;
        }
        else
        {
            StatusValue = Enums.Status.Inactive;
        }
       
        CreateUpdateOrganiztionDto createUpdateOrganiztion = new CreateUpdateOrganiztionDto()
        {
            ArabicName = ArabicName,
            EnglishName = EnglishName,
            CommercialRegistrationNumber = CommercialRegistrationNumber,
            NumOfCameras = NumOfCameras,
            MaxCapacity = MaxCapacity,
            CityId = int.Parse(CitySelectValue),
            RegionId = int.Parse(RegionSelectValue),
            StateId = int.Parse(StateSelectValue),
            Email = ContactMail,
            CountryId = int.Parse(CountrySelectValue),
            CRExpiryDate = DateOnly.FromDateTime(CRExpiryDate ?? DateTime.MinValue),
            Status = StatusValue,
            PhoneNumber = PhoneNum,
            IsNumberOfHoursExceeded = IsNumberOfHoursExceeded,
            IsClose = IsClose,
            WhatsAppNum = WhatsAppNum,
            FirstHourPrice = FirstHourPrice,
            SecondHourPrice = SecondHourPrice,
            ThirdHourPrice = ThirdHourPrice,
            AdditionalHourPrice = AdditionalHourPrice,
            FullDayPrice = FullDayPrice,
            NumberOfHours = NumberOfHours > 0 ? NumberOfHours : 0,
            ClosingHoursStart = ClosingHoursStart.HasValue
            ? TimeOnly.FromTimeSpan(ClosingHoursStart.Value)
            : null,
            ClosingHoursEnd = ClosingHoursEnd.HasValue
            ? TimeOnly.FromTimeSpan(ClosingHoursEnd.Value)
            : null,
            UserName = UserName,
            StreetName = StreetName,
            District = District,
            BuildingNumber = BuildingNumber,
            PostalCode = PostalCode,
            SecondaryNumber = SecondaryNumber,
            OrganizationPicture = await FileHelper.SaveImageFromBase64Async(OrganizationPicture, PathHelper.OrganizationFolder),
            FreeMinutes = FreeMinutes,
            Address = NewAddress,
            Graceperiod = Graceperiod,
            TaxNumber=TaxIdentificationNumber,
            SubscriptionId= int.Parse(PackageSelectValue),
            NationalId=NationalId,
            

        };
       
        
        try
        {
            var oragnization =  await OrganizationAppService.CreateAsync(createUpdateOrganiztion);
            await OrganizationAppService.AddOrganizationForSubscription(oragnization.Id,int.Parse(PackageSelectValue));
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]} {L["Organization"]} {oragnization.ArabicName}", Severity.Success);
            Navigation.NavigateTo("/organizations");
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
    }
    private void Cancel()
    {
        Navigation.NavigateTo("/organizations");
    }

    private async Task OnImageUpload(IBrowserFile file)
    {
        var imageFile = file;
        if (imageFile != null)
        {
            // Save the image and get the file path
            OrganizationPicture = await ImageHelper.GetImageBase64Async(imageFile);
            StateHasChanged();
        }
    }

}
