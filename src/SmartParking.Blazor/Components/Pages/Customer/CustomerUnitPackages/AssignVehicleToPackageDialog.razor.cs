using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Currency;
using SmartParking.CustomerUnitPackages;
using SmartParking.Vehicles;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

public partial class AssignVehicleToPackageDialog
{

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        [Parameter] public VehicleDto Vehicle { get; set; }
    [Parameter]
    public Guid CustomerId { get; set; }
        private void Cancel() => MudDialog.Cancel();

        private int TotalItem;
    public int CurrentPage { get; set; } = 1;
        public int RowPerPage { get; set; } = 10;

        private TableData itemData = new TableData();
        private string termSearch;

        private List<CustomerUnitPackageDto> customerUnitPackages = new List<CustomerUnitPackageDto>();

    /// <summary>
    /// Initializes the component and checks if the current tenant is set.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
     protected override async Task OnInitializedAsync()
     {
          await base.OnInitializedAsync();
     }

        /// <summary>
        /// Fetches the table data for the specified page and updates the table with the retrieved data.
        /// </summary>
        /// <param name="page">The page number for which to fetch the data.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task FetchTableData(PageChangeEventArgs pageInfo)
        {


            customerUnitPackages = await CustomerUnitPackageAppService.GetUnitPackageByCustomer(CustomerId);


            CreateTable(customerUnitPackages);

        }
        /// <summary>
        /// Creates a table data structure from a list of vehicle DTOs.
        /// </summary>
        /// <param name="vehicles">The list of vehicle DTOs.</param>
        private void CreateTable(List<CustomerUnitPackageDto> packageDtos)
        {
            var tableData = new TableData()
            {
                Name = L["Packages"],
                ShowHeader = true
            };

            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["PackageName"], Name = "PackageArabicName", IsFilterable = false, PropertyName = "PackageArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["PackageName"], Name = "PackageEnglishName", IsFilterable = false, PropertyName = "PackageEnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }

            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Price"], Name = "PriceOfPackage", IsFilterable = false, PropertyName = "PriceOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["PackageDuration"], Name = "DurationOfPackage", IsFilterable = false, PropertyName = "DurationOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ActivationDate"], Name = "ActivationDate", IsFilterable = false, PropertyName = "ActivationDate", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ExpiryDate"], Name = "ExpiryDate", IsFilterable = false, PropertyName = "ExpiryDate", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Status"], Name = "Status", IsFilterable = false, PropertyName = "CustomerCategory", IsSortable = false, TypeFilter = TypeFilter.Text });

            foreach (var item in packageDtos)
            {
                var rowDef = new RowDef();
                rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["PackageName"], item.EnglishName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["PackageName"], item.ArabicName));
                }
                var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
                rowDef.Cells.Add(new KeyValuePair<string, string>(
                    L["Price"],
                    $"{item.Price} {IconHtml}"));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["PackageDuration"], (item.ActivationDate - item.ActivationDate).Days.ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ActivationDate"], item.ActivationDate.ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ExpiryDate"], item.DectivationDate.ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Published"], item.PublishTo == item.PublishFrom ? 24 + L["Hour"] : $" {item.PublishFrom} " + " - " + $" {item.PublishTo}"));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Status"], item.CustomerPackageStatus.ToString()));

                tableData.Rows.Add(rowDef);
            }
            itemData = tableData;
        }
        

        public async Task HandleAssign(string customerUnitId)
        {
          
            try
            {
                var customerUnitPackage = customerUnitPackages.FirstOrDefault(x => x.Id == int.Parse(customerUnitId));
                var CustomerPackage = customerUnitPackage.ToCreateUpdateCustomerPackageDto(Vehicle.EnPlateNumber, Vehicle.ArPlateNumber);
                var result = await AssignCustomerPackagesAppService.AssignPackageToCustomerAsync(CustomerPackage);
                if (result != null)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L["PackagePaymentSuccessfully"]}", MudBlazor.Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo($"/customers/view/{CustomerId}");
                }
            }
            catch (Exception ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
            }

        }


    
}


