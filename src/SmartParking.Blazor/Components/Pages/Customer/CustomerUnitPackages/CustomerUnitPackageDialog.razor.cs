using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Cameras;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Currency;
using SmartParking.CustomerPackages;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.UnitPackages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

public partial class CustomerUnitPackageDialog
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
    public IOrganizationsAppService OrganizationsAppService { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    

    private int TotalItem;
    private TableData itemData = new TableData();
    private CustomerDto customer = new CustomerDto();
    private OrganizationDto organization = new OrganizationDto();
    private void Cancel() => MudDialog.Cancel();

    protected async override Task OnInitializedAsync()
    {

    }
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
        customer = await CustomersAppService.GetAsync(Guid.Parse(CustomerId));
        organization = await OrganizationsAppService.GetOrganizationForCurrentTenatAsync();
        var result = await CustomerUnitPackageAppService.GetListOfUnitPackageByCustomer(Guid.Parse(CustomerId) , organization.Id);
        await CreateTable(result);
    }

    private async Task CreateTable(List<CustomerUnitPackageDto> packages)
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["NumberOfVehicles"], Name = "NumberOfVehicles", IsFilterable = false, PropertyName = "NumberOfVehicles", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["NumberOfSlots"], Name = "NumberOfSlots", IsFilterable = false, PropertyName = "NumberOfSlots", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Status"], Name = "Status", IsFilterable = false, PropertyName = "CustomerCategory", IsSortable = false, TypeFilter = TypeFilter.Text });

        foreach (var item in packages)
        {
            if (item.Status == Enums.Status.Deleted)
                continue;
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
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["PackageDuration"], item.DurationOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["ActivationDate"], item.ActivationDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["ExpiryDate"], item.DectivationDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["NumberOfVehicles"], item.NumberOfVehicles.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["NumberOfSlots"], item.NumberOfSlots.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Published"], item.PublishTo==item.PublishFrom ? 24 + L["Hour"] : $" {item.PublishFrom} " + " - " + $" {item.PublishTo}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Status"], L[item.CustomerPackageStatus.ToString()]));

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }

    private async Task OpenAssignPackage()
    {
        var parameters = new DialogParameters
        {
            ["Customer"] = customer,
        };

        // Define the size and options here
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true
        };

        await Dialog.ShowAsync<AssignCustomerUnitPackageDialog>(
            "Assign Customer Package",
            parameters,
            options); // Pass the options as the third argument
    }


    private async Task RemovePackage(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var packageName = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = packageName,
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete Package", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                var customerPackage = await CustomerUnitPackageAppService.GetAsync(id);
                var unitVehiclePackage = await UnitVehiclePackageAppService.GetByCustomerUnitPackageId(customerPackage.Id);
                await CustomerUnitPackageAppService.DeleteAsync(id);
                await UnitVehiclePackageAppService.DeleteByCustomerUnitPackage(id);
                await CustomerPackagesAppService.DeleteAsync(unitVehiclePackage.CustomerId, unitVehiclePackage.EnPlateNumber);
                Snackbar.Add(L["PackageDeletedSuccessfully"], Severity.Success);
                StateHasChanged();
                NavigationManager.Refresh();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
