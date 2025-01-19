using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Customers;
using SmartParking.Localization;
using SmartParking.Vehicles;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.CustomerPackages;
using SmartParking.Blazor.Components.Shared;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;

public partial class ViewCustomerPackageDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    [Parameter] public string CustomerId { get; set; }
    private void Cancel() => MudDialog.Cancel();
    private int TotalItem;
    private TableData itemData = new TableData();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public ViewCustomerPackageDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
        PagedResultDto<CustomerPackageDto> customerPackageDto;
        if (CurrentTenant.Id == null)
        {
            customerPackageDto = await CustomerPackagesAppService.GetListPackagesByCustomerMdkl(Guid.Parse(CustomerId), input);
        }
        else
        {
            customerPackageDto = await CustomerPackagesAppService.GetListPackagesByCustomerOrganization(Guid.Parse(CustomerId), input);
        }
        TotalItem =(int) customerPackageDto.TotalCount;


        CreateTable(customerPackageDto.Items.ToList());

    }

    private void CreateTable(List<CustomerPackageDto> customerPackages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "customer package Data",
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PriceOfPackage"], Name = "PriceOfPackage", IsFilterable = false, PropertyName = "PriceOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PackageDuration"], Name = "DurationOfPackage", IsFilterable = false, PropertyName = "DurationOfPackage", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ActivationDate"], Name = "ActivationDate", IsFilterable = false, PropertyName = "ActivationDate", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ExpiryDate"], Name = "ExpiryDate", IsFilterable = false, PropertyName = "ExpiryDate", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = false, TypeFilter = TypeFilter.Text });
        
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
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PriceOfPackage"], item.PriceOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PackageDuration"], item.DurationOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ActivationDate"], item.ActivationDateOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ExpiryDate"], item.ExpiryDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Published"], (bool)item.publishedOfPackage ? 24 + _localizer[$"Hour"] : $" {item.PublishFromOfPackage} " + " - " + $" {item.PublishToOfPackage}"));



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private async Task<IDialogReference> HandleAdd()
    {

        var customer = await CustomerAppService.GetAsync(Guid.Parse(CustomerId));
        var parameters = new DialogParameters
        {
            ["Customer"] = customer,
        };


        return await DialogService.ShowAsync<AssignPackgeDialog>("view customer package", parameters);
    }

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
                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS)+" "+
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE)+ " "+
                    LocalizationHelper.GetLocalizedString("Package"), Severity.Success);

                await Task.Delay(1000);

                Navigation.Refresh(true);

            }


        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }

    }
}
