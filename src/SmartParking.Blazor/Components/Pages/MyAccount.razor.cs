using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.State;
using SmartParking.Organizations;
using SmartParking.Subscriptions;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages;

public partial class MyAccount
{
    private bool CountryClicked = false;
    private string CityNameEdit { get; set; }
    private string CountryNameEdit { get; set; }
    private string RegionNameEdit { get; set; }
    private string StateNameEdit { get; set; }
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
    private async Task Update()
    {
        var tenant = currenttenant.Id.Value;
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);

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
            ClosingHoursEnd = TimeOnly.FromTimeSpan(ClosingHoursEnd ?? TimeSpan.MinValue),
        };


        try
        {


            await OrganizationAppService.UpdateAsync(id, createUpdateOrganiztion);

            ShowSnackbar("Update Orgainzation Successfully", Severity.Success);
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

            ShowSnackbar($"Failed to add organization: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
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
        
        var tenant = currenttenant.Id;
        var id = await OrganizationAppService.GetOrganizationIdByTenant(tenant);
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
        StateSelectValue = organization.StateId.ToString();
        ContactMail = organization.ContactMail;
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        LicenseExpiryDate = organization.LicenseExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
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
        IsClose = organization.IsClose;
        ClosingHoursStart = organization.ClosingHoursStart.ToTimeSpan();
        ClosingHoursEnd = organization.ClosingHoursEnd.ToTimeSpan();
        AdditionalHourPrice = organization.AdditionalHourPrice;
    }
    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
        regions = await StateAppService.GetRegionLookupAsync();
        countries = await GovernorateAppService.GetCountryLookupAsync();
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
        var id = await OrganizationAppService.GetOrganizationIdByTenant(currenttenant.Id.Value);
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
