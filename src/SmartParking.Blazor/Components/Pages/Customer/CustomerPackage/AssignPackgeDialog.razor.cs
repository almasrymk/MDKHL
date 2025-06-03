using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerPackage
{
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
            if (CustomerPackage.CustomerCategory != Customercategory.ToString() && !IsCustom)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("Package")} , " +
                $"{LocalizationHelper.GetLocalizedString("customer category")} " +
                $"{LocalizationHelper.GetLocalizedString("NotSameType")} ", Severity.Error);
                return;
            }

            try
            {
                var vehicle = await VehiclesAppService.GetAsync(VehicleId);
                if (CustomerPackage.CustomerCategory == CustomerCategoryStatus.Customer.ToString())
                {
                    await CustomerPackagesAppService.CheckSubscriptionCustomersLimitsAsync(CustomerPackage.OrganizationId);
                }

                _customerPackageDto.ArPlateNumber = vehicle.ArPlateNumber;
                _customerPackageDto.EnPlateNumber = vehicle.EnPlateNumber;
                _customerPackageDto.CustomerId = Customer.Id;

                // Handle custom package assignment
                if (IsCustom)
                {
                    //Method  to Check Is Custom Varaiable
                     CheckIsCustomProperties(_customerPackageDto);
                    _customerPackageDto.CustomerCategory = Customercategory;
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

                // Set organization and tenant ID based on the selected organization
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

                var assignPackage = await CustomerPackagesAppService.AssignPackageToCustomerWebAsync(CustomerPackage.Id, _customerPackageDto);

                if (!assignPackage)
                    throw new Exception(LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ASSIGN) + " " +
                    LocalizationHelper.GetLocalizedString("Package"));

                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ADD) + " " +
                LocalizationHelper.GetLocalizedString("Package"), Severity.Success);

                MudDialogAdd.Close(DialogResult.Ok(true));

                await Task.Delay(1000);
                Navigation.NavigateTo("/customers", forceLoad: true);
            }
            catch (Exception ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
            }
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

        private void CheckIsCustomProperties (CustomerPackageDto customerPackageDto)
        {
            if(ActivationDate  == null)
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
    }
}
