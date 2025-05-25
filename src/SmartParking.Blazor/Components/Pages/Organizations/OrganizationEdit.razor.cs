using AutoMapper;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using SmartParking.Organizations;
using SmartParking.Permissions.OrganizationPermissions;
using SmartParking.Subscriptions;
using SmartParking.Utilities;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;
using YamlDotNet.Core.Tokens;

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
    private string CountrySelectValue { get; set; }
    private string CitySelectValue { get; set; }
    public string RegionSelectValue { get; set; }
    public string StateSelectValue { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    public TimeSpan? ClosingHoursStart { get; set; }
    public TimeSpan? ClosingHoursEnd { get; set; }
    public bool Status { get; set; } = true;
    public string District { get; set; }
    private string SubscriptionSelectValue { get; set; }
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfCustomers { get; set; }
    public int SubscriptionSlotNumber { get; set; }
    public int NumberOfHours { get; set; }
    public int Parkingslot { get; set; }
    private SubscriptionDto SelectedSubscription = new SubscriptionDto();
    public string ViewSubscriptionArabicName { get; set; }
    public string Code { get; set; }
    public string OrganizationPicture { get; set; }

    private bool HasChangeStatusPermission;
    private bool IsPictureChanged;

    private CreateUpdateOrganiztionDto _createUpdateOrganiztionDto = new CreateUpdateOrganiztionDto();

    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    IList<SubscriptionDto> SubscriptionDtos = new List<SubscriptionDto>();
    public OrganizationDto OrganizationDto { get; set; } = new();
    private readonly IMapper _mapper;

    public OrganizationEdit(IMapper mapper)
    {
        _mapper = mapper;
    }

    private async Task OnSubscriptionSelected(int value)
    {

        _createUpdateOrganiztionDto .SubscriptionId =  value;
        SelectedSubscription = SubscriptionDtos.FirstOrDefault(p => p.Id == value);
        // Trigger UI update
        StateHasChanged();


    }
    private async Task Update()
    {
        var id = int.Parse(Id);
        _createUpdateOrganiztionDto.ClosingHoursStart = TimeOnly.FromTimeSpan(ClosingHoursStart ?? TimeSpan.MinValue);
        _createUpdateOrganiztionDto.ClosingHoursEnd = TimeOnly.FromTimeSpan(ClosingHoursEnd ?? TimeSpan.MaxValue);
        _createUpdateOrganiztionDto.RegionId = int.Parse(RegionSelectValue);
        _createUpdateOrganiztionDto.StateId = int.Parse(StateSelectValue);
        _createUpdateOrganiztionDto.CityId = int.Parse(CitySelectValue);
        _createUpdateOrganiztionDto.CountryId = int.Parse(CountrySelectValue);
        _createUpdateOrganiztionDto.CRExpiryDate = DateOnly.FromDateTime(CRExpiryDate ?? DateTime.MinValue);
        _createUpdateOrganiztionDto.NumberOfHours = NumberOfHours;
        _createUpdateOrganiztionDto.Status = Status ? Enums.Status.Active : Enums.Status.Inactive;

        try
        {
            _createUpdateOrganiztionDto.IsClose = PackageValidations.GetOrganizationIsCloseAfterTimeValidity(_createUpdateOrganiztionDto.IsClose, _createUpdateOrganiztionDto.ClosingHoursStart, _createUpdateOrganiztionDto.ClosingHoursEnd);

            if (IsPictureChanged)
                _createUpdateOrganiztionDto.OrganizationPicture = await FileHelper.SaveImageFromBase64Async(OrganizationPicture, PathHelper.OrganizationFolder);
            var oragnization = await OrganizationAppService.UpdateAsync(id, _createUpdateOrganiztionDto);

            Navigation.NavigateTo("/organizations");
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {L["Organization"]} {_createUpdateOrganiztionDto.ArabicName}", Severity.Success);

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
        Navigation.NavigateTo("/organizations");
    }
    private async Task FetchData()
    {

        var id = int.Parse(Id);
        var organization = await OrganizationAppService.GetAsync(id);
        SelectedSubscription = await SubscriptionAppService.GetAsync(organization.SubscriptionId);
        _createUpdateOrganiztionDto = _mapper.Map<CreateUpdateOrganiztionDto>(organization);
        if (organization.Status == Enums.Status.Active)
        {
            Status = true;
        }
        else
        {
            Status = false;
        }
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            CityNameEdit = organization.CityName;
            CountryNameEdit = organization.CountryName;
            RegionNameEdit = organization.RegionName;
            StateNameEdit = organization.StateName;

        }
        else
        {
            CityNameEdit = organization.CityNameAr;
            CountryNameEdit = organization.CountryNameAr;
            RegionNameEdit = organization.RegionNameAr;
            StateNameEdit = organization.StateNameAr;
        }
        StateSelectValue = organization.StateId.ToString();
        CRExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        ClosingHoursStart = organization.ClosingHoursStart.ToTimeSpan();
        ClosingHoursEnd = organization.ClosingHoursEnd.ToTimeSpan();
        CitySelectValue = organization.CityId.ToString();
        CountrySelectValue = organization.CountryId.ToString();
        RegionSelectValue = organization.RegionId.ToString();
        Code= organization.Code;
        SubscriptionSelectValue = organization.SubscriptionId.ToString();
        ViewSubscriptionArabicName = organization.SubscriptionArabicName;
        NumbersOfGarages = SelectedSubscription.NumbersOfGarages;
        NumberOfCameras = SelectedSubscription.NumberOfCameras;
        NumberOfUsers = SelectedSubscription.NumberOfUsers;
        NumberOfCustomers = SelectedSubscription.NumberOfCustomers;
        SubscriptionSlotNumber = SelectedSubscription.ParkingSlot;
        Parkingslot = SelectedSubscription.ParkingSlot;

    }


    private async Task GetRegions()
    {

        if (CountrySelectValue != null)
        {
            regions = await RegionAppService.GetRegionByCountry(int.Parse(CountrySelectValue));
        }

    }
    private async Task GetCities()
    {

        if (StateSelectValue != null)
        {
            cities = await CitiesAppService.GetCityByState(int.Parse(StateSelectValue));
        }

    }
    private async Task GetStates()
    {

        if (RegionSelectValue != null)
        {
            states = await StateAppService.GetStateLookupByRegionAsync(int.Parse(RegionSelectValue));

        }

    }
    protected override async Task OnInitializedAsync()
    {
        HasChangeStatusPermission = await AuthorizationService.IsGrantedAsync(OrganizationPermissions.ChanageStatus);
        await FetchData();

        countries = await CountriesAppService.GetCountryLookupAsync();

        await GetRegions();

        await GetStates();

        await GetCities();
        SubscriptionDtos = await SubscriptionAppService.GetAllAsync();

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
        var dialog = await Dialog.ShowAsync<CustomerDialog>("view package", parameters);
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
