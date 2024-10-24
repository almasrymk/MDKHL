using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.State;
using SmartParking.LookupsAppServices;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationAdd
{
    public string ArabicName { get; set; } 
    public string EnglishName { get; set; }
    public string UnifiedLicenseNum { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private string CountrySelectValue {  get; set; }
    private string CitySelectValue {  get; set; }
    private string StateSelectValue {  get; set; }
    public string RegionSelectValue { get; set; }
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
    public bool Status { get; set; } = true;
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }

    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        //
       
        countries = await GovernorateAppService.GetCountryLookupAsync();
        //
    }
    
    private async Task GetRegions(string value)
    {
         CountrySelectValue = value;
        if (CountrySelectValue != null)
        {
            regions = await RegionAppService.GetRegionByCountry(int.Parse(CountrySelectValue));
        }
         
    }    
    private async Task GetStates(string value)
    {
         RegionSelectValue = value;
        if (RegionSelectValue != null)
        {
            states = await StateAppService.GetStateLookupByRegionAsync(int.Parse(RegionSelectValue));
            cities = await CitiesAppService.GetCityByRegion(int.Parse(RegionSelectValue));
        }
         
    }

    private async Task Save()
    {
		var successedMessage = L["OrganiztionCreatedSuccessfully"];
		var errorMessage = L["Can'tCreateOrganiztion"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];

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
            UnifiedLicenseNum = UnifiedLicenseNum,
            NumOfCameras = NumOfCameras,
            MaxCapacity = MaxCapacity,
            CityId = int.Parse(CitySelectValue),
            RegionId = int.Parse(RegionSelectValue),
            StateId = int.Parse(StateSelectValue),
            ContactMail = ContactMail,
            CountryId = int.Parse(CountrySelectValue),
            CRExpiryDate = DateOnly.FromDateTime(CRExpiryDate ?? DateTime.MinValue),
            LicenseExpiryDate = DateOnly.FromDateTime(LicenseExpiryDate ?? DateTime.MinValue),
            Status = StatusValue,
            PhoneNum = PhoneNum,
            WhatsAppNum = WhatsAppNum,
            FirstHourPrice = FirstHourPrice,
            SecondHourPrice = SecondHourPrice,
            ThirdHourPrice = ThirdHourPrice,
            AdditionalHourPrice = AdditionalHourPrice,
            FullDayPrice = FullDayPrice,
            IsNumberOfHoursExceeded = IsNumberOfHoursExceeded,
            NumberOfHours = NumberOfHours,
            IsClose = IsClose,
            ClosingHoursStart = IsClose ? TimeOnly.FromTimeSpan(ClosingHoursStart ?? TimeSpan.MinValue) : TimeOnly.MinValue,
            ClosingHoursEnd = IsClose ? TimeOnly.FromTimeSpan(ClosingHoursEnd ?? TimeSpan.MinValue) : TimeOnly.MaxValue
        };
       
        
        try
        {
           
             
            await OrganizationAppService.CreateAsync(createUpdateOrganiztion);
            
            ShowSnackbar($"{successedMessage} You Can Login Your Password Is P@ssw0rd", Severity.Success);
            Navigation.NavigateTo("/organizations");
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationErrorMessage}: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
       
            ShowSnackbar($"{errorMessage}: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage} : : {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage}: {ex.Message} + ", Severity.Error);
        }
    }
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }
    private void Cancel()
    {
        Navigation.NavigateTo("/organizations");
    }
}
