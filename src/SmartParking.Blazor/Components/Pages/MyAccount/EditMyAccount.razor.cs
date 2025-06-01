using AutoMapper;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using SmartParking.Organizations;
using SmartParking.Permissions.GaragePermissions;
using SmartParking.Permissions.MyAccount;
using SmartParking.Utilities;
using SmartParking.Validations;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;


namespace SmartParking.Blazor.Components.Pages.MyAccount;

public partial class EditMyAccount
{
    private bool _isClose { get; set; }
    private string _cityNameEdit { get; set; }
    private string _countryNameEdit { get; set; }
    private string _regionNameEdit { get; set; }
    private string _stateNameEdit { get; set; }
    private string _countrySelectValue { get; set; }
    private string _citySelectValue { get; set; }
    private string _regionSelectValue { get; set; }
    private string _stateSelectValue { get; set; }
    private bool HasChangeStatusPermission { get; set; }
    private bool _isPictureChanged { get; set; }
    private string _organizationPicture { get; set; }
    private int _numberOfHours { get; set; }

    private DateTime? _crExpiryDate { get; set; }
    private TimeSpan? _closingHoursStart { get; set; }
    private TimeSpan? _closingHoursEnd { get; set; }
    private string _code { get; set; }
    private string _address { get; set; }
    private CreateUpdateOrganiztionDto _createUpdateOrganization { get; set; } = new();
    private int _organizationId;
    private readonly IMapper _mapper;
    private bool _status=true;

    public EditMyAccount(IMapper mapper)
    {
        CreatePolicyName = GaragePermissions.Create;
        UpdatePolicyName = GaragePermissions.Edit;
        DeletePolicyName = GaragePermissions.Delete;
        _mapper = mapper;
    }

    IList<StateLookupDto> _states = new List<StateLookupDto>();
    IList<RegionLookupDto> _regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> _countries = new List<CountryLookupDto>();
    IList<CityLookupDto> _cities = new List<CityLookupDto>();

    private async Task Update()
    {
    
        try
        {
            _createUpdateOrganization.IsClose = PackageValidations.GetOrganizationIsCloseAfterTimeValidity(_isClose, _createUpdateOrganization.ClosingHoursStart, _createUpdateOrganization.ClosingHoursEnd);
            if (_isPictureChanged)
                _createUpdateOrganization.OrganizationPicture = await FileHelper.SaveImageFromBase64Async(_organizationPicture, PathHelper.OrganizationFolder);
            await OrganizationAppService.UpdateAsync(_organizationId, _createUpdateOrganization);
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {L["Organization"]} {_createUpdateOrganization.ArabicName}", Severity.Success);
            Navigation.NavigateTo("/view-my-account");

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
        var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();
        _organizationId = organization.Id;
        _organizationPicture = organization.OrganizationPicture ?? "";
        _address=organization.Address;
        _createUpdateOrganization = _mapper.Map<CreateUpdateOrganiztionDto>(organization);
        if (organization.Status == Enums.Status.Active)
        {
            _status = true;
        }
        else
        {
            _status = false;
        }
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            _cityNameEdit = organization.CityName;
            _countryNameEdit = organization.CountryName;
            _regionNameEdit = organization.RegionName;
            _stateNameEdit = organization.StateName;

        }
        else
        {
            _cityNameEdit = organization.CityNameAr;
            _countryNameEdit = organization.CountryNameAr;
            _regionNameEdit = organization.RegionNameAr;
            _stateNameEdit = organization.StateNameAr;
        }
        _stateSelectValue = organization.StateId.ToString();
        _crExpiryDate = organization.CRExpiryDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        _closingHoursStart = organization.ClosingHoursStart.ToTimeSpan();
        _closingHoursEnd = organization.ClosingHoursEnd.ToTimeSpan();
        _citySelectValue = organization.CityId.ToString();
        _countrySelectValue = organization.CountryId.ToString();
        _regionSelectValue = organization.RegionId.ToString();
        _code = organization.Code;



    }
    protected override async Task OnInitializedAsync()
    {
        HasChangeStatusPermission = await AuthorizationService.IsGrantedAsync(MyAccountPermissions.ChanageStatus);
        _states = await StateAppService.GetStateLookupAsync();
        _regions = await StateAppService.GetRegionLookupAsync();
        _countries = await CountriesAppService.GetCountryLookupAsync();
        _cities = await CitiesAppService.GetCityLookupAsync();
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

        var subscription = await SubscriptionAppService.GetAsync(Org.SubscriptionId);

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
            _organizationPicture = await ImageHelper.GetImageBase64Async(imageFile);
            _isPictureChanged = true;
            StateHasChanged();
        }
    }

 
}
