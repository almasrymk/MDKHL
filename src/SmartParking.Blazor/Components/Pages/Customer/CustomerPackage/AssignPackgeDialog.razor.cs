using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.PrePackageAssign;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.State;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.Discounts;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Results;
using SmartParking.Utilities;
using Volo.Abp.Application.Dtos;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;

/// <summary>
/// Dialog component for assigning a package to a customer.
/// </summary>
partial class AssignPackgeDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    public PackageDto CustomerPackage = new PackageDto();
    public CustomerPackageDto _customerPackageDto = new CustomerPackageDto();
    [Parameter]
    public CustomerDto Customer { get; set; }
    [Parameter]
    public int VehicleId { get; set; }
    public CustomerCategoryStatus Customercategory { get; set; }

    private int organizationId;
    private IReadOnlyList<OrganizationDto> Organizations = new List<OrganizationDto>();
    public List<PackageDto> Packages { get; set; } = new List<PackageDto>();

    bool _clearable;
    private MudForm form;
    public List<PackageDto> selectedPackageDto { get; set; } = new List<PackageDto>();
    private bool IsCustom { get; set; } = false;
    private DateTime? ActivationDate { get; set; }
    private DateTime? DectivationDate { get; set; }
    public TimeSpan? startTime { get; set; }
    public TimeSpan? endTime { get; set; }

    /// <summary>
    /// Retrieves the available packages based on the customer's category.
    /// </summary>
    /// <param name="customerCategory">The category of the customer.</param>
    private async Task getPackage(string customerCategory)
    {
        Customercategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customerCategory);
        if (CurrentTenant.Id is null)
            Packages = await PackagesAppService.GetAllMdkhlAsync(organizationId, Customercategory);
        else
            Packages = await PackagesAppService.GetAllOrganizationAsync(Customercategory);
        selectedPackageDto.Clear();
    }

    /// <summary>
    /// Initializes the component and loads the list of organizations.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items;
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the package assignment to the customer.
    /// </summary>
    private async Task SubmitEdit()
    {
        var package= selectedPackageDto.FirstOrDefault();
        if (CustomerPackage.CustomerCategory != Customercategory.ToString() && !IsCustom)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("Package")} , " +
            $"{LocalizationHelper.GetLocalizedString("customer category")} " +
            $"{LocalizationHelper.GetLocalizedString("NotSameType")} ", Severity.Error);
            return;
        }

       
            var vehicle = await VehiclesAppService.GetAsync(VehicleId);
            if (CustomerPackage.CustomerCategory == CustomerCategoryStatus.Customer.ToString())
            {
                await CustomerPackagesAppService.CheckSubscriptionCustomersLimitsAsync(CustomerPackage.OrganizationId);
            }

            _customerPackageDto.ArPlateNumber = vehicle.ArPlateNumber;
            _customerPackageDto.EnPlateNumber = vehicle.EnPlateNumber;
            _customerPackageDto.CustomerId = Customer.Id;
            _customerPackageDto.CustomerCategory = Customercategory;



        // Handle custom package assignment
        if (IsCustom)
        {
                //Method  to Check Is Custom Varaiable
                 CheckIsCustomProperties(_customerPackageDto);
                _customerPackageDto.publishedOfPackage = true;
                _customerPackageDto.ActivationDateOfPackage = ActivationDate.HasValue && startTime.HasValue
                                                            ? ActivationDate.Value.Date + startTime.Value
                                                            : ActivationDate.Value.Date;
                _customerPackageDto.ExpiryDate = DectivationDate.HasValue && endTime.HasValue
                                                ? DectivationDate.Value.Date + endTime.Value
                                                : DectivationDate.Value.Date;
                _customerPackageDto.IsCustomPackage = true;
                _customerPackageDto.DurationOfPackage = DectivationDate.Value.DayOfYear - ActivationDate.Value.DayOfYear;
                CustomerPackage.Id = default;
                _customerPackageDto.PackageArabicName = AlertMessages.AlertMessages.PackageNameAr;
                _customerPackageDto.PackageEnglishName = AlertMessages.AlertMessages.PackageNameEn;
                _customerPackageDto.PublishFromOfPackage = startTime.HasValue ? TimeOnly.FromTimeSpan(startTime.Value) : null;
                _customerPackageDto.PublishToOfPackage = endTime.HasValue ? TimeOnly.FromTimeSpan(endTime.Value) : null;
                if(ActivationDate == DectivationDate &&endTime.Value  <= startTime.Value)
                    throw new Exception(LocalizationHelper.GetLocalizedString("EndTimeShouldBeGreaterThanStartTime"));
        }
        else
        {
            _customerPackageDto.PackageId = package.Id;
            _customerPackageDto.PriceOfPackage = package.Price;
            _customerPackageDto.PackageArabicName = package.ArabicName;
            _customerPackageDto.PackageEnglishName = package.EnglishName;
            _customerPackageDto.ExpiryDate = DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute);
             _customerPackageDto.PublishFromOfPackage = package.PublishFrom;
             _customerPackageDto.PublishToOfPackage = package.PublishTo;
            _customerPackageDto.ActivationDateOfPackage = DateTime.Now;
            _customerPackageDto.DectivationDateOfPackage = package.DectivationDate;
            _customerPackageDto.DescriptionOfPackage = package.Description;
            _customerPackageDto.DurationOfPackage = package.DurationOfPackage;
            _customerPackageDto.publishedOfPackage = package.Published;


        }
        if (CurrentTenant.Id is null)
        {
            _customerPackageDto.OrganizationId = organizationId;
            _customerPackageDto.TenantId = Organizations.FirstOrDefault(x => x.Id == organizationId).TenantId;
        }
        else
        {
            _customerPackageDto.OrganizationId = Organizations.FirstOrDefault(x => x.TenantId == CurrentTenant.Id).Id;
            _customerPackageDto.TenantId = CurrentTenant.Id;
        }
        _customerPackageDto.Organization = Organizations.FirstOrDefault(x => x.TenantId == CurrentTenant.Id);
        preAssignState.CustomerPackage = _customerPackageDto;
        preAssignState.Customer = Customer;

        Navigation.NavigateTo("/prepayment");
      
    }
    private string ValidateEndTime(TimeSpan? value)
    {
        if (!value.HasValue || !startTime.HasValue)
            return null;

        if (ActivationDate == DectivationDate && value <= startTime)
            return L["EndTimeShouldBeGreaterThanStartTime"];

        return null;
    }
    /// <summary>
    /// Cancels the edit operation and closes the dialog.
    /// </summary>
    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();
    }

    /// <summary>
    /// Handles the package selection change event and updates the selected package.
    /// </summary>
    /// <param name="value">The ID of the selected package.</param>
    private async Task PackageValueChanged(int value)
    {
        selectedPackageDto.Clear();
        selectedPackageDto.Add(Packages.Find(x => x.Id == value));
        CustomerPackage = Packages.Find(x => x.Id == value);
        
    }

    private void CheckIsCustomProperties(CustomerPackageDto customerPackageDto)
    {
        if (ActivationDate == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("StartDate") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (DectivationDate == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("EndDate") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (startTime == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("StartTime") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (endTime == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("EndTime") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (customerPackageDto.PriceOfPackage == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Price") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
    }

    private void OnActivationDateChanged(DateTime? newDate)
    {
        ActivationDate = newDate;
        OnDateChanged();
    }

    private void OnDectivationDateChanged(DateTime? newDate)
    {       
        DectivationDate = newDate;
        OnDateChanged();
    }

    private void OnPriceOfPackageChanged(decimal? amount)
    {
        _customerPackageDto.PriceOfPackage = amount;
        if (_customerPackageDto.CountOfDays > 0)
            _customerPackageDto.PriceOfPackagePerDay = _customerPackageDto.PriceOfPackage / _customerPackageDto.CountOfDays;
    }

    private void OnPriceOfPackagePerDayChanged(decimal? amount)
    {
        _customerPackageDto.PriceOfPackagePerDay = amount;
        if (_customerPackageDto.CountOfDays > 0)
            _customerPackageDto.PriceOfPackage = _customerPackageDto.PriceOfPackagePerDay * _customerPackageDto.CountOfDays;
    }

    private void OnCountOfDaysChanged(decimal? count)
    {
        _customerPackageDto.CountOfDays = count;
        if (ActivationDate != null && _customerPackageDto.CountOfDays > 0)
        {
            DectivationDate = ActivationDate;
            if (_customerPackageDto.CountOfDays.Value - 1 > 0)
                DectivationDate = ActivationDate.Value.AddDays((int)_customerPackageDto.CountOfDays.Value - 1);
            if (_customerPackageDto.PriceOfPackagePerDay > 0)
            {
                _customerPackageDto.PriceOfPackage = _customerPackageDto.PriceOfPackagePerDay * _customerPackageDto.CountOfDays;
            }
        }
    }

    private void OnDateChanged()
    {
        _customerPackageDto.CountOfDays = 0;       
        if (ActivationDate != null && DectivationDate != null)
        {
            _customerPackageDto.CountOfDays = (decimal)DectivationDate.Value.Subtract(ActivationDate.Value).TotalDays + 1;
            if(_customerPackageDto.PriceOfPackagePerDay > 0 )
            {
                _customerPackageDto.PriceOfPackage = _customerPackageDto.PriceOfPackagePerDay * _customerPackageDto.CountOfDays;
            }
        }
    }
}
