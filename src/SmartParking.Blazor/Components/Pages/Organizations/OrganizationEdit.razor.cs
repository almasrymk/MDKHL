using Microsoft.AspNetCore.Identity;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp.Identity;
using Volo.Abp;
using Abp.Runtime.Validation;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Components;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using SmartParking.Blazor.Components.Pages.Subscriptionss;
using JetBrains.Annotations;
using SmartParking.Subscriptions;
using SmartParking.Entities.Organizations;
using SmartParking.LookupsAppServices;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationEdit
{
    private bool CountryClicked = false;
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
    public bool Status { get; set; } = true;
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }

    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    IList<SubscriptionDto> packages = new List<SubscriptionDto>();
    private async Task Update()
    {
        var successedMessage = L["OrganiztionUpdatedSuccessfully"];
        var errorMessage = L["Can'tUpdateOrganiztion"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        var id = int.Parse(Id);

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
            ClosingHoursStart = TimeOnly.FromTimeSpan(ClosingHoursStart ?? TimeSpan.MinValue),
            ClosingHoursEnd = TimeOnly.FromTimeSpan(ClosingHoursEnd ?? TimeSpan.MaxValue),

        };


        try
        {
           
           
           var oragnization =  await OrganizationAppService.UpdateAsync(id,createUpdateOrganiztion);
            ShowSnackbar(successedMessage, Severity.Success);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"Validation failed: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"{errorMessage} {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage} {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage} {ex.Message} + ", Severity.Error);
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
        CountryNameEdit =  organization.CountryName;
        CityNameEdit = organization.CityName;
        RegionNameEdit = organization.RegionName;
        StateSelectValue = organization.StateId.ToString();
        ContactMail = organization.ContactMail;
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        LicenseExpiryDate = organization.LicenseExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        PhoneNum = organization.PhoneNum;
        WhatsAppNum = organization.WhatsAppNum;
        CitySelectValue = organization.CityId.ToString();
        CountrySelectValue = organization.CountryId.ToString();
        RegionSelectValue = organization.RegionId.ToString();
        FirstHourPrice=organization.FirstHourPrice;
        SecondHourPrice=organization.SecondHourPrice;
        ThirdHourPrice=organization.ThirdHourPrice;
        FullDayPrice=organization.FullDayPrice;
        NumberOfHours=organization.NumberOfHours;
        IsNumberOfHoursExceeded = organization.IsNumberOfHoursExceeded;
        FullDayPrice = organization.FullDayPrice;
        ClosingHoursStart = organization.ClosingHoursStart.ToTimeSpan();
        ClosingHoursEnd = organization.ClosingHoursEnd.ToTimeSpan();
        AdditionalHourPrice = organization.AdditionalHourPrice;
    }
    private async Task GetRegions()
    {
        
        if (CountrySelectValue != null)
        {
            regions = await RegionAppService.GetRegionByCountry(int.Parse(CountrySelectValue));
        }

    }
    private async Task GetStates()
    {
       
        if (RegionSelectValue != null)
        {
            states = await StateAppService.GetStateLookupByRegionAsync(int.Parse(RegionSelectValue));
            cities = await CitiesAppService.GetCityByRegion(int.Parse(RegionSelectValue));
        }

    }
    protected override async Task OnInitializedAsync()
    {
        
        countries = await GovernorateAppService.GetCountryLookupAsync(); 
        await FetchData();
     //   await base.OnInitializedAsync();


    }
    private async Task ViewGarages()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<GarageDialog>("view garage",parameters);
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
 
    private async Task ViewCustomers()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
		var dialog = await Dialog.ShowAsync<CustomerDialog>("view package", parameters);
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
    private async Task ViewSubscription()
    {
        var id = int.Parse(Id);
        //change it to garage.subscription
        var org = await OrganizationAppService.GetAsync(id);
        var subscription = new SubscriptionDto();
        if (org.SubscriptionId != 0)
        {
          subscription = await SubscriptionAppService.GetAsync(org.SubscriptionId);

        }
        else
        {
            subscription = null;
        }

        var parameters = new DialogParameters
        {
            ["subscription"] = subscription
        };
        await Dialog.ShowAsync<OrganizationSubscriptionDialog>("view DetailsSubscription", parameters);
    }
}
