using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.Shared.Utilities;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Currency;
using SmartParking.CustomerPackages;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using static QRCoder.PayloadGenerator;

using SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerPackage
{
    /// <summary>
    /// Component for viewing and managing customer packages in a dialog.
    /// </summary>
    public partial class ViewCustomerPackageDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        [Parameter]
        public bool UserAccess { get; set; }

        private const string RLM = "\u200F"; 
        private const string LRM = "\u200E"; 
        private void Cancel() => MudDialog.Cancel();
        private int TotalItem;
        private TableData itemData = new TableData();

        private readonly IStringLocalizer<SmartParkingResource> _localizer;

        /// <summary>
        /// Constructor for ViewCustomerPackageDialog.
        /// </summary>
        /// <param name="localizer">The localizer used for string translations.</param>
        public ViewCustomerPackageDialog(IStringLocalizer<SmartParkingResource> localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Fetches the customer package data for the table.
        /// </summary>
        /// <param name="pageInfo">The page information including page number, rows per page, and sorting information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task FetchTableData(PageChangeEventArgs pageInfo)
        {
            var input = new PagedAndSortedAndSearchResultRequestDto
            {
                MaxResultCount = pageInfo.RowsPerPage,
                SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
                Sorting = pageInfo.SortColumn
            };

            PagedResultDto<CustomerPackageDto> customerPackageDto = new();

            if (CurrentTenant.Id == null)
            {
                customerPackageDto = await CustomerPackagesAppService.GetListPackagesByCustomerMdkl(Guid.Parse(CustomerId), input);
            }
            else
            {
                customerPackageDto = await CustomerPackagesAppService.GetListPackagesByCustomerOrganization(Guid.Parse(CustomerId), input);
            }

            TotalItem = (int)customerPackageDto.TotalCount;
            CreateTable(customerPackageDto.Items.ToList());
        }

        /// <summary>
        /// Initializes the table data for displaying customer packages.
        /// </summary>
        /// <param name="customerPackages">List of customer package DTOs to populate the table.</param>
        private void CreateTable(List<CustomerPackageDto> customerPackages)
        {
            var tableData = new TableData()
            {
                Name = L["CustomerPackageHeader"],
                ShowHeader = true
            };

            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PackageName"], Name = "PackageArabicName", IsFilterable = false, PropertyName = "PackageArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PackageName"], Name = "PackageEnglishName", IsFilterable = false, PropertyName = "PackageEnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }

            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["customer category"], Name = "CustomerCategory", IsFilterable = false, PropertyName = "CustomerCategory", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Price"], Name = "PriceOfPackage", IsFilterable = false, PropertyName = "PriceOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PackageDuration"], Name = "DurationOfPackage", IsFilterable = false, PropertyName = "DurationOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ActivationDate"], Name = "ActivationDate", IsFilterable = false, PropertyName = "ActivationDate", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ExpiryDate"], Name = "ExpiryDate", IsFilterable = false, PropertyName = "ExpiryDate", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = false, TypeFilter = TypeFilter.Text });

            if (CultureInfo.CurrentCulture.Name == "en")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "EnPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "ArPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "CustomerPackageStatus", IsSortable = false, TypeFilter = TypeFilter.Enum });

            foreach (var item in customerPackages)
            {
                var rowDef = new RowDef();
                rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PackageName"], item.PackageArabicName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PackageName"], item.PackageEnglishName));
                }
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["customer category"], item.CustomerCategory.ToString()));
                var iconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
                rowDef.Cells.Add(new KeyValuePair<string, string>(
                   $"{LocalizationHelper.GetLocalizedString("Price")}",
                   item.PriceOfPackage != null ? $"{item.PriceOfPackage.ToString()} {iconHtml}" : $"0 {iconHtml}"
                ));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PackageDuration"], item!.DurationOfPackage.ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ActivationDate"], DateOnly.FromDateTime(item!.ActivationDateOfPackage.Value).ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ExpiryDate"], DateOnly.FromDateTime(item!.ExpiryDate.Date).ToString()));
                string publishedText;

                if (item.publishedOfPackage.HasValue && item.publishedOfPackage.Value)
                {
                    publishedText = $"24 {_localizer["Hour"]}";
                }
                else
                {
                    var from = item.PublishFromOfPackage?.ToString() ?? "-";
                    var to = item.PublishToOfPackage?.ToString() ?? "-";
                    publishedText = $"{from}-{to}";
                }

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Published"], publishedText));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], $"{RLM}{item.ArPlateNumber} , {LRM}{item.EnPlateNumber}"));
                CustomerPackageStatus displayStatus = BlazorPackageValidations.GetDisplayStatus(item);

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{displayStatus.ToString()}"]));
                
                
                tableData.Rows.Add(rowDef);
            }
            itemData = tableData;
        }

        private async Task HandleDownload(RowDef row)
        {
            var customerPackage = await CustomerPackagesAppService.GetAsync(int.Parse(row.Cells[0].Value));
            if (string.IsNullOrEmpty(customerPackage.QRCodePath))
                return;
            await JSRuntime.InvokeVoidAsync("window.open", $"{customerPackage.QRCodePath}", "_blank");

        }

        private async Task OpenAssignDialog()
        {
            var parameters = new DialogParameters
            {
                ["CustomerId"] = CustomerId
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<AssignCustomerUnitPackage>(L["AssignPackage"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await FetchTableData(new PageChangeEventArgs { PageNumber = 1, RowsPerPage = 10 });
            }
        }

        /// <summary>
        /// Handles the delete action for a customer package.
        /// </summary>
        /// <param name="row">The row containing the customer package to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleDelete(RowDef row)
        {
            var id = int.Parse(row.Cells[0].Value);
            var Name = row.Cells[2].Value;

            var parameters = new DialogParameters
            {
                ["Name"] = Name,
            };

            var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete package", parameters);
            var result = await dialog.Result;

            try
            {
                if (!result.Canceled)
                {
                    await CustomerPackagesAppService.DeleteAsync(id);
                    SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE) + " " +
                        LocalizationHelper.GetLocalizedString("Package"), Severity.Success);

                    await Task.Delay(1000);

                    Navigation.Refresh(true);
                }
            }
            catch (BusinessException ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
            }
            catch (AuthenticationException ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
            }
            catch (Exception ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
            }
        }
    }
}
