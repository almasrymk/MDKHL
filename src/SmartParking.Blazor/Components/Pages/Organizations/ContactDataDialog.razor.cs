using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.CustomerCategories;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SmartParking.Organizations;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class ContactDataDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    [Parameter] public int organizationId { get; set; }
    private void Cancel() => MudDialog.Cancel();

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public ContactDataDialog(IStringLocalizer<SmartParkingResource> localizer)
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

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        Console.WriteLine(RowPerPage);
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of organizations from the server
        var contactDto = await ContactInfoAppService.GetContactInfoByOrganization(input,organizationId);
        TotalItem = (int)contactDto.TotalCount;
        CreateTable(contactDto);

    }

    private void CreateTable(PagedResultDto<ContactInfoDto> garages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Contact Data",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = false, PropertyNmae = "Name", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PhoneNum"], Name = "PhoneNum", IsFilterable = false, PropertyNmae = "PhoneNum", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["WhatsAppNum"], Name = "WhatsAppNum", IsFilterable = false, PropertyNmae = "WhatsAppNum", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ContactMail"], Name = "ContactMail", IsFilterable = false, PropertyNmae = "ContactMail", IsSortable = false, TypeFilter = TypeFilter.Text });

        foreach (var item in garages.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PhoneNum"], item.PhoneNum));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["WhatsAppNum"], item.PhoneNum));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ContactMail"], item.PhoneNum));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
}
