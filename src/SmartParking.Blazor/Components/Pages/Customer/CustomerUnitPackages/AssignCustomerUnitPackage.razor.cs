using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.UnitPackages;
using SmartParking.CustomerUnitPackages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using SmartParking.Currency;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages
{
    public partial class AssignCustomerUnitPackage
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        [Inject]
        public IUnitPackageAppService UnitPackageAppService { get; set; }

        [Inject]
        public ICustomerUnitPackageAppService CustomerUnitPackageAppService { get; set; }

        [Inject]
        public IStringLocalizer<SmartParkingResource> L { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        private int TotalItem;
        private TableData itemData = new TableData();

        private void Cancel() => MudDialog.Cancel();

        private async Task FetchTableData(PageChangeEventArgs pageInfo)
        {
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = pageInfo.RowsPerPage,
                SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
                Sorting = pageInfo.SortColumn
            };

            var result = await UnitPackageAppService.GetListAsync(input);
            TotalItem = (int)result.TotalCount;
            await CreateTable(result);
        }

        private async Task CreateTable(PagedResultDto<UnitPackageDto> packages)
        {
            var tableData = new TableData()
            {
                Name = L["Packages"],
                ShowHeader = true
            };

            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Price"], Name = "Price", IsFilterable = false, PropertyName = "Price", IsSortable = true, TypeFilter = TypeFilter.Text, TypeOfColumn = typeof(decimal) });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

            foreach (var item in packages.Items)
            {
                var rowDef = new RowDef();
                rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["EnglishName"], item.Package.EnglishName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["ArabicName"], item.Package.ArabicName));
                }
                var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
                rowDef.Cells.Add(new KeyValuePair<string, string>(
                    L["Price"],
                    $"{item.Package.Price} {IconHtml}"));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Published"], item.Package.Published ? 24 + L["Hour"] : $" {item.Package.PublishFrom} " + " - " + $" {item.Package.PublishTo}"));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Status"], L[$"{item.Package.Status.ToString()}"]));

                tableData.Rows.Add(rowDef);
            }
            itemData = tableData;
            await Task.Yield();
        }

        private async Task HandleAssign(string packageId)
        {
            var result = await CustomerUnitPackageAppService.AssignPackageToCustomerAsync(int.Parse(packageId), Guid.Parse(CustomerId));
            if (result.IsSuccess)
            {
                Snackbar.Add(L["Success"], Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add(string.Join(", ", result.Errors), Severity.Error);
            }
        }
    }
}
