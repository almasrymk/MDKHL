using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Blazor.Components.Pages.Countries;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Garages;
using System.Security.Authentication;
using Volo.Abp;
using SmartParking.Permissions.GaragePermissions;
using SmartParking.Permissions.ContactPermissions;
using SmartParking.Blazor.Components.Shared;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Permissions.CameraPermissions;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

/// <summary>
/// Represents the contact data component that handles the management of contact information.
/// </summary>
public partial class ContactData
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10; 
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactData"/> class.
    /// </summary>
    /// <param name="localizer">The localizer for string resources.</param>
    public ContactData(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = ContactPermissions.Create;
        UpdatePolicyName = ContactPermissions.Edit;
        DeletePolicyName = ContactPermissions.Delete;
    }

    /// <summary>
    /// Called when the component is initialized. Checks the user's authorization to access the contact data page.
    /// </summary>
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(ContactPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Filters contact data on the server side based on the specified column filter.
    /// </summary>
    /// <param name="pageInfo">The page change event containing filter and pagination information.</param>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch,
            filters = pageInfo.Filters
        };
        var contactInfoDto = await ContactInfoAppService.Filter(input);
        TotalItem = (int)contactInfoDto.TotalCount;
        CreateTable(contactInfoDto);
    }

    /// <summary>
    /// Searches for contact data on the server side using the provided search term.
    /// </summary>
    /// <param name="pageInfo">The page change event containing search term and pagination information.</param>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };
        var contactInfosDto = await ContactInfoAppService.Search(input);
        TotalItem = (int)contactInfosDto.TotalCount;
        CreateTable(contactInfosDto);
    }

    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="pageInfo">The page change event containing pagination information.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
        var organization = await OrganizationsAppService.GetOrganizationForCurrentTenatAsync();
        var contactDto = await ContactInfoAppService.GetContactInfoByOrganization(input, organization.Id);
        TotalItem = (int)contactDto.TotalCount;
        CreateTable(contactDto);
    }

    /// <summary>
    /// Creates the table based on the fetched contact data.
    /// </summary>
    /// <param name="contactInfos">The contact data to display in the table.</param>
    private void CreateTable(PagedResultDto<ContactInfoDto> contactInfos)
    {
        var tableData = new TableData()
        {
            Name = "Contact Data",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = true, PropertyName = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MobileNumber"], Name = "PhoneNum", IsFilterable = true, PropertyName = "PhoneNum", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["WhatsAppNum"], Name = "WhatsAppNum", IsFilterable = true, PropertyName = "WhatsAppNum", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "ContactMail", IsFilterable = true, PropertyName = "ContactMail", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in contactInfos.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MobileNumber"], item.PhoneNum));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["WhatsAppNum"], item.WhatsAppNum));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.ContactMail));

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    /// <summary>
    /// Opens the dialog to create new contact data.
    /// </summary>
    private async Task CreateContactData()
    {
        var organization = await OrganizationsAppService.GetOrganizationForCurrentTenatAsync();
        var parameters = new DialogParameters
        {
            ["organizationId"] = organization.Id
        };
        await Dialog.ShowAsync<AddContactDataDialog>("Simple Dialog", parameters);
    }

    /// <summary>
    /// Opens the dialog to update an existing contact data entry.
    /// </summary>
    /// <param name="row">The row representing the contact data to update.</param>
    private async Task UpdateContactData(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var contactData = await ContactInfoAppService.GetAsync(id);
        var organization = await OrganizationsAppService.GetOrganizationForCurrentTenatAsync();
        var parameters = new DialogParameters
        {
            ["organizationId"] = organization.Id,
            ["ContactDataDto"] = contactData,
            ["contactDataId"] = contactData.Id
        };
        await Dialog.ShowAsync<UpdateContactDataDialog>("Simple Dialog", parameters);
    }

    /// <summary>
    /// Opens the dialog to view the details of a contact data entry.
    /// </summary>
    /// <param name="row">The row representing the contact data to view.</param>
    private async Task ViewContactData(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var contactData = await ContactInfoAppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["ContactDataDto"] = contactData
        };
        await Dialog.ShowAsync<ViewContactDataDialog>("Simple Dialog", parameters);
    }

    /// <summary>
    /// Opens the dialog to confirm and delete a contact data entry.
    /// </summary>
    /// <param name="row">The row representing the contact data to delete.</param>
    private async Task DeleteContactData(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("DeleteContactData", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await ContactInfoAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {Name}", Severity.Success);

                await Task.Delay(1000);

                Navigation.NavigateTo("/contact-data", forceLoad: true);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
    }
}
