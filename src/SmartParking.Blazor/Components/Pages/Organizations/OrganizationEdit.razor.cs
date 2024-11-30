using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.State;
using SmartParking.Organizations;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
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
    public int PackageId { get; set; }
    public string District { get; set; }
    public string OrganizationPicture { get; set; }
    public string Photo { get; set; }
    private string PackageSelectValue { get; set; }
    private Guid orgTenant { get; set; }
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfCustomers { get; set; }
    public int SubscriptionSlotNumber { get; set; }
    public int Parkingslot { get; set; }
    private SubscriptionDto SelectedPackage = new SubscriptionDto();
    public string ViewSubscriptionArabicName { get; set; }
    public  int FreeMinutes { get; set; }




    IList<StateLookupDto> states = new List<StateLookupDto>();
    IList<RegionLookupDto> regions = new List<RegionLookupDto>();
    IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    IList<CityLookupDto> cities = new List<CityLookupDto>();
    IList<SubscriptionDto> packages = new List<SubscriptionDto>();


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
            SubscriptionSlotNumber = SelectedPackage.ParkingSlot;
            Parkingslot = SelectedPackage.ParkingSlot;
        }
        else
        {
            // Reset values if no package is selected
            NumbersOfGarages = 0;
            NumberOfCameras = 0;
            NumberOfUsers = 0;
            NumberOfCustomers = 0;
            SubscriptionSlotNumber = 0;
            Parkingslot = 0;
        }

        // Trigger UI update
        StateHasChanged();


    }
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
            CommercialRegistrationNumber = CommercialRegistrationNumber,
            NumOfCameras = NumOfCameras,
            MaxCapacity = MaxCapacity,
            CityId = int.Parse(CitySelectValue),
            RegionId = int.Parse(RegionSelectValue),
            StateId = int.Parse(StateSelectValue),
            ContactMail = ContactMail,
            CountryId = int.Parse(CountrySelectValue),
            CRExpiryDate = DateOnly.FromDateTime(CRExpiryDate ?? DateTime.MinValue),
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
            BuildingNumber = BuildingNumber,
            StreetName = StreetName,
            SecondaryNumber = SecondaryNumber,
            PostalCode = PostalCode,
            District = District,
            OrganizationPicture = OrganizationPicture,
            Address = BuildingNumber + StreetName + SecondaryNumber + District + PostalCode + CityNameEdit ,
            FreeMinutes= FreeMinutes,
        };


        try
        {
            createUpdateOrganiztion.TenantId = orgTenant;


            var oragnization = await OrganizationAppService.UpdateAsync(id, createUpdateOrganiztion);
            if (PackageId != int.Parse(PackageSelectValue))
            {
                await OrganizationAppService.AddOrganizationForSubscription(oragnization.Id, int.Parse(PackageSelectValue));

            }
            Navigation.NavigateTo("/organizations");
            ShowSnackbar(successedMessage, Severity.Success);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
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
        SelectedPackage = await SubscriptionAppService.GetAsync(organization.SubscriptionId);
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
        PackageId = organization.PackageId;
        orgTenant = organization.TenantId;
        FreeMinutes = organization.FreeMinutes;
        PackageSelectValue=organization.SubscriptionId.ToString();
        ViewSubscriptionArabicName = organization.SubscriptionArabicName;
        NumbersOfGarages = SelectedPackage.NumbersOfGarages;
        NumberOfCameras = SelectedPackage.NumberOfCameras;
        NumberOfUsers = SelectedPackage.NumberOfUsers;
        NumberOfCustomers = SelectedPackage.NumberOfCustomers;
        SubscriptionSlotNumber = SelectedPackage.ParkingSlot;
        Parkingslot = SelectedPackage.ParkingSlot;

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
        await FetchData();

        countries = await CountriesAppService.GetCountryLookupAsync();

        await GetRegions();

        await GetStates();

        await GetCities();
        packages = await SubscriptionAppService.GetAllAsync();

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
    private async Task OnImageUpload(IBrowserFile file)
    {
        var imageFile = file;
        if (imageFile != null)
        {
            // Save the image and get the file path
            OrganizationPicture = await SaveImageAndReturnPathAsync(imageFile);
            StateHasChanged();
        }
    }
    private async Task<string> SaveImageAndReturnPathAsync(IBrowserFile imageFile)
    {
        // Ensure directory structure
        var imageDirectoryPath = CreateImageDirectory();

        // Generate a unique file name with the original file extension
        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.Name)}";
        string fullPath = Path.Combine(imageDirectoryPath, fileName);

        // Save the image to the specified path
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            // Copy the image data from the IBrowserFile stream to the file stream
            await imageFile.OpenReadStream().CopyToAsync(fileStream);
        }

        // Return the relative path for storing in the database
        return $"/OrganizationImages/{fileName}";
    }
    private string CreateImageDirectory()
    {
        // Define the path where the image will be saved
        string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "OrganizationImages");

        // Ensure the directory exists
        if (!Directory.Exists(imageDirectoryPath))
        {
            Directory.CreateDirectory(imageDirectoryPath);
        }
        return imageDirectoryPath;
    }
    private string ValidateSecondaryNumber(int value)
    {
        if (value < 1111)
            return "The value not valid.";
        if (value > 9999)
            return "The value not valid.";

        return null; // No error
    }
    private string ValidatePostalCode(int value)
    {
        if (value < 11111)
            return "The value not valid.";
        if (value > 99999)
            return "The value not valid.";

        return null; // No error
    }
    private string ValidateBuildingNumber(int value)
    {
        if (value < 11111)
            return "The value not valid.";
        if (value > 99999)
            return "The value not valid.";

        return null; // No error
    }

}
