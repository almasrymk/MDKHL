using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using SmartParking.Organizations;
using SmartParking.Permissions.ContactPermissions;
using SmartParking.Permissions.GaragePermissions;
using SmartParking.Permissions.MyAccount;
using SmartParking.Permissions.OrganizationPermissions;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;
using Image = SixLabors.ImageSharp.Image;
using ResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using Size = SixLabors.ImageSharp.Size;


namespace SmartParking.Blazor.Components.Pages.MyAccount;

public partial class EditMyAccount
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
    public string Photo { get; set; }
    public string Code { get; set; }
    public int FreeMinutes { get; set; }
    public int Graceperiod { get; set; }
    public int? SubscriptionId { get; set; }
    public bool HasChangeStatusPermission { get; set; }
    public bool IsPictureChanged { get; set; }
    public string TaxIdentificationNumber { get; set; }

    public EditMyAccount()
    {
        CreatePolicyName = GaragePermissions.Create;
        UpdatePolicyName = GaragePermissions.Edit;
        DeletePolicyName = GaragePermissions.Delete;
    }

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
            BuildingNumber = BuildingNumber,
            StreetName = StreetName,
            SecondaryNumber = SecondaryNumber,
            PostalCode = PostalCode,
            District = District,
            TenantId = tenant,
            FreeMinutes = FreeMinutes,
            Graceperiod = Graceperiod,
            SubscriptionId = SubscriptionId,
            OrganizationPicture = OrganizationPicture,
            TaxNumber=TaxIdentificationNumber,
        };

        try
        {
            if (IsPictureChanged)
                createUpdateOrganiztion.OrganizationPicture = await FileHelper.SaveImageFromBase64Async(OrganizationPicture, PathHelper.OrganizationFolder);
            await OrganizationAppService.UpdateAsync(id, createUpdateOrganiztion);
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {L["Organization"]} {createUpdateOrganiztion.ArabicName}", Severity.Success);

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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
    }
    private void Cancel()
    {
            Navigation.NavigateTo("/view-my-account");
    }
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
        Graceperiod = organization.Graceperiod;
        SubscriptionId = organization.SubscriptionId;
        TaxIdentificationNumber= organization.TaxNumber;

    }
    protected override async Task OnInitializedAsync()
    {
        HasChangeStatusPermission = await AuthorizationService.IsGrantedAsync(MyAccountPermissions.ChanageStatus);
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
        var Org = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();
        int? subsId = Org.SubscriptionId;

        var subscription = await SubscriptionAppService.GetAsync(Org.SubscriptionId ?? 0);
        //change it to garage.subscription

        var parameters = new DialogParameters
        {
            ["subscription"] = subscription
        };
        await Dialog.ShowAsync<OrganizationSubscriptionDialog>("view DetailsSubscription", parameters);
    }
    private async Task OnImageUpload(IBrowserFile file)
    {
        var imageFile = file;
        if (imageFile != null)
        {
            OrganizationPicture = await ImageHelper.GetImageBase64Async(imageFile);
            IsPictureChanged = true;
            StateHasChanged();
        }
    }

 
}
